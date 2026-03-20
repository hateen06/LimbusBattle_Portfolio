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
        Unit attackerUnit,
        SkillData attackerSkill,
        Unit defenderUnit,
        SkillData defenderSkill,
        int attackerSpeed,
        int defenderSpeed)
    {
        ClashResult result = new ClashResult();
        StringBuilder logBuilder = new StringBuilder();

        if (attackerUnit == null || defenderUnit == null || attackerSkill == null || defenderSkill == null)
        {
            result.winner = ClashWinner.Draw;
            result.finalDamage = 0;
            result.clashLog = "※ 합 처리 실패";
            return result;
        }

        int attackerCoins = attackerSkill.coinCount;
        int defenderCoins = defenderSkill.coinCount;
        int roundIndex = 1;
        int clashCount = 0;

        while (attackerCoins > 0 && defenderCoins > 0 && clashCount < ClashSafetyLimit)
        {
            clashCount++;

            int attackerBleedDamage = attackerUnit.ConsumeBleedOnAttack(1);
            int defenderBleedDamage = defenderUnit.ConsumeBleedOnAttack(1);

            if (attackerBleedDamage > 0)
            {
                AppendShortBleedLog(logBuilder, attackerUnit, attackerBleedDamage);
            }

            if (defenderBleedDamage > 0)
            {
                AppendShortBleedLog(logBuilder, defenderUnit, defenderBleedDamage);
            }

            if (!attackerUnit.IsAlive || !defenderUnit.IsAlive)
            {
                break;
            }

            SkillPowerRollResult attackerRoll = CoinCalculator.RollSkillPower(attackerSkill, attackerCoins);
            SkillPowerRollResult defenderRoll = CoinCalculator.RollSkillPower(defenderSkill, defenderCoins);

            ClashRoundResult roundResult = new ClashRoundResult
            {
                roundIndex = roundIndex,
                attackerRoll = attackerRoll,
                defenderRoll = defenderRoll
            };

            if (attackerRoll.finalPower > defenderRoll.finalPower)
            {
                defenderCoins--;
                roundResult.roundWinner = ClashWinner.Attacker;
            }
            else if (defenderRoll.finalPower > attackerRoll.finalPower)
            {
                attackerCoins--;
                roundResult.roundWinner = ClashWinner.Defender;
            }
            else if (attackerSpeed > defenderSpeed)
            {
                defenderCoins--;
                roundResult.roundWinner = ClashWinner.Attacker;
                roundResult.resolvedBySpeed = true;
            }
            else if (defenderSpeed > attackerSpeed)
            {
                attackerCoins--;
                roundResult.roundWinner = ClashWinner.Defender;
                roundResult.resolvedBySpeed = true;
            }
            else
            {
                roundResult.roundWinner = ClashWinner.Draw;
            }

            result.rounds.Add(roundResult);
            roundIndex++;
        }

        result.attackerRemainingCoins = attackerCoins;
        result.defenderRemainingCoins = defenderCoins;

        if (!attackerUnit.IsAlive && !defenderUnit.IsAlive)
        {
            result.winner = ClashWinner.Draw;
            result.finalDamage = 0;
            AppendLine(logBuilder, "● 양측 출혈 | ◆ 무승부");
        }
        else if (!attackerUnit.IsAlive)
        {
            result.winner = ClashWinner.Defender;
            result.finalDamage = 0;
            AppendLine(logBuilder, "● " + attackerUnit.UnitName + " 출혈 | ◆ 행동 실패");
        }
        else if (!defenderUnit.IsAlive)
        {
            result.winner = ClashWinner.Attacker;
            result.finalDamage = 0;
            AppendLine(logBuilder, "● " + defenderUnit.UnitName + " 출혈 | ◆ 전투 불가");
        }
        else if (attackerCoins > 0 && defenderCoins == 0)
        {
            result.winner = ClashWinner.Attacker;
            ResolveFinalAttack(result, attackerUnit, attackerSkill, defenderUnit, attackerCoins, "◆ 합 승리", logBuilder);
        }
        else if (defenderCoins > 0 && attackerCoins == 0)
        {
            result.winner = ClashWinner.Defender;
            ResolveFinalAttack(result, defenderUnit, defenderSkill, attackerUnit, defenderCoins, "◆ 합 패배", logBuilder);
        }
        else
        {
            result.winner = ClashWinner.Draw;
            result.finalDamage = 0;
            AppendLine(logBuilder, "◆ 합 무승부");
        }

        result.clashLog = logBuilder.ToString().TrimEnd();
        return result;
    }

    private static void ResolveFinalAttack(
        ClashResult result,
        Unit attackerUnit,
        SkillData attackSkill,
        Unit defenderUnit,
        int remainingCoins,
        string label,
        StringBuilder logBuilder)
    {
        int bleedDamage = attackerUnit.ConsumeBleedOnAttack(remainingCoins);

        if (bleedDamage > 0)
        {
            AppendShortBleedLog(logBuilder, attackerUnit, bleedDamage);
        }

        if (!attackerUnit.IsAlive)
        {
            result.finalDamage = 0;
            AppendLine(logBuilder, label + " | ● 출혈로 공격 실패");
            return;
        }

        result.finalAttackRoll = CoinCalculator.RollSkillPower(attackSkill, remainingCoins);
        result.finalDamage = Mathf.Max(0, result.finalAttackRoll.finalPower);

        AppendLine(logBuilder, label + " | ■ 피해 " + result.finalDamage);

        if (result.finalDamage > 0)
        {
            ApplyBleedOnHit(defenderUnit, attackSkill, logBuilder);
        }
    }

    private static void AppendShortBleedLog(StringBuilder logBuilder, Unit unit, int damage)
    {
        AppendLine(logBuilder, "● " + unit.UnitName + " -" + damage);
    }

    private static void ApplyBleedOnHit(Unit targetUnit, SkillData attackSkill, StringBuilder logBuilder)
    {
        if (targetUnit == null || attackSkill == null)
        {
            return;
        }

        if (attackSkill.bleedPotency <= 0 || attackSkill.bleedCount <= 0)
        {
            return;
        }

        targetUnit.AddBleed(attackSkill.bleedPotency, attackSkill.bleedCount);
        AppendLine(logBuilder, "● 출혈 +" + attackSkill.bleedPotency + "/" + attackSkill.bleedCount);
    }

    private static void AppendLine(StringBuilder builder, string text)
    {
        if (builder.Length > 0)
        {
            builder.Append(" | ");
        }

        builder.Append(text);
    }
}

