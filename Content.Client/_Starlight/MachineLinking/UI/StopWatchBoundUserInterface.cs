using Content.Shared._Starlight.MachineLinking;
using Content.Shared.MachineLinking;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;

namespace Content.Client._Starlight.MachineLinking.UI;

public sealed class StopWatchBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private StopWatchWindow? _window;

    public StopWatchBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<StopWatchWindow>();
        _window.OnStartStopwatch += () => SendMessage(new StopWatchStartMessage());
        _window.OnStopStopwatch += () => SendMessage(new StopWatchStopMessage());
        _window.OnResetStopwatch += () => SendMessage(new StopWatchResetMessage());

    }

    /// <summary>
    /// Update the UI state based on server-sent info
    /// </summary>
    /// <param name="state"></param>
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not StopWatchBoundUserInterfaceState cast)
            return;

        _window.TriggerTime = cast.TriggerTime;
        _window.Accumulated = cast.AccumulatedTime;
        _window.StopwatchRunning = cast.StopWatchRunning;

    }
}
