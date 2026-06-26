using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.FlagShip.Shared.FTLDrive;

[RegisterComponent, NetworkedComponent]
public sealed partial class FTLDriveComponent : Component
{
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

    /// <summary>
    /// Components that are added when the ship enters bluespace
    /// </summary>
    [DataField]
    public ComponentRegistry? FTLComponents;

    [DataField]
    public SoundPathSpecifier SpoolUpSound = new ("/Audio/_FlagShip/Effects/Shuttle/main_drive_spoolup.ogg");

    [DataField]
    public SoundPathSpecifier SpoolDownSound = new ("/Audio/_FlagShip/Effects/Shuttle/main_drive_spooldown.ogg");

    [DataField]
    public SoundPathSpecifier EngagedLoopSound = new ("/Audio/_FlagShip/Effects/Shuttle/main_drive_loop.ogg");

    [DataField]
    public SoundPathSpecifier LoopSound = new ("/Audio/_FlagShip/Effects/Shuttle/FTL_drive_hum.ogg");

    [DataField]
    public bool IsCharging;

    [DataField]
    public bool IsEngaged;

    /// <summary>
    /// The Range the FTL allows
    /// </summary>
    [DataField]
    public float Range = 1000;
}

[Serializable, NetSerializable]
public enum FTLDriveVisuals : byte
{
    Active,
}
