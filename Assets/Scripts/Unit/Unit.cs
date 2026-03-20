using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitData unitData;
    [SerializeField] private SkillData[] skillSlots = new SkillData[3];

    private readonly List<StatusEffect> statusEffects = new List<StatusEffect>();
    private int currentHP;
    private bool isAlive = true;

    public string UnitName => unitData != null ? unitData.unitName : "미지정";
    public int CurrentHP => currentHP;
    public int MaxHP => unitData != null ? unitData.maxHP : 0;
    public float CurrentHPRatio => MaxHP > 0 ? (float)currentHP / MaxHP : 0f;
    public bool IsAlive => isAlive;
    public SkillData[] SkillSlots => skillSlots;

    public int RollSpeed()
    {
        if (unitData == null)
        {
            return 0;
        }

        return Random.Range(unitData.minSpeed, unitData.maxSpeed + 1);
    }

    public void Initialize()
    {
        statusEffects.Clear();

        if (unitData == null)
        {
            currentHP = 0;
            isAlive = false;
            return;
        }

        currentHP = unitData.maxHP;
        isAlive = currentHP > 0;
    }

    public void AddBleed(int potency, int count)
    {
        if (!isAlive || potency <= 0 || count <= 0)
        {
            return;
        }

        StatusEffect bleed = GetBleed();

        if (bleed == null)
        {
            statusEffects.Add(new StatusEffect(StatusType.Bleed, potency, count));
            return;
        }

        bleed.potency += potency;
        bleed.count += count;
    }

    public int ConsumeBleedOnAttack(int triggerCount)
    {
        if (!isAlive || triggerCount <= 0)
        {
            return 0;
        }

        StatusEffect bleed = GetBleed();

        if (bleed == null || bleed.potency <= 0 || bleed.count <= 0)
        {
            return 0;
        }

        int actualTriggerCount = Mathf.Min(triggerCount, bleed.count);
        int damage = bleed.potency * actualTriggerCount;

        TakeDamage(damage);

        bleed.count -= actualTriggerCount;

        if (bleed.count <= 0)
        {
            statusEffects.Remove(bleed);
        }

        return damage;
    }

    public StatusEffect GetBleed()
    {
        for (int i = 0; i < statusEffects.Count; i++)
        {
            if (statusEffects[i].type == StatusType.Bleed)
            {
                return statusEffects[i];
            }
        }

        return null;
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive)
        {
            return;
        }

        currentHP -= damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            isAlive = false;
        }
    }
}
