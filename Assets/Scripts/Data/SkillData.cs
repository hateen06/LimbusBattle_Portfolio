using UnityEngine;

public enum SkillType { Attack, Defense, Evade}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Data/SkillData")]
public class SkillData : ScriptableObject 
{
    public string skillName;
    public int basePower; //기본 위력
    public int coinCount; // 코인 개수 (1~3)
    public int coinPower; // 코인당 추가 위력
    public SkillType skillType;
}