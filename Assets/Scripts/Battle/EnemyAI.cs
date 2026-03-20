using System.Collections.Generic;
using UnityEngine;

public static class EnemyAI
{
    private const float LowHpThreshold = 0.3f;

    public static SkillData SelectSkill(Unit enemy)
    {
        if (enemy == null || enemy.SkillSlots == null || enemy.SkillSlots.Length == 0)
        {
            return null;
        }

        if (enemy.CurrentHPRatio <= LowHpThreshold)
        {
            SkillData defenseSkill = FindFirstSkillByType(enemy.SkillSlots, SkillType.Defense);

            if (defenseSkill != null)
            {
                return defenseSkill;
            }
        }

        List<SkillData> attackSkills = CollectSkillsByType(enemy.SkillSlots, SkillType.Attack);

        if (attackSkills.Count > 0)
        {
            int randomAttackIndex = Random.Range(0, attackSkills.Count);
            return attackSkills[randomAttackIndex];
        }

        List<SkillData> allSkills = CollectValidSkills(enemy.SkillSlots);

        if (allSkills.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, allSkills.Count);
        return allSkills[randomIndex];
    }

    private static SkillData FindFirstSkillByType(SkillData[] skillSlots, SkillType skillType)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            SkillData skill = skillSlots[i];

            if (skill != null && skill.skillType == skillType)
            {
                return skill;
            }
        }

        return null;
    }

    private static List<SkillData> CollectSkillsByType(SkillData[] skillSlots, SkillType skillType)
    {
        List<SkillData> skills = new List<SkillData>();

        for (int i = 0; i < skillSlots.Length; i++)
        {
            SkillData skill = skillSlots[i];

            if (skill != null && skill.skillType == skillType)
            {
                skills.Add(skill);
            }
        }

        return skills;
    }

    private static List<SkillData> CollectValidSkills(SkillData[] skillSlots)
    {
        List<SkillData> skills = new List<SkillData>();

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                skills.Add(skillSlots[i]);
            }
        }

        return skills;
    }
}
