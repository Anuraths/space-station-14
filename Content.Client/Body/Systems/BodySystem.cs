using Content.Client.Alerts;
using Content.Shared.Body.Components;
using Content.Shared.Body.Damage.Components;
using Content.Shared.Body.Damage.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.FixedPoint;
using Robust.Client.GameObjects;

namespace Content.Client.Body.Systems;

public sealed class BodySystem : SharedBodySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly BodyDamageThresholdsSystem _thresholds = default!;

    private const int SegmentCount = 7;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private void OnUpdateAlert(EntityUid uid, BodyComponent component, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != component.Alert)
            return;

        var parts = GetBodyChildren(uid, component);

        HashSet<BodyPart> foundParts = [];

        foreach (var (currentPart, currentPartComp) in parts)
        {
            var relative = _thresholds.RelativeToState(currentPart, BodyDamageState.Dead);
            var bodyPart = new BodyPart(currentPartComp.PartType, currentPartComp.Symmetry);
            var layer = BodyPartToLayer(bodyPart);

            if (layer == BodyPartLayer.None)
                continue;

            float offset;

            if (relative <= FixedPoint2.Zero)
            {
                offset = SegmentCount;
            }
            else
            {
                // 1 indexed
                // we programming in lua or some shit??
                var percentage = (float)(1 / relative);
                offset = SegmentCount * percentage;

                if (offset < 0)
                    offset = 1;
                else if (offset > SegmentCount - 1 && offset < SegmentCount)
                    offset = SegmentCount - 1; // reserve highest for dead only
                else
                    offset = (uint)Math.Ceiling(offset) + 1;
            }

            var state = $"{layer}{offset}";
            _sprite.LayerSetRsiState(args.SpriteViewEnt.AsNullable(), layer, state);

            foundParts.Add(bodyPart);
        }

        foreach (var (bodyPart, layer) in component.AlertLayers)
        {
            var matchFound = false;

            foreach (var foundPart in foundParts)
            {
                if (bodyPart.Type != foundPart.Type || bodyPart.Side != foundPart.Side)
                    continue;

                matchFound = true;
            }

            if (matchFound)
                continue;

            var state = $"{layer}Removed";
            _sprite.LayerSetRsiState(args.SpriteViewEnt.AsNullable(), layer, state);
        }
    }
}
