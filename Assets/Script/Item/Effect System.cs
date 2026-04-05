using UnityEngine;

public enum EffectType { Buff, Debuff, Instant, DotDamage};

public abstract class StatusEffect : ScriptableObject
{
    public string Name; 
    public float Duration;
    public EffectType Type;

    public abstract void Apply(GameObject target);
    public abstract void Remove(GameObject target);
}

[CreateAssetMenu(menuName = "Effects/StatModifier")]
public class StatEffect : StatusEffect
{
    public float Amount;
    public StatModType ModType;

    public override void Apply(GameObject target)
    {
        var stats = target.GetComponent<PlayerStatModifier>();
        if (stats != null)
        {
            stats.MoveSpeed.AddModifier(new StatModifier(Amount, ModType, this));
        }
    }

    public override void Remove(GameObject target)
    {
        var stats = target.GetComponent<PlayerStatModifier>();
        if (stats != null)
        {
            stats.MoveSpeed.RemoveAllModifiersFromSource(this);
        }
    }
}

[CreateAssetMenu(menuName = "Effects/DotDamage")]
public class DotEffect : StatusEffect
{
    public float DamagePerSecond;

    public override void Apply(GameObject target) { }

    public override void Remove(GameObject target) { }
}