using Content.Server.Access.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Station.Systems;
using Content.Server.StationRecords.Systems;
using Content.Shared._Starlight.Abstract;
using Content.Shared._Starlight.Railroading.Components;
using Content.Shared._Starlight.Railroading.Components.Handlers;
using Content.Shared._Starlight.Railroading.Events;
using Content.Shared.Chat;
using Content.Shared.Delivery;
using Content.Shared.FingerprintReader;
using Content.Shared.GameTicking;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Power.EntitySystems;
using Content.Shared.StationRecords;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Starlight.Railroading.HandlerSystem;

/// <summary>
/// This handles spawning the delivery item when a card is chosen
/// </summary>
public sealed partial class RailroadingDeliveryOnChosenSystem : AccUpdateEntitySystem
{
    [Dependency] private FingerprintReaderSystem _fingerprintReader = default!;
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private IdCardSystem _idCard = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IPrototypeManager _protoMan = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private LabelSystem _label = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedPowerReceiverSystem _power = default!;
    [Dependency] private StationRecordsSystem _records = default!;
    [Dependency] private StationSystem _station = default!;

    protected override float Threshold { get; set; } = 2.3f;

    private readonly Queue<(Entity<RailroadDeliveryOnChosenComponent> Card, Entity<RailroadableComponent> Subject)> _queue = [];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RailroadDeliveryOnChosenComponent, RailroadingCardChosenEvent>(OnChosen);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);
    }

    private void OnCleanup(RoundRestartCleanupEvent ev) => _queue.Clear();
    private void OnChosen(Entity<RailroadDeliveryOnChosenComponent> ent, ref RailroadingCardChosenEvent args)
    {
        ent.Comp.RecipientMind = null;
        // Capture recipient identity at card selection time so it does not depend on mutable display names.
        if (_mind.TryGetMind(args.Subject.Owner, out var mindId, out _))
            ent.Comp.RecipientMind = mindId;
        _queue.Enqueue((ent, args.Subject));
    }

    protected override void AccUpdate(float _)
    {
        var processed = 0;
        while (processed < 5 && _queue.TryDequeue(out var pending))
        {
            if (!TryDeliver(pending.Card, pending.Subject))
                _queue.Enqueue(pending);

            processed++;
        }
    }

    private bool TryDeliver(Entity<RailroadDeliveryOnChosenComponent> ent, Entity<RailroadableComponent> subject)
    {
        if (TerminatingOrDeleted(ent) || TerminatingOrDeleted(subject))
            return true;

        if (_station.GetOwningStation(subject) is not { } station)
            return false;

        EntityUid? spawner = null;

        var spawners = EntityQueryEnumerator<DeliverySpawnerComponent>();
        while (spawners.MoveNext(out var spawnerUid, out _))
        {
            if (_station.GetOwningStation(spawnerUid) is not { } spawnerStation)
                continue;

            if (spawnerStation != station)
                continue;

            if (!_power.IsPowered(spawnerUid))
                continue;

            spawner = spawnerUid;
            break;
        }

        if (spawner == null)
            return false;

        if (ent.Comp.RecipientMind is not { } recipientMind
            || !TryComp<MindComponent>(recipientMind, out var trackedMind)
            || string.IsNullOrWhiteSpace(trackedMind.CharacterName))
            return true;

        var delivery = SpawnAtPosition(ent.Comp.Delivery, Transform(spawner.Value).Coordinates);
        var subjectName = trackedMind.CharacterName!;
        var recordID = _records.GetRecordByName(station, subjectName);

        // Records use the name on the ID card, which can differ from the character name (e.g. a custom clown name),
        // so fall back to the record keyed on their ID, if it's filed under their name (not a stolen ID).
        if (recordID == null
            && _idCard.TryFindIdCard(subject, out var idCard)
            && TryComp<StationRecordKeyStorageComponent>(idCard, out var keyStorage)
            && keyStorage.Key is { } key
            && key.OriginStation == station
            && _records.TryGetRecord<GeneralStationRecord>(key, out var idRecord)
            && idRecord.Name == Name(subject))
        {
            recordID = key.Id;
        }

        if (ent.Comp.Dataset != null && _playerManager.TryGetSessionByEntity(subject, out var session))
        {
            var dataset = _protoMan.Index(ent.Comp.Dataset);
            var pick = _random.Pick(dataset.Values);
            if (ent.Comp.WrappedDataset != null)
            {
                var wrappedDataset = _protoMan.Index(ent.Comp.WrappedDataset.Value);
                _chat.ChatMessageToOne(ChatChannel.Notifications, Loc.GetString(pick), Loc.GetString(wrappedDataset.Values[dataset.Values.IndexOf(pick)]), default, false, session.Channel, Color.FromHex("#FF84FF"));
            }
        }

        if (!TryComp<DeliveryComponent>(delivery, out var deliveryComp))
            return true;

        deliveryComp.RecipientName = subjectName;
        deliveryComp.RecipientStation = station;

        if (recordID != null
            && _records.TryGetRecord<GeneralStationRecord>(_records.Convert((GetNetEntity(station), recordID.Value)), out var entry))
        {
            deliveryComp.RecipientName = entry.Name;
            deliveryComp.RecipientJobTitle = entry.JobTitle;
            _appearance.SetData(delivery, DeliveryVisuals.JobIcon, entry.JobIcon);

            if (TryComp<FingerprintReaderComponent>(delivery, out var reader) && entry.Fingerprint != null)
                _fingerprintReader.AddAllowedFingerprint((delivery, reader), entry.Fingerprint);
        }

        _label.Label(delivery, deliveryComp.RecipientName);

        Dirty(delivery, deliveryComp);
        return true;
    }
}
