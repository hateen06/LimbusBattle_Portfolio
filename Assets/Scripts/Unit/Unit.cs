using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitData unitData;
    [SerializeField] private SkillData[] skillSlots = new SkillData[3];

    private int currentHP;
    private bool isAlive = true;

    public string UnitName => unitData.unitName;
    public int CurrentHP => currentHP;
    public int MaxHP => unitData.maxHP;
    public int Speed => unitData.speed;
    public bool IsAlive => isAlive;
    public SkillData[] SkillSlots => skillSlots;

    public void Initialize()
    {
        currentHP = unitData.maxHP;
        isAlive = true;
    }
    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            isAlive = false;
        }
    }
}
