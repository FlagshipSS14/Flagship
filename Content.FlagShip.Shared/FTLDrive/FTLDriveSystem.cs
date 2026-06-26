using Content.FlagShip.Common.FTLDrive;
using Content.Shared.Audio;
using Content.Shared.Interaction;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.FlagShip.Shared.FTLDrive;

public sealed partial class FTLDriveSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FTLDriveComponent, ActivateInWorldEvent>(OnInteract);
        SubscribeLocalEvent<ShuttleFTLDriveComponent, GetFTLDriveRangeEvent>(OnGetRange);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ActiveFTLDriveComponent, FTLDriveComponent>();
        while (query.MoveNext(out var uid, out _, out var drive))
        {
            if (drive.IsEngaged && curTime > drive.EngagedBreakdownTime)
                BreakDownFTLDrive((uid, drive));

            if (drive.IsCharging && curTime > drive.StartUpFinishTime)
                FinishChargingFTLDrive((uid, drive));
        }
    }

    private void OnGetRange(Entity<ShuttleFTLDriveComponent> ent, ref GetFTLDriveRangeEvent args)
    {
        args.Range = ent.Comp.Range;
    }

    private void OnInteract(Entity<FTLDriveComponent> ent, ref ActivateInWorldEvent args)
    {
        TryToStartupFTLDrive(ent);
    }

    public void TryToStartupFTLDrive(Entity<FTLDriveComponent> ent)
    {
        if (ent.Comp.IsCharging || _timing.CurTime < ent.Comp.CoolDownFinishedTime)
            return;

        if (ent.Comp.IsEngaged)
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
        ent.Comp.IsCharging = true;

        ent.Comp.StartUpFinishTime = _timing.CurTime + ent.Comp.StartUpTime;

        _appearance.SetData(ent.Owner, FTLDriveVisuals.Active, true);

        EnsureComp<ActiveFTLDriveComponent>(ent.Owner);
    }

    public void ShutDownFTLDrive(Entity<FTLDriveComponent> ent, TimeSpan coolDownTime)
    {
        if (ent.Comp.SoundEntity is not null)
            _audio.Stop(ent.Comp.SoundEntity);

        if (TryComp<AmbientSoundComponent>(ent.Owner, out var ambient))
            ambient.Sound = ent.Comp.LoopSound;

        ent.Comp.SoundEntity = _audio.PlayPredicted(ent.Comp.SpoolDownSound, ent.Owner, ent.Owner)?.Entity;

        ent.Comp.IsCharging = false;
        ent.Comp.IsEngaged = false;

        ent.Comp.CoolDownFinishedTime = _timing.CurTime + coolDownTime;

        _appearance.SetData(ent.Owner, FTLDriveVisuals.Active, false);

        if (ent.Comp.FTLComponents is not null)
            EntityManager.RemoveComponents(ent.Owner, ent.Comp.FTLComponents);

        var shuttleUid = Transform(ent.Owner).GridUid;

        if (shuttleUid is not null)
        {
            RemComp<ShuttleFTLDriveComponent>(shuttleUid.Value);
        }

        RemComp<ActiveFTLDriveComponent>(ent.Owner);
    }

    public void FinishChargingFTLDrive(Entity<FTLDriveComponent> ent)
    {
        ent.Comp.IsEngaged = true;
        ent.Comp.IsCharging = false;

        var shuttleUid = Transform(ent.Owner).GridUid;

        if (shuttleUid is not null)
        {
            var shuttleDriveComponent = EnsureComp<ShuttleFTLDriveComponent>(shuttleUid.Value);

            shuttleDriveComponent.Range = ent.Comp.Range;
        }

        if (ent.Comp.FTLComponents is not null)
            EntityManager.AddComponents(ent.Owner, ent.Comp.FTLComponents);

        if (TryComp<AmbientSoundComponent>(ent.Owner, out var ambient))
            ambient.Sound = ent.Comp.EngagedLoopSound;

        ent.Comp.EngagedBreakdownTime = _timing.CurTime + ent.Comp.StableEngagedTime;
    }

    public void BreakDownFTLDrive(Entity<FTLDriveComponent> ent)
    {
        ShutDownFTLDrive(ent, ent.Comp.CoolDownTimeBreakDown);
    }
}
