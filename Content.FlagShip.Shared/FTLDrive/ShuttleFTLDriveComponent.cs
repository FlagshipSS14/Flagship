using Robust.Shared.GameStates;

namespace Content.FlagShip.Shared.FTLDrive;

/// <summary>
/// Added to a shuttle when a warp drive fully starts up and passes in its range, removed on warp drive shut down
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShuttleFTLDriveComponent : Component
{
    /// <summary>
    /// How far the shuttle should be able to FTL
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range;

    /// <summary>
    /// The acutal FTL drive entity that is on the shuttle
    /// </summary>
    [ViewVariables]
    public Entity<FTLDriveComponent> FTLDriveEntity;
}
