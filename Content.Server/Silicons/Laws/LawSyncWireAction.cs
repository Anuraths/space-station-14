using Content.Server.Wires;
using Content.Shared.Doors;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Wires;
using Robust.Shared.Random;

namespace Content.Server.Silicons.Laws;

public sealed partial class LawSyncWireAction : ComponentWireAction<SiliconLawSyncedComponent>
{
    public override Color Color { get; set; } = Color.Teal;
    public override string Name { get; set; } = "wire-name-law-sync";

    public override StatusLightState? GetLightState(Wire wire, SiliconLawSyncedComponent comp)
        => comp.Syncer != null ? StatusLightState.On : StatusLightState.Off;

    public override object StatusKey { get; } = AirlockWireStatus.LawSyncIndicator;

    public override bool Cut(EntityUid user, Wire wire, SiliconLawSyncedComponent comp)
    {
        WiresSystem.TryCancelWireAction(wire.Owner, PulseTimeoutKey.Key);
        comp.Syncer = null;
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, SiliconLawSyncedComponent comp)
    {
        var query = EntityManager.EntityQueryEnumerator<SiliconCanLawSyncComponent>();
        List<EntityUid> syncers = new();

        while (query.MoveNext(out var syncer, out _))
        {
            syncers.Add(syncer);
        }

        if (syncers.Count > 0)
        {
            comp.Syncer = IoCManager.Resolve<IRobustRandom>().Pick(syncers);
        }

        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, SiliconLawSyncedComponent comp)
    {
        if (!EntityManager.TryGetComponent<SiliconLawProviderComponent>(wire.Owner, out var lawProto))
            return;

        var lawSystem = EntityManager.System<SiliconLawSystem>();

        var laws = comp.Syncer == null
            ? lawSystem.GetLawset(lawProto.Laws).Laws
            : lawSystem.GetLaws(comp.Syncer.Value).Laws;

        lawSystem.SetLaws(laws, wire.Owner);
    }

    private enum PulseTimeoutKey : byte
    {
        Key
    }
}
