using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    [Header("유닛")]
    [SerializeField] private Unit allyUnit;
    [SerializeField] private Unit enemyUnit;

    [Header("UI - 텍스트")]
    [SerializeField] private TextMeshProUGUI allyNameText;
    [SerializeField] private TextMeshProUGUI allyHPText;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI battleLogText;

    private SkillData selectedSkill;
    private int turnCount = 0;

    private void Start()
    {
        allyUnit.Initialize();
        enemyUnit.Initialize();
        UpdateUI();
        Log("전투 시작! 스킬을 선택하세요.");
    }

    public void OnSkillSelected(int skillIndex)
    {
        selectedSkill = allyUnit.SkillSlots[skillIndex];
        Log(selectedSkill.skillName + " 선택됨. [턴 실행] 을 누르세요.");
    }

    public void OnExecuteTurn()
    {
        if (selectedSkill == null)
        {
            Log("스킬을 먼저 선택하세요!");
            return;
        }

        turnCount++;

        SkillData enemySkill = EnemyAI.SelectSkill(enemyUnit);

        int allySpeed = allyUnit.RollSpeed();
        int enemySpeed = enemyUnit.RollSpeed();

        string logMessage = "[ " + turnCount + "턴 ]\n"
            + "속도 — 아군:" + allySpeed + " vs 적:" + enemySpeed + "\n";
        if (allySpeed > enemySpeed)
        {
            int damage = ClashResolver.RollAttackDamage(selectedSkill);

            bool defending = enemySkill.skillType == SkillType.Defense;
            if (defending)
                damage = Mathf.RoundToInt(damage * 0.5f);

            enemyUnit.TakeDamage(damage);
            logMessage += "아군 속도 승리! 적 스킬 파괴!\n";
            logMessage += "아군 [" + selectedSkill.skillName + "] → 적에게 " + damage + " 데미지!";
            if (defending) logMessage += " (방어 50% 감소)";
        }
        else if (enemySpeed > allySpeed)
        {
            int damage = ClashResolver.RollAttackDamage(enemySkill);

            bool defending = selectedSkill.skillType == SkillType.Defense;
            if (defending)
                damage = Mathf.RoundToInt(damage * 0.5f);

            allyUnit.TakeDamage(damage);
            logMessage += "적 속도 승리! 아군 스킬 파괴!\n";
            logMessage += "적 [" + enemySkill.skillName + "] → 아군에게 " + damage + " 데미지!";
            if (defending) logMessage += " (방어 50% 감소)";
        }
        else if (enemySpeed > allySpeed)
        {
            // 적 속도 승리 → 아군 스킬 파괴, 적 일방 공격
            ClashResult result = ClashResolver.Resolve(enemySkill, selectedSkill);
            int damage = result.finalDamage;

            bool defending = selectedSkill.skillType == SkillType.Defense;
            if (defending)
                damage = Mathf.RoundToInt(damage * 0.5f);

            allyUnit.TakeDamage(damage);
            logMessage += "적 속도 승리! 아군 스킬 파괴!\n";
            logMessage += "적 [" + enemySkill.skillName + "] → 아군에게 " + damage + " 데미지!";
            if (defending) logMessage += " (방어 50% 감소)";
        }
        else
        {
            // 속도 동일 → 합(Clash) 돌입!
            ClashResult result = ClashResolver.Resolve(selectedSkill, enemySkill);
            logMessage += "속도 동일! 합(Clash) 돌입!\n";
            logMessage += result.clashLog;

            if (result.winner == ClashWinner.Attacker)
            {
                enemyUnit.TakeDamage(result.finalDamage);
                logMessage += "아군 승리! 적에게 " + result.finalDamage + " 데미지!";
            }
            else if (result.winner == ClashWinner.Defender)
            {
                allyUnit.TakeDamage(result.finalDamage);
                logMessage += "적 역전! 아군에게 " + result.finalDamage + " 데미지!";
            }
            else
            {
                logMessage += "양쪽 코인 소진! 데미지 없음.";
            }

            if (result.wasDefending)
                logMessage += " (방어 50% 감소)";
        }

        Log(logMessage);
        UpdateUI();
        CheckBattleEnd();

        selectedSkill = null;
    }

    private void UpdateUI()
    {
        allyNameText.text = allyUnit.UnitName;
        allyHPText.text = allyUnit.CurrentHP + "/" + allyUnit.MaxHP;
        enemyNameText.text = enemyUnit.UnitName;
        enemyHPText.text = enemyUnit.CurrentHP + "/" + enemyUnit.MaxHP;
    }

    private void Log(string message)
    {
        battleLogText.text = message;
    }

    private void CheckBattleEnd()
    {
        if (!enemyUnit.IsAlive)
        {
            Log("승리! 적을 쓰러뜨렸습니다!");
            DisableButtons();
        }
        else if (!allyUnit.IsAlive)
        {
            Log("패배... 아군이 쓰러졌습니다.");
            DisableButtons();
        }
    }

    private void DisableButtons()
    {
        GameObject.Find("SkillButton1").GetComponent<Button>().interactable = false;
        GameObject.Find("SkillButton2").GetComponent<Button>().interactable = false;
        GameObject.Find("SkillButton3").GetComponent<Button>().interactable = false;
        GameObject.Find("ExecuteButton").GetComponent<Button>().interactable = false;
    }
}