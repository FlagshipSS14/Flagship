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

        _menu.StatusText.Text = Loc.GetString("ftl-menu-status", ("Status", stats.State.ToString().ToUpper()));
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

        var breakDownSeconds = MathF.Round(status.DriveData.CoolDownFailureTime.Seconds, 2);
        var breakDownMinutes = MathF.Round(status.DriveData.CoolDownFailureTime.Minutes, 2);

        breakDownSeconds = Math.Clamp(breakDownSeconds, 0, 99999);
        breakDownMinutes = Math.Clamp(breakDownMinutes, 0, 99999);

        var formattedBreakDown = string.Format("{0:00}:{1:00}", breakDownMinutes, breakDownSeconds);

        var coolDownSeconds = MathF.Round(status.DriveData.CoolDownFinishedTime.Seconds, 2);
        var coolDownMinutes = MathF.Round(status.DriveData.CoolDownFinishedTime.Minutes, 2);

        coolDownSeconds = Math.Clamp(coolDownSeconds, 0, 99999);
        coolDownMinutes = Math.Clamp(coolDownMinutes, 0, 99999);

        var formattedCoolDown = string.Format("{0:00}:{1:00}", coolDownMinutes, coolDownSeconds);

        _menu.StatusText.Text = Loc.GetString("ftl-menu-status", ("Status", status.DriveData.State.ToString().ToUpper()));
        _menu.CoolDownTimeLeft.Text = Loc.GetString("ftl-menu-cooldown-time", ("FinishedTime", formattedCoolDown));
        _menu.TimeTillCoolFailure.Text = Loc.GetString("ftl-menu-cooling-failure", ("FailTime", formattedBreakDown));
        _menu.CurrentPowerDraw.Text = Loc.GetString("ftl-menu-power-draw", ("Draw", status.DriveData.PowerDraw));
    }
}
