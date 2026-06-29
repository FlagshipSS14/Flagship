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

        var breakDownSeconds = Math.Clamp(stats.CoolDownFailureTime.Seconds, 0, 999);
        var breakDownMinutes = Math.Clamp(stats.CoolDownFailureTime.Minutes, 0, 999);

        var coolDownSeconds = Math.Clamp(stats.CoolDownFinishedTime.Seconds, 0, 999);
        var coolDownMinutes = Math.Clamp(stats.CoolDownFinishedTime.Minutes, 0, 999);

        var formattedBreakDown = string.Format("{0:00}:{1:00}", breakDownMinutes, breakDownSeconds);
        var formattedCoolDown = string.Format("{0:00}:{1:00}", coolDownMinutes, coolDownSeconds);

        _menu.StatusText.Text = Loc.GetString("ftl-menu-status", ("Status", stats.State.ToString().ToUpper()));
        _menu.StatsRange.Text = Loc.GetString("ftl-menu-range", ("Range", stats.Range));
        _menu.StatsCooldown.Text = Loc.GetString("ftl-menu-cooldown", ("CoolTime", stats.CoolDown));
        _menu.StatsStartup.Text = Loc.GetString("ftl-menu-startup", ("StartTime", stats.StartUp));
        _menu.StatsStableTime.Text = Loc.GetString("ftl-menu-stable", ("StableTime", stats.StableTime));
        _menu.CoolDownTimeLeft.Text = Loc.GetString("ftl-menu-cooldown-time", ("FinishedTime", formattedCoolDown));
        _menu.TimeTillCoolFailure.Text = Loc.GetString("ftl-menu-cooling-failure", ("FailTime", formattedBreakDown));
        _menu.CurrentPowerDraw.Text = Loc.GetString("ftl-menu-power-draw", ("Draw", stats.PowerDraw));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_menu is null || state is not FTLDriveBuiState status)
            return;

        // Do rounding and clamp and formatting.

        var breakDownSeconds = Math.Clamp(status.DriveData.CoolDownFailureTime.Seconds, 0, 999);
        var breakDownMinutes = Math.Clamp(status.DriveData.CoolDownFailureTime.Minutes, 0, 999);

        var coolDownSeconds = Math.Clamp(status.DriveData.CoolDownFinishedTime.Seconds, 0, 999);
        var coolDownMinutes = Math.Clamp(status.DriveData.CoolDownFinishedTime.Minutes, 0, 999);

        var formattedBreakDown = string.Format("{0:00}:{1:00}", breakDownMinutes, breakDownSeconds);
        var formattedCoolDown = string.Format("{0:00}:{1:00}", coolDownMinutes, coolDownSeconds);

        // Set the UI text

        _menu.StatusText.Text = Loc.GetString("ftl-menu-status", ("Status", status.DriveData.State.ToString().ToUpper()));
        _menu.CoolDownTimeLeft.Text = Loc.GetString("ftl-menu-cooldown-time", ("FinishedTime", formattedCoolDown));
        _menu.TimeTillCoolFailure.Text = Loc.GetString("ftl-menu-cooling-failure", ("FailTime", formattedBreakDown));
        _menu.CurrentPowerDraw.Text = Loc.GetString("ftl-menu-power-draw", ("Draw", status.DriveData.PowerDraw));
    }
}
