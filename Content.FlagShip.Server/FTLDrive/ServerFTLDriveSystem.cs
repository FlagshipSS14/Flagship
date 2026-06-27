using Content.FlagShip.Shared.FTLDrive;
using Content.Server.Shuttles.Events;

namespace Content.FlagShip.Server.FTLDrive;

public sealed partial class ServerFTLDriveSystem : EntitySystem
{

    [Dependency] private FTLDriveSystem _ftlDrive = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShuttleFTLDriveComponent, FTLStartedEvent>(OnFTLStarted);
        SubscribeLocalEvent<ShuttleFTLDriveComponent, FTLCompletedEvent>(OnFTLCompleted);
    }

    private void OnFTLCompleted(Entity<ShuttleFTLDriveComponent> ent, ref FTLCompletedEvent args)
    {
        _ftlDrive.ShutDownFTLDrive(ent.Comp.FTLDriveEntity, ent.Comp.FTLDriveEntity.Comp.CoolDownTime);

        if (ent.Comp.FTLDriveEntity.Comp.FTLComponents is not null)
            EntityManager.RemoveComponents(ent.Comp.FTLDriveEntity.Owner, ent.Comp.FTLDriveEntity.Comp.FTLComponents);
    }

    private void OnFTLStarted(Entity<ShuttleFTLDriveComponent> ent, ref FTLStartedEvent args)
    {
        if (ent.Comp.FTLDriveEntity.Comp.FTLComponents is not null)
            EntityManager.AddComponents(ent.Comp.FTLDriveEntity.Owner, ent.Comp.FTLDriveEntity.Comp.FTLComponents);

        ent.Comp.FTLDriveEntity.Comp.State = FTLDriveState.InWarp;
        Dirty(ent.Comp.FTLDriveEntity);
    }
}
