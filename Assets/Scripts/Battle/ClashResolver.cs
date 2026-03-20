using UnityEngine;

public enum ClashWinner { Attacker, Defender, Draw }

public class ClashResult
{
    public int finalDamage;
    public ClashWinner winner;
    public bool wasDefending;
    public string clashLog;
}

public static class ClashResolver
{
    public static ClashResult Resolve(SkillData winnerSkill, SkillData loserSkill)
    {
        int winnerCoins = winnerSkill.coinCount;
        int loserCoins = loserSkill.coinCount;
        string log = "";
        int round = 1;

        int winnerPower = 0;
        int loserPower = 0;

        // 합 — 코인 대 코인 비교
        while (winnerCoins > 0 && loserCoins > 0)
        {
            winnerPower = RollPower(winnerSkill, winnerCoins);
            loserPower = RollPower(loserSkill, loserCoins);

            log += "  [" + round + "차] "
                + winnerSkill.skillName + " 위력:" + winnerPower + "(" + winnerCoins + "코인)"
                + " vs "
                + loserSkill.skillName + " 위력:" + loserPower + "(" + loserCoins + "코인)";

            if (winnerPower >= loserPower)
            {
                loserCoins--;
                log += " → " + loserSkill.skillName + " 코인 파괴!\n";
            }
            else
            {
                winnerCoins--;
                log += " → " + winnerSkill.skillName + " 코인 파괴!\n";
            }

            round++;
        }

        ClashResult result = new ClashResult();

        // 공격 — 남은 코인을 다시 굴려서 데미지 계산
        if (winnerCoins > 0)
        {
            result.winner = ClashWinner.Attacker;
            result.finalDamage = RollPower(winnerSkill, winnerCoins);
            result.wasDefending = loserSkill.skillType == SkillType.Defense;
            log += "  공격! " + winnerSkill.skillName + " 남은 " + winnerCoins + "코인 → 위력:" + result.finalDamage + "\n";
        }
        else if (loserCoins > 0)
        {
            result.winner = ClashWinner.Defender;
            result.finalDamage = RollPower(loserSkill, loserCoins);
            result.wasDefending = winnerSkill.skillType == SkillType.Defense;
            log += "  역전 공격! " + loserSkill.skillName + " 남은 " + loserCoins + "코인 → 위력:" + result.finalDamage + "\n";
        }
        else
        {
            result.winner = ClashWinner.Draw;
            result.finalDamage = 0;
            result.wasDefending = false;
            log += "  양쪽 코인 소진!\n";
        }

        if (result.wasDefending)
            result.finalDamage = Mathf.RoundToInt(result.finalDamage * 0.5f);

        result.clashLog = log;
        return result;
    }
    public static int RollAttackDamage(SkillData skill)
    {
        return RollPower(skill, skill.coinCount);
    }

    private static int RollPower(SkillData skill, int remainingCoins)
    {
        int power = skill.basePower;

        for (int i = 0; i < remainingCoins; i++)
        {
            bool isHeads = Random.Range(0, 100) < 50;

            if (isHeads)
                power += skill.coinPower;
        }


        return power;
    }
}