using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ClashWinner
{
    Attacker,
    Defender,
    Draw
}

public class ClashRoundResult
{
    public int roundIndex;
    public SkillPowerRollResult attackerRoll;
    public SkillPowerRollResult defenderRoll;
    public ClashWinner roundWinner;
    public bool resolvedBySpeed;
}

public class ClashResult
{
    public ClashWinner winner;
    public int finalDamage;
    public int attackerRemainingCoins;
    public int defenderRemainingCoins;
    public SkillPowerRollResult finalAttackRoll;
    public List<ClashRoundResult> rounds = new List<ClashRoundResult>();
    public string clashLog;
}

public class TurnResolutionResult
{
    public int damageToAlly;
    public int damageToEnemy;
    public string logMessage;
}

public static class ClashResolver
{
    private const int ClashSafetyLimit = 20;

    public static ClashResult ResolveAttackClash(
        SkillData attackerSkill,
        SkillData defenderSkill,
        int attackerSpeed,
        int defenderSpeed)
    {
        ClashResult result = new ClashResult();
        StringBuilder logBuilder = new StringBuilder();

        int attackerCoins = attackerSkill != null ? attackerSkill.coinCount : 0;
        int defenderCoins = defenderSkill != null ? defenderSkill.coinCount : 0;
        int roundIndex = 1;
        int clashCount = 0;

        while (attackerCoins > 0 && defenderCoins > 0 && clashCount < ClashSafetyLimit)
        {
            clashCount++;

            SkillPowerRollResult attackerRoll = CoinCalculator.RollSkillPower(attackerSkill, attackerCoins);
            SkillPowerRollResult defenderRoll = CoinCalculator.RollSkillPower(defenderSkill, defenderCoins);

            ClashRoundResult roundResult = new ClashRoundResult
            {
                roundIndex = roundIndex,
                attackerRoll = attackerRoll,
                defenderRoll = defenderRoll
            };

            logBuilder.Append("합 ");
            logBuilder.Append(roundIndex);
            logBuilder.Append(": ");
            logBuilder.Append(CoinCalculator.BuildRollSummary(attackerRoll));
            logBuilder.Append(" vs ");
            logBuilder.Append(CoinCalculator.BuildRollSummary(defenderRoll));

            if (attackerRoll.finalPower > defenderRoll.finalPower)
            {
                defenderCoins--;
                roundResult.roundWinner = ClashWinner.Attacker;
                logBuilder.Append(" -> 수비 측 코인 1개 파괴");
            }
            else if (defenderRoll.finalPower > attackerRoll.finalPower)
            {
                attackerCoins--;
                roundResult.roundWinner = ClashWinner.Defender;
                logBuilder.Append(" -> 공격 측 코인 1개 파괴");
            }
            else if (attackerSpeed > defenderSpeed)
            {
                defenderCoins--;
                roundResult.roundWinner = ClashWinner.Attacker;
                roundResult.resolvedBySpeed = true;
                logBuilder.Append(" -> 위력이 같아서 공격 측이 속도로 우세");
            }
            else if (defenderSpeed > attackerSpeed)
            {
                attackerCoins--;
                roundResult.roundWinner = ClashWinner.Defender;
                roundResult.resolvedBySpeed = true;
                logBuilder.Append(" -> 위력이 같아서 수비 측이 속도로 우세");
            }
            else
            {
                roundResult.roundWinner = ClashWinner.Draw;
                logBuilder.Append(" -> 완전 동점, 같은 코인으로 다시 합");
            }

            logBuilder.AppendLine();
            result.rounds.Add(roundResult);
            roundIndex++;
        }

        result.attackerRemainingCoins = attackerCoins;
        result.defenderRemainingCoins = defenderCoins;

        if (attackerCoins > 0 && defenderCoins == 0)
        {
            result.winner = ClashWinner.Attacker;
            result.finalAttackRoll = CoinCalculator.RollSkillPower(attackerSkill, attackerCoins);
            result.finalDamage = Mathf.Max(0, result.finalAttackRoll.finalPower);

            logBuilder.Append("공격 측 합 승리. 마무리 공격: ");
            logBuilder.Append(CoinCalculator.BuildRollSummary(result.finalAttackRoll));
            logBuilder.AppendLine();
        }
        else if (defenderCoins > 0 && attackerCoins == 0)
        {
            result.winner = ClashWinner.Defender;
            result.finalAttackRoll = CoinCalculator.RollSkillPower(defenderSkill, defenderCoins);
            result.finalDamage = Mathf.Max(0, result.finalAttackRoll.finalPower);

            logBuilder.Append("수비 측 합 승리. 반격: ");
            logBuilder.Append(CoinCalculator.BuildRollSummary(result.finalAttackRoll));
            logBuilder.AppendLine();
        }
        else
        {
            result.winner = ClashWinner.Draw;
            result.finalDamage = 0;
            logBuilder.AppendLine("합이 너무 오래 이어져 이번 공격은 무효 처리되었습니다.");
        }

        result.clashLog = logBuilder.ToString().TrimEnd();
        return result;
    }
}

