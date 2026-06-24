using Content.FlagShip.Shared.ShuttleRecords;

namespace Content.FlagShip.Server.ShuttleRecords.Components;

/// <summary>
/// A component that stores records for all shuttle purchases in the sector.
/// Note: all purchases are currently added, will need to be filtered appropriately by viewing clients.
/// </summary>
[RegisterComponent]
[Access(typeof(ShuttleRecordsSystem))]
public sealed partial class SectorShuttleRecordsComponent : Component
{
    [DataField]
    public Dictionary<NetEntity, ShuttleRecord> ShuttleRecords = [];
}
