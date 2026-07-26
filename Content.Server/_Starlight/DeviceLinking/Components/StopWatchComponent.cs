using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Starlight.DeviceLinking.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class StopWatchComponent : Component
{
    [DataField("triggerTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan TriggerTime;
    [DataField("triggerTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan Accumulated;
    [DataField]
    public bool StopWatchRunning;
}
