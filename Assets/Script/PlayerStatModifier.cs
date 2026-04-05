using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatModType { Flat, PercentAdd, PercentMult }

[Serializable]
public class StatModifier
{
    public float Value;
    public StatModType Type;
    public object Source;

    public StatModifier(float value, StatModType type, object source)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}

[Serializable]
public class CharacterStat
{
    public float BaseValue;
    protected bool isDirty = true;
    protected float _finalValue;

    public float Value
    {
        get
        {
            if (isDirty)
            {
                _finalValue = CalculateFinalValue();
                isDirty = false;
            }
            return _finalValue;
        }
    }

    protected readonly List<StatModifier> statModifiers = new List<StatModifier>();

    public void AddModifier(StatModifier mod)
    {
        isDirty = true;
        statModifiers.Add(mod);
        statModifiers.Sort((a, b) => a.Type.CompareTo(b.Type));
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if (statModifiers.Remove(mod))
        {
            isDirty = true;
            return true;
        }
        return false;
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        bool didRemove = false;
        for (int i = statModifiers.Count - 1; i >= 0; i--)
        {
            if (statModifiers[i].Source == source)
            {
                isDirty = true;
                didRemove = true;
                statModifiers.RemoveAt(i);
            }
        }
        return didRemove;
    }

    protected virtual float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        // 1. Flat부터 싹 다 더합니다.
        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier mod = statModifiers[i];
            if (mod.Type == StatModType.Flat)
            {
                finalValue += mod.Value;
            }
        }

        // 2. PercentAdd 계산을 위해 별도로 합산
        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier mod = statModifiers[i];
            if (mod.Type == StatModType.PercentAdd)
            {
                sumPercentAdd += mod.Value;
            }
        }
        // Flat이 적용된 finalValue에 한 번에 PercentAdd를 곱합니다.
        finalValue *= (1 + sumPercentAdd);

        // 3. 마지막으로 PercentMult 적용
        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier mod = statModifiers[i];
            if (mod.Type == StatModType.PercentMult)
            {
                finalValue *= (1 + mod.Value);
            }
        }

        return (float)Math.Round(finalValue, 4);
    }
}

public class PlayerStatModifier : MonoBehaviour
{
    public CharacterStat MaxHealth = new CharacterStat();
    public CharacterStat MoveSpeed = new CharacterStat();
    public CharacterStat HealthRegen = new CharacterStat(); // 초당 회복량

    [Header("현재 스텟")]
    public float CurrentHealth;

    void Start()
    {
        MaxHealth.BaseValue = 100f; // 기본 체력
        MoveSpeed.BaseValue = 10f;  // 기본 이동속도
        HealthRegen.BaseValue = 1f; //  초당 회복 수치
        CurrentHealth = MaxHealth.Value;

        // 테스트: 5초 동안만 지속되는 이속 버프 (+50%)
        AddTimedModifier(MoveSpeed, new StatModifier(0.5f, StatModType.PercentAdd, "SpeedPotion"), 5f);
    }

    void Update()
    {
        // 2. 시간에 따른 지속 회복 (Regeneration)
        if (CurrentHealth < MaxHealth.Value)
        {
            // HealthRegen.Value(최종값)만큼 매초 부드럽게 회복
            CurrentHealth += HealthRegen.Value * Time.deltaTime;
            CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth.Value); // 최대 체력을 넘음 방지
        }
    }

    // 영구적인 체력 상승 (아이템 먹었을 때 등)
    public void IncreasePermanentMaxHealth(float amount)
    {
        MaxHealth.BaseValue += amount;
        CurrentHealth += amount; // 현재 체력도 보너스
        Debug.Log($"최대 체력 영구 상승! 현재: {MaxHealth.Value}");
    }

    public void AddTimedModifier(CharacterStat stat, StatModifier mod, float duration)
    {
        stat.AddModifier(mod);
        StartCoroutine(RemoveAfterSeconds(stat, mod, duration));
    }

    private IEnumerator RemoveAfterSeconds(CharacterStat stat, StatModifier mod, float duration)
    {
        yield return new WaitForSeconds(duration);
        stat.RemoveModifier(mod);
        Debug.Log("기간제 버프가 종료되었습니다.");
    }
}