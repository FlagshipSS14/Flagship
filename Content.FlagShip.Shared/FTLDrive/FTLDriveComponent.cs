using Content.Shared.Explosion;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.FTLDrive;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FTLDriveComponent : Component
{
    /// <summary>
    /// The Range the FTL allows
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 5000;

    [DataField, AutoNetworkedField]
    public FTLDriveState State;

    /// <summary>
    /// How long it takes for the drive to cool down when a breakdown happens (from being engaged too long)
    /// </summary>
    [DataField]
    public TimeSpan CoolDownTimeBreakDown = TimeSpan.FromSeconds(640);

    /// <summary>
    /// How long it takes for the drive to cool down normally
    /// </summary>
    [DataField]
    public TimeSpan CoolDownTime = TimeSpan.FromSeconds(320);

    /// <summary>
    /// How long it takes for the drive to startup
    /// </summary>
    [DataField]
    public TimeSpan StartUpTime = TimeSpan.FromSeconds(25);

    /// <summary>
    /// How long the drive can stay Engaged until it breaks down.
    /// </summary>
    [DataField]
    public TimeSpan StableEngagedTime = TimeSpan.FromSeconds(60);

    /// <summary>
    /// When this drive will finish startup
    /// </summary>
    [DataField]
    public TimeSpan StartUpFinishTime;

    /// <summary>
    /// When this drive will break down
    /// </summary>
    [DataField]
    public TimeSpan EngagedBreakdownTime;

    /// <summary>
    /// When this drive will be cooled down
    /// </summary>
    [DataField]
    public TimeSpan CoolDownFinishedTime;

    [DataField]
    public EntityUid? SoundEntity;

    [DataField]
    public ProtoId<ExplosionPrototype> ExplosionType = "Radioactive";

    [DataField]
    public float IntensitySlope = 1;

    [DataField]
    public float TotalIntensity = 100;

    [DataField]
    public float TileBreakScale = 20f;

    [DataField]
    public int MaxTileBreak = int.MaxValue;

    /// <summary>
    /// Components that are added when the ship enters bluespace
    /// </summary>
    [DataField]
    public ComponentRegistry? FTLComponents;

    /// <summary>
    /// Components that are added when the FTL drive is engaged
    /// </summary>
    [DataField]
    public ComponentRegistry? EngagedComponents;

    [DataField]
    public SoundPathSpecifier SpoolUpSound = new ("/Audio/_FlagShip/Effects/Shuttle/main_drive_spoolup.ogg");

    [DataField]
    public SoundPathSpecifier SpoolDownSound = new ("/Audio/_FlagShip/Effects/Shuttle/main_drive_spooldown.ogg");

    [DataField]
    public SoundPathSpecifier EngagedLoopSound = new ("/Audio/_FlagShip/Effects/Shuttle/main_drive_loop.ogg");

    [DataField]
    public SoundPathSpecifier LoopSound = new ("/Audio/_FlagShip/Effects/Shuttle/FTL_drive_hum.ogg");
}

[Serializable, NetSerializable]
public enum FTLDriveVisuals : byte
{
    Active,
}

[Serializable, NetSerializable]
public enum FTLDriveState : byte
{
    Idle,
    Charging,
    Engaged,
    InWarp,
}
