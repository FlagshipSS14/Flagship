using Content.FlagShip.Shared.FTLDrive;
using Robust.Client.UserInterface;

namespace Content.FlagShip.Client.FTLDrive;

public sealed partial class FTLDriveBoundUserInterface : BoundUserInterface
{
    [Dependency] private IEntityManager _entMan = default!;
    private FTLDriveSystem _ftlDrive = default!;
    private FTLDriveUI? _menu;

    public FTLDriveBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _ftlDrive = _entMan.System<FTLDriveSystem>();

        _menu = this.CreateWindow<FTLDriveUI>();

        var stats = _ftlDrive.GetDriveStats(Owner);

        _menu.OnFTLDriveChargeButtonPressed += () =>
        {
            SendMessage(new FTLChargeButtonPressedMessage());
        };

        _menu.StatusText.Text = Loc.GetString("ftl-menu-status", ("Status", stats.State));
        _menu.StatsRange.Text = Loc.GetString("ftl-menu-range", ("Range", stats.Range));
        _menu.StatsCooldown.Text = Loc.GetString("ftl-menu-cooldown", ("CoolTime", stats.CoolDown));
        _menu.StatsStartup.Text = Loc.GetString("ftl-menu-startup", ("StartTime", stats.StartUp));
        _menu.StatsStableTime.Text = Loc.GetString("ftl-menu-stable", ("StableTime", stats.StableTime));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_menu is null || state is not FTLDriveBuiState status)
            return;

        _menu.StatusText.Text = Loc.GetString("ftl-menu-status", ("Status", status.DriveData.State));
        _menu.CoolDownTimeLeft.Text = Loc.GetString("ftl-menu-cooldown-time", ("FinishedTime", status.DriveData.CoolDownFinishedTime));
        _menu.TimeTillCoolFailure.Text = Loc.GetString("ftl-menu-cooling-failure", ("FailTime", status.DriveData.CoolDownFailureTime));
        _menu.CurrentPowerDraw.Text = Loc.GetString("ftl-menu-power-draw", ("Draw", status.DriveData.PowerDraw));
    }
}