public static class BattleTurnResolver
{
    public static TurnResolutionResult ResolveTurn(
        SkillData allySkill,
        SkillData enemySkill,
        int allySpeed,
        int enemySpeed)
    {
        TurnResolutionResult result = new TurnResolutionResult();
        StringBuilder logBuilder = new StringBuilder();

        logBuilder.Append("속도: 아군 ");
        logBuilder.Append(allySpeed);
        logBuilder.Append(" / 적 ");
        logBuilder.Append(enemySpeed);
        logBuilder.AppendLine();
        logBuilder.Append("선택 스킬: 아군 [");
        logBuilder.Append(GetSkillName(allySkill));
        logBuilder.Append("] / 적 [");
        logBuilder.Append(GetSkillName(enemySkill));
        logBuilder.AppendLine("]");

        if (allySkill == null || enemySkill == null)
        {
            logBuilder.Append("스킬 참조가 없어서 턴을 처리할 수 없습니다.");
            result.logMessage = logBuilder.ToString();
            return result;
        }

        if (allySkill.skillType == SkillType.Attack && enemySkill.skillType == SkillType.Attack)
        {
            ClashResult clashResult = ClashResolver.ResolveAttackClash(allySkill, enemySkill, allySpeed, enemySpeed);
            logBuilder.AppendLine(clashResult.clashLog);

            if (clashResult.winner == ClashWinner.Attacker)
            {
                result.damageToEnemy = clashResult.finalDamage;
                logBuilder.Append("결과: 아군이 ");
                logBuilder.Append(clashResult.finalDamage);
                logBuilder.Append(" 데미지를 입혔습니다.");
            }
            else if (clashResult.winner == ClashWinner.Defender)
            {
                result.damageToAlly = clashResult.finalDamage;
                logBuilder.Append("결과: 적이 ");
                logBuilder.Append(clashResult.finalDamage);
                logBuilder.Append(" 데미지를 입혔습니다.");
            }
            else
            {
                logBuilder.Append("결과: 이번 합은 무승부로 끝났습니다.");
            }

            result.logMessage = logBuilder.ToString();
            return result;
        }

        if (allySkill.skillType == SkillType.Attack && enemySkill.skillType == SkillType.Defense)
        {
            ResolveAttackVsDefense(allySkill, enemySkill, true, result, logBuilder);
            result.logMessage = logBuilder.ToString();
            return result;
        }

        if (enemySkill.skillType == SkillType.Attack && allySkill.skillType == SkillType.Defense)
        {
            ResolveAttackVsDefense(enemySkill, allySkill, false, result, logBuilder);
            result.logMessage = logBuilder.ToString();
            return result;
        }

        if (allySkill.skillType == SkillType.Attack && enemySkill.skillType == SkillType.Evade)
        {
            ResolveAttackVsEvade(allySkill, enemySkill, true, result, logBuilder);
            result.logMessage = logBuilder.ToString();
            return result;
        }

        if (enemySkill.skillType == SkillType.Attack && allySkill.skillType == SkillType.Evade)
        {
            ResolveAttackVsEvade(enemySkill, allySkill, false, result, logBuilder);
            result.logMessage = logBuilder.ToString();
            return result;
        }

        logBuilder.Append("공격 스킬이 사용되지 않아서 이번 턴에는 체력 변화가 없습니다.");
        result.logMessage = logBuilder.ToString();
        return result;
    }

