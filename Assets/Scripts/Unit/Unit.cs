using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitData unitData;
    [SerializeField] private SkillData[] skillSlots = new SkillData[3];

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
        if (unitData == null)
        {
            currentHP = 0;
            isAlive = false;
            return;
        }

        currentHP = unitData.maxHP;
        isAlive = currentHP > 0;
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
