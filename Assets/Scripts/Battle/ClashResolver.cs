using UnityEngine;

public enum ClashWinner { Attacker, Defender, Draw }

public class ClashResult
{
    public int attackerPower;
    public int defenderPower;
    public ClashWinner winner;
    public int finalDamage;
    public bool wasDefending;
}

public static class ClashResolver
{
    public static ClashResult Resolve(SkillData winnerSkill, SkillData loserSkill, ClashWinner speedWinner)
    {
        ClashResult result = new ClashResult();
        result.attackerPower = CoinCalculator.CalculateSkillPower(winnerSkill);
        result.defenderPower = CoinCalculator.CalculateSkillPower(loserSkill);
        result.winner = speedWinner;

        if (speedWinner == ClashWinner.Draw)
        {
            result.finalDamage = 0;
            result.wasDefending = false;
        }
        else
        {
            result.finalDamage = result.attackerPower;
            result.wasDefending = loserSkill.skillName == "방어";

            if (result.wasDefending)
                result.finalDamage = Mathf.RoundToInt(result.finalDamage * 0.5f);
        }

        return result;
    }
}