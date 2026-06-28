using Content.FlagShip.Common.FTLDrive;
using Content.Shared.Audio;
using Content.Shared.Explosion.EntitySystems;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.FlagShip.Shared.FTLDrive;

public sealed partial class FTLDriveSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedExplosionSystem _explosion = default!;
    [Dependency] private SharedPowerStateSystem _powerState = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShuttleFTLDriveComponent, GetFTLDriveRangeEvent>(OnGetRange);
        SubscribeLocalEvent<FTLDriveComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnGetRange(Entity<ShuttleFTLDriveComponent> ent, ref GetFTLDriveRangeEvent args)
    {
        args.Range = ent.Comp.Range;
    }

    private void OnPowerChanged(Entity<FTLDriveComponent> ent, ref PowerChangedEvent args)
    {
        if (args.Powered)
            return;

        ent.Comp.NextPowerCheckTime = _timing.CurTime + ent.Comp.PowerGracePeriod;
    }

    public void TryToStartupFTLDrive(Entity<FTLDriveComponent> ent)
    {
        if (ent.Comp.State is not FTLDriveState.Idle && ent.Comp.State is not FTLDriveState.Engaged)
            return;

        if  (_timing.CurTime < ent.Comp.CoolDownFinishedTime)
            return;

        if (ent.Comp.State == FTLDriveState.Engaged)
        {
            ShutDownFTLDrive(ent, ent.Comp.CoolDownTime);
            return;
        }

        StartUpFTLDrive(ent);
    }

    public void StartUpFTLDrive(Entity<FTLDriveComponent> ent)
    {
        if (ent.Comp.SoundEntity is not null)
            _audio.Stop(ent.Comp.SoundEntity);

        ent.Comp.SoundEntity = _audio.PlayPredicted(ent.Comp.SpoolUpSound, ent.Owner, ent.Owner)?.Entity;
        ent.Comp.State = FTLDriveState.Charging;

        ent.Comp.StartUpFinishTime = _timing.CurTime + ent.Comp.StartUpTime;

        _appearance.SetData(ent.Owner, FTLDriveVisuals.Active, true);

        _powerState.SetWorkingState(ent.Owner, true);

        Dirty(ent);
        EnsureComp<ActiveFTLDriveComponent>(ent.Owner);
    }

    public void ShutDownFTLDrive(Entity<FTLDriveComponent> ent, TimeSpan coolDownTime)
    {
        _powerState.SetWorkingState(ent.Owner, false);

        if (ent.Comp.SoundEntity is not null)
            _audio.Stop(ent.Comp.SoundEntity);

        if (TryComp<AmbientSoundComponent>(ent.Owner, out var ambient))
        {
            ambient.Sound = ent.Comp.LoopSound;
            Dirty(ent.Owner, ambient);
        }

        ent.Comp.SoundEntity = _audio.PlayPredicted(ent.Comp.SpoolDownSound, ent.Owner, ent.Owner)?.Entity;

        ent.Comp.State = FTLDriveState.Idle;

        ent.Comp.CoolDownFinishedTime = _timing.CurTime + coolDownTime;

        _appearance.SetData(ent.Owner, FTLDriveVisuals.Active, false);

        if (ent.Comp.FTLComponents is not null)
            EntityManager.RemoveComponents(ent.Owner, ent.Comp.FTLComponents);

        if (ent.Comp.EngagedComponents is not null)
            EntityManager.RemoveComponents(ent.Owner, ent.Comp.EngagedComponents);

        var shuttleUid = Transform(ent.Owner).GridUid;

        if (shuttleUid is not null)
        {
            RemComp<ShuttleFTLDriveComponent>(shuttleUid.Value);
        }

        ent.Comp.EngagedBreakdownTime = _timing.CurTime;
        RemComp<ActiveFTLDriveComponent>(ent.Owner);
    }

    public void FinishChargingFTLDrive(Entity<FTLDriveComponent> ent)
    {
        ent.Comp.State = FTLDriveState.Engaged;

        var shuttleUid = Transform(ent.Owner).GridUid;

        if (shuttleUid is not null)
        {
            if (!HasComp<ShuttleFTLDriveComponent>(shuttleUid.Value))
            {
                var shuttleDriveComponent = EnsureComp<ShuttleFTLDriveComponent>(shuttleUid.Value);

                shuttleDriveComponent.Range = ent.Comp.Range;
                shuttleDriveComponent.FTLDriveEntity = ent;
                Dirty(shuttleUid.Value, shuttleDriveComponent);
            }
            else
            {
                Log.Warning("Tried to add ShuttleFTLDriveComponent to: " + shuttleUid + " But already had a ShuttleFTLDriveComponent!");
            }
        }

        if (ent.Comp.EngagedComponents is not null)
            EntityManager.AddComponents(ent.Owner, ent.Comp.EngagedComponents);

        if (TryComp<AmbientSoundComponent>(ent.Owner, out var ambient))
        {
            ambient.Sound = ent.Comp.EngagedLoopSound;
            Dirty(ent.Owner, ambient);
        }

        ent.Comp.EngagedBreakdownTime = _timing.CurTime + ent.Comp.StableEngagedTime;
    }

    public void BreakDownFTLDrive(Entity<FTLDriveComponent> ent)
    {
        if (ent.Comp.State is not FTLDriveState.Engaged)
            return;

        ShutDownFTLDrive(ent, ent.Comp.CoolDownTimeBreakDown);
        _explosion.QueueExplosion(ent.Owner, ent.Comp.ExplosionType, ent.Comp.TotalIntensity, ent.Comp.IntensitySlope, ent.Comp.MaxTileBreak, canCreateVacuum:true);
    }

    public FTLDriveStatsData GetDriveStats(EntityUid uid)
    {
        var data = new FTLDriveStatsData();

        if (!TryComp<FTLDriveComponent>(uid, out var drive))
            return data;

        data.State = drive.State;
        data.CoolDown = drive.CoolDownTime;
        data.Range = drive.Range;
        data.StableTime = drive.StableEngagedTime;
        data.StartUp = drive.StartUpTime;

        return data;
    }

    public FTLDriveData GetDriveStatus(EntityUid uid)
    {
        var data = new FTLDriveData();

        if (!TryComp<FTLDriveComponent>(uid, out var drive))
            return data;

        data.State = drive.State;
        data.CoolDownFinishedTime = drive.CoolDownFinishedTime - _timing.CurTime;
        data.CoolDownFailureTime = drive.EngagedBreakdownTime - _timing.CurTime;

        // I can't find how to get the power it's actually using in shared soo I guess I'll just use this
        if (TryComp<PowerStateComponent>(uid, out var power))
        {
            if (power.IsWorking)
                data.PowerDraw = power.WorkingPowerDraw;
            else
                data.PowerDraw = power.IdlePowerDraw;
        }

        return data;
    }

}
