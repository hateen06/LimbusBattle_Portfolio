using UnityEngine;

public class EnemyAI
{
    public static SkillData SelectSkill(Unit enemy)
    {
        // HP가 30% 이하면 방어 스킬 선택 (마지막 슬롯이 방어라고 가정)
        float hpRatio = (float)enemy.CurrentHP / enemy.MaxHP;

        if (hpRatio <= 0.3f)
        {
            //방어 스킬이 있으면 사용
            SkillData lastSkill = enemy.SkillSlots[enemy.SkillSlots.Length - 1];
            return lastSkill;
        }

        //그 외에는 랜덤 스킬
        int randomIndex = Random.Range(0, enemy.SkillSlots.Length);
        return enemy.SkillSlots[randomIndex];
    }
}
