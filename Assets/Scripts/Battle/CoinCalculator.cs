using UnityEngine;
public class CoinResult
{
    public int[] coinValues;
    public int totalPower;
}
public static class CoinCalculator
{
    public static CoinResult RollCoins(SkillData skill)
    {
        CoinResult result = new CoinResult();
        result.coinValues = new int[skill.coinCount];

        int currentPower = skill.basePower;

        for (int i = 0; i < skill.coinCount; i++)
        {
            bool isHeads = Random.Range(0, 100) < 50;

            if (isHeads)
                currentPower += skill.coinPower;

            result.coinValues[i] = currentPower;
        }

        result.totalPower = currentPower;
        return result;
    }
}