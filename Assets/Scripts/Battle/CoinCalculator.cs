using UnityEngine;
using UnityEngine.Rendering;

public class CoinCalculator
{
    public static int CalculateSkillPower(SkillData skill)
    {
        int power = skill.basePower;

        for (int i = 0; i < skill.coinCount; i++)
        {
            bool isHeads = Random.Range(0, 100) < 50;
            if (isHeads)
                power += skill.coinPower;
            else
                power -= skill.coinPower;
        }
        return Mathf.Max(0, power);
    }
}