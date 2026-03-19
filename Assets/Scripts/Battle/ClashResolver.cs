using UnityEngine;

public enum ClashWinner { Attacker, Defender , Draw}
public class ClashResult
{
    public int attackerPower;
    public int defenderPower;
    public ClashWinner winner;
}
public static class ClashResolver
{
    public static ClashResult Resolve(SkillData attackerSkill, SkillData defenderSkill)
    {
        int atkPower = CoinCalculator.CalculateSkillPower(attackerSkill);
        int defPower = CoinCalculator.CalculateSkillPower(defenderSkill);

        ClashResult result = new ClashResult();
        result.attackerPower = atkPower;
        result.defenderPower = defPower;

        if (atkPower > defPower)
            result.winner = ClashWinner.Attacker;
        else if (defPower > atkPower)
            result.winner = ClashWinner.Defender;
        else
            result.winner = ClashWinner.Draw;
        return result;
    }
}