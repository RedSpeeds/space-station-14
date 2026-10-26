using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Starlight.TeleScience;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class CoordinateServerComponent : Component
{
    [DataField] public List<int> Offers = new();

    [DataField] public int OfferCount;

    [DataField] public List<EntityUid>? ResolvedCoordinates = new();

    [DataField] public TimeSpan OfferCooldown = TimeSpan.FromMinutes(5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextOffer;


}

