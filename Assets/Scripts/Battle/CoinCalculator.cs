using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum CoinFlipResult
{
    Heads,
    Tails
}

public class CoinRollDetail
{
    public int coinIndex;
    public CoinFlipResult flipResult;
    public int powerBefore;
    public int powerAfter;
}

public class SkillPowerRollResult
{
    public SkillData skill;
    public int startingPower;
    public int finalPower;
    public List<CoinRollDetail> coinRolls = new List<CoinRollDetail>();
}

public static class CoinCalculator
{
    private const int HeadsChancePercent = 50;

    public static SkillPowerRollResult RollSkillPower(SkillData skill)
    {
        return RollSkillPower(skill, skill != null ? skill.coinCount : 0);
    }

    public static SkillPowerRollResult RollSkillPower(SkillData skill, int remainingCoins)
    {
        SkillPowerRollResult result = new SkillPowerRollResult();

        if (skill == null)
        {
            return result;
        }

        int usableCoinCount = Mathf.Clamp(remainingCoins, 0, skill.coinCount);
        int currentPower = skill.basePower;

        result.skill = skill;
        result.startingPower = skill.basePower;

        for (int i = 0; i < usableCoinCount; i++)
        {
            int powerBefore = currentPower;
            bool isHeads = Random.Range(0, 100) < HeadsChancePercent;

            if (isHeads)
            {
                currentPower += skill.coinPower;
            }

            result.coinRolls.Add(new CoinRollDetail
            {
                coinIndex = i + 1,
                flipResult = isHeads ? CoinFlipResult.Heads : CoinFlipResult.Tails,
                powerBefore = powerBefore,
                powerAfter = currentPower
            });
        }

        result.finalPower = Mathf.Max(0, currentPower);
        return result;
    }

    public static string BuildRollSummary(SkillPowerRollResult rollResult)
    {
        if (rollResult == null || rollResult.skill == null)
        {
            return "스킬 정보 없음";
        }

        StringBuilder builder = new StringBuilder();
        builder.Append(rollResult.skill.skillName);
        builder.Append(" [");

        for (int i = 0; i < rollResult.coinRolls.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(' ');
            }

            builder.Append(rollResult.coinRolls[i].flipResult == CoinFlipResult.Heads ? "앞" : "뒤");
        }

        builder.Append("] ");
        builder.Append(rollResult.startingPower);
        builder.Append(" -> ");
        builder.Append(rollResult.finalPower);

        return builder.ToString();
    }
}
