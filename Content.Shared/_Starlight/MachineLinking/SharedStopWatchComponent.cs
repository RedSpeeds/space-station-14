using Robust.Shared.Serialization;

namespace Content.Shared._Starlight.MachineLinking
{

    [Serializable, NetSerializable]
    public enum StopWatchUiKey : byte
    {
        Key
    }

    [Serializable, NetSerializable]
    public sealed class StopWatchBoundUserInterfaceState : BoundUserInterfaceState
    {
        public bool StopWatchRunning;
        public TimeSpan TriggerTime;
        public TimeSpan AccumulatedTime;

        public StopWatchBoundUserInterfaceState(bool stopWatchRunning, TimeSpan triggerTime, TimeSpan accumulatedTime)
        {
            StopWatchRunning = stopWatchRunning;
            TriggerTime = triggerTime;
            AccumulatedTime = accumulatedTime;
        }
    }

    [Serializable, NetSerializable]
    public sealed class StopWatchStartMessage : BoundUserInterfaceMessage;

    [Serializable, NetSerializable]
    public sealed class StopWatchStopMessage : BoundUserInterfaceMessage;

    [Serializable, NetSerializable]
    public sealed class StopWatchResetMessage : BoundUserInterfaceMessage;

}
