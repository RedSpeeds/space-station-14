using Content.Server._Starlight.DeviceLinking.Components;
using Content.Shared._Starlight.MachineLinking;
using Content.Shared.TextScreen;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._Starlight.DeviceLinking.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class StopWatchSystem : EntitySystem
{
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    private List<Entity<StopWatchComponent>> _stopwatches = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StopWatchComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<StopWatchComponent, AfterActivatableUIOpenEvent>(OnUiOpen);

        SubscribeLocalEvent<StopWatchComponent, StopWatchStartMessage>(OnStart);
        SubscribeLocalEvent<StopWatchComponent, StopWatchStopMessage>(OnStop);
        SubscribeLocalEvent<StopWatchComponent, StopWatchResetMessage>(OnReset);
    }

    private void OnStop(Entity<StopWatchComponent> ent, ref StopWatchStopMessage args)
    {
        throw new NotImplementedException();
    }

    private void OnReset(Entity<StopWatchComponent> ent, ref StopWatchResetMessage args)
    {
        throw new NotImplementedException();
    }

    private void OnStart(Entity<StopWatchComponent> ent, ref StopWatchStartMessage args)
    {
        TryComp<AppearanceComponent>(ent, out var appearance);
        ent.Comp.TriggerTime = _gameTiming.CurTime;

    }

    private void OnUiOpen(Entity<StopWatchComponent> ent, ref AfterActivatableUIOpenEvent args)
    {
        if (_ui.HasUi(ent, StopWatchUiKey.Key))
        {
            _ui.SetUiState(ent.Owner, StopWatchUiKey.Key, new StopWatchBoundUserInterfaceState(ent.Comp.StopWatchRunning, ent.Comp.TriggerTime, ent.Comp.Accumulated));
        }
    }

    private void OnInit(Entity<StopWatchComponent> ent, ref ComponentInit args)
    {
        _appearanceSystem.SetData(ent, TextScreenVisuals.DefaultText, "0:00");
        _appearanceSystem.SetData(ent, TextScreenVisuals.ScreenText, "0:00");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateStopwatch();
    }

    private void UpdateStopwatch()
    {
        var query = EntityQueryEnumerator<StopWatchComponent>();
        while (query.MoveNext(out var uid, out var stopwatch))
        {
            var elapsed = stopwatch.Accumulated;
            if (stopwatch.StopWatchRunning && stopwatch.TriggerTime != null)
            {
                var delta = _gameTiming.CurTime - stopwatch.TriggerTime;
                if (delta > TimeSpan.Zero)
                    elapsed += delta;
            }
            _appearanceSystem.SetData(uid, TextScreenVisuals.ScreenText, FormatElapsed(elapsed));
        }
    }
    private static string FormatElapsed(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
            elapsed = TimeSpan.Zero;

        var minutes = (int)elapsed.TotalMinutes;
        var seconds = elapsed.Seconds;
        return $"{minutes:D2}:{seconds:D2}";
    }
}