public static class BattleTurnResolver
{
    public static TurnResolutionResult ResolveTurn(
        Unit allyUnit,
        SkillData allySkill,
        Unit enemyUnit,
        SkillData enemySkill,
        int allySpeed,
        int enemySpeed)
    {
        TurnResolutionResult result = new TurnResolutionResult();
        StringBuilder logBuilder = new StringBuilder();

        if (allyUnit == null || enemyUnit == null || allySkill == null || enemySkill == null)
        {
            result.logMessage = "※ 전투 정보 없음";
            return result;
        }

        AppendSegment(logBuilder, "▲ 속도 " + allySpeed + ":" + enemySpeed);
        AppendSegment(logBuilder, "◆ " + allySkill.skillName + " vs " + enemySkill.skillName);

        if (allySkill.skillType == SkillType.Attack && enemySkill.skillType == SkillType.Attack)
        {
            ClashResult clashResult = ClashResolver.ResolveAttackClash(
                allyUnit,
                allySkill,
                enemyUnit,
                enemySkill,
                allySpeed,
                enemySpeed);

            if (clashResult.winner == ClashWinner.Attacker)
            {
                result.damageToEnemy = clashResult.finalDamage;
            }
            else if (clashResult.winner == ClashWinner.Defender)
            {
                result.damageToAlly = clashResult.finalDamage;
            }

            AppendSegment(logBuilder, clashResult.clashLog);
            result.logMessage = logBuilder.ToString();
            return result;
        }

        if (allySkill.skillType == SkillType.Attack && enemySkill.skillType == SkillType.Defense)
        {
            ResolveAttackVsDefense(allyUnit, allySkill, enemyUnit, enemySkill, true, result, logBuilder);
            result.logMessage = logBuilder.ToString();
            return result;
        }

        if (enemySkill.skillType == SkillType.Attack && allySkill.skillType == SkillType.Defense)
        {
            ResolveAttackVsDefense(enemyUnit, enemySkill, allyUnit, allySkill, false, result, logBuilder);
            result.logMessage = logBuilder.ToString();
            return result;
        }

        if (allySkill.skillType == SkillType.Attack && enemySkill.skillType == SkillType.Evade)
        {
            ResolveAttackVsEvade(allyUnit, allySkill, enemyUnit, enemySkill, true, result, logBuilder);
            result.logMessage = logBuilder.ToString();
            return result;
        }

        if (enemySkill.skillType == SkillType.Attack && allySkill.skillType == SkillType.Evade)
        {
            ResolveAttackVsEvade(enemyUnit, enemySkill, allyUnit, allySkill, false, result, logBuilder);
            result.logMessage = logBuilder.ToString();
            return result;
        }

        AppendSegment(logBuilder, "■ 피해 없음");
        result.logMessage = logBuilder.ToString();
        return result;
    }