    private static void ResolveAttackVsDefense(
        SkillData attackSkill,
        SkillData defenseSkill,
        bool allyIsAttacker,
        TurnResolutionResult result,
        StringBuilder logBuilder)
    {
        SkillPowerRollResult attackRoll = CoinCalculator.RollSkillPower(attackSkill);
        SkillPowerRollResult defenseRoll = CoinCalculator.RollSkillPower(defenseSkill);
        int reducedDamage = Mathf.Max(0, attackRoll.finalPower - defenseRoll.finalPower);

        logBuilder.Append("공격 굴림: ");
        logBuilder.AppendLine(CoinCalculator.BuildRollSummary(attackRoll));
        logBuilder.Append("방어 굴림: ");
        logBuilder.AppendLine(CoinCalculator.BuildRollSummary(defenseRoll));

        if (allyIsAttacker)
        {
            result.damageToEnemy = reducedDamage;
            logBuilder.Append("결과: 적이 ");
            logBuilder.Append(defenseRoll.finalPower);
            logBuilder.Append("만큼 막아서 ");
            logBuilder.Append(reducedDamage);
            logBuilder.Append(" 데미지를 받았습니다.");
        }
        else
        {
            result.damageToAlly = reducedDamage;
            logBuilder.Append("결과: 아군이 ");
            logBuilder.Append(defenseRoll.finalPower);
            logBuilder.Append("만큼 막아서 ");
            logBuilder.Append(reducedDamage);
            logBuilder.Append(" 데미지를 받았습니다.");
        }
    }

    private static void ResolveAttackVsEvade(
        SkillData attackSkill,
        SkillData evadeSkill,
        bool allyIsAttacker,
        TurnResolutionResult result,
        StringBuilder logBuilder)
    {
        SkillPowerRollResult attackRoll = CoinCalculator.RollSkillPower(attackSkill);
        SkillPowerRollResult evadeRoll = CoinCalculator.RollSkillPower(evadeSkill);
        bool evaded = evadeRoll.finalPower >= attackRoll.finalPower;

        logBuilder.Append("공격 굴림: ");
        logBuilder.AppendLine(CoinCalculator.BuildRollSummary(attackRoll));
        logBuilder.Append("회피 굴림: ");
        logBuilder.AppendLine(CoinCalculator.BuildRollSummary(evadeRoll));

        if (evaded)
        {
            logBuilder.Append("결과: 회피에 성공해서 데미지가 없습니다.");
            return;
        }

        if (allyIsAttacker)
        {
            result.damageToEnemy = attackRoll.finalPower;
            logBuilder.Append("결과: 회피에 실패해서 적이 ");
            logBuilder.Append(attackRoll.finalPower);
            logBuilder.Append(" 데미지를 받았습니다.");
        }
        else
        {
            result.damageToAlly = attackRoll.finalPower;
            logBuilder.Append("결과: 회피에 실패해서 아군이 ");
            logBuilder.Append(attackRoll.finalPower);
            logBuilder.Append(" 데미지를 받았습니다.");
        }
    }

    private static string GetSkillName(SkillData skill)
    {
        return skill != null ? skill.skillName : "없음";
    }
}
