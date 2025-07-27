using Content.Shared.Body.Damage.Components;
using Content.Shared.Damage;

namespace Content.Shared.Body.Damage.Systems;

public sealed partial class BodyDamageOnDamageSystem : EntitySystem
{
    [Dependency] private readonly BodyDamageableSystem _bodyDamageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyDamageOnDamageComponent, DamageChangedEvent>(OnDamage);
    }

    private void OnDamage(Entity<BodyDamageOnDamageComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;
        _bodyDamageable.ChangeDamage(ent.Owner, args.DamageDelta.GetTotal());
    }
}