    private static void ResolveAttackVsDefense(
        Unit attackerUnit,
        SkillData attackSkill,
        Unit defenderUnit,
        SkillData defenseSkill,
        bool allyIsAttacker,
        TurnResolutionResult result,
        StringBuilder logBuilder)
    {
        int bleedDamage = attackerUnit.ConsumeBleedOnAttack(attackSkill.coinCount);

        if (bleedDamage > 0)
        {
            AppendSegment(logBuilder, "● " + attackerUnit.UnitName + " -" + bleedDamage);
        }

        if (!attackerUnit.IsAlive)
        {
            AppendSegment(logBuilder, "● 출혈로 공격 실패");
            return;
        }

        SkillPowerRollResult attackRoll = CoinCalculator.RollSkillPower(attackSkill);
        SkillPowerRollResult defenseRoll = CoinCalculator.RollSkillPower(defenseSkill);
        int reducedDamage = Mathf.Max(0, attackRoll.finalPower - defenseRoll.finalPower);

        AppendSegment(logBuilder, "▣ 방어 " + defenseRoll.finalPower);
        AppendSegment(logBuilder, "■ 피해 " + reducedDamage);

        if (allyIsAttacker)
        {
            result.damageToEnemy = reducedDamage;
        }
        else
        {
            result.damageToAlly = reducedDamage;
        }

        if (reducedDamage > 0)
        {
            ApplyBleedOnHit(defenderUnit, attackSkill, logBuilder);
        }
    }

    private static void ResolveAttackVsEvade(
        Unit attackerUnit,
        SkillData attackSkill,
        Unit evadeUnit,
        SkillData evadeSkill,
        bool allyIsAttacker,
        TurnResolutionResult result,
        StringBuilder logBuilder)
    {
        int bleedDamage = attackerUnit.ConsumeBleedOnAttack(attackSkill.coinCount);

        if (bleedDamage > 0)
        {
            AppendSegment(logBuilder, "● " + attackerUnit.UnitName + " -" + bleedDamage);
        }

        if (!attackerUnit.IsAlive)
        {
            AppendSegment(logBuilder, "● 출혈로 공격 실패");
            return;
        }

        SkillPowerRollResult attackRoll = CoinCalculator.RollSkillPower(attackSkill);
        SkillPowerRollResult evadeRoll = CoinCalculator.RollSkillPower(evadeSkill);
        bool evaded = evadeRoll.finalPower >= attackRoll.finalPower;

        if (evaded)
        {
            AppendSegment(logBuilder, "◇ 회피 성공");
            return;
        }

        AppendSegment(logBuilder, "◇ 회피 실패");
        AppendSegment(logBuilder, "■ 피해 " + attackRoll.finalPower);

        if (allyIsAttacker)
        {
            result.damageToEnemy = attackRoll.finalPower;
        }
        else
        {
            result.damageToAlly = attackRoll.finalPower;
        }

        if (attackRoll.finalPower > 0)
        {
            ApplyBleedOnHit(evadeUnit, attackSkill, logBuilder);
        }
    }

    private static void ApplyBleedOnHit(Unit targetUnit, SkillData attackSkill, StringBuilder logBuilder)
    {
        if (targetUnit == null || attackSkill == null)
        {
            return;
        }

        if (attackSkill.bleedPotency <= 0 || attackSkill.bleedCount <= 0)
        {
            return;
        }

        targetUnit.AddBleed(attackSkill.bleedPotency, attackSkill.bleedCount);
        AppendSegment(logBuilder, "● 출혈 +" + attackSkill.bleedPotency + "/" + attackSkill.bleedCount);
    }

    private static void AppendSegment(StringBuilder builder, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(" | ");
        }

        builder.Append(text);
    }
}
