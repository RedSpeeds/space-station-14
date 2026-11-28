using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.Railroading.Components.Handlers;

/// <summary>
/// This is used for sending a delivery to the person who picked the card
/// </summary>
[RegisterComponent]
public sealed partial class RailroadDeliveryOnChosenComponent : Component
{
    [DataField] public EntProtoId Delivery;

    [DataField] public ProtoId<LocalizedDatasetPrototype>? Dataset = null;

    [DataField] public ProtoId<LocalizedDatasetPrototype>? WrappedDataset = null;

    [NonSerialized] public EntityUid? RecipientMind;

}
