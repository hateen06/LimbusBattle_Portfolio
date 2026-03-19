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
    private int turnCount = 0;

    public void OnExecuteTurn()
    {
        if (selectedSkill == null)
        {
            Log("스킬을 먼저 선택하세요!");
            return;
        }

        turnCount++;

        SkillData enemySkill = EnemyAI.SelectSkill(enemyUnit);

        // 속도 비교 - 빠른 쪽이 먼저 공격
        int allySpeed = allyUnit.RollSpeed();
        int enemySpeed = enemyUnit.RollSpeed();

        ClashWinner speedWinner;
        Unit winnerUnit;
        Unit loserUnit;
        SkillData winnerSkill;
        SkillData loserSkill;

        if (allySpeed > enemySpeed)
        {
            speedWinner = ClashWinner.Attacker;
            winnerUnit = allyUnit;
            loserUnit = enemyUnit;
            winnerSkill = selectedSkill;
            loserSkill = EnemyAI.SelectSkill(enemyUnit);
        }
        else if (enemySpeed > allySpeed)
        {
            speedWinner = ClashWinner.Defender;
            winnerUnit = enemyUnit;
            loserUnit = allyUnit;
            winnerSkill = EnemyAI.SelectSkill(enemyUnit);
            loserSkill = selectedSkill;
        }
        else
        {
            speedWinner = ClashWinner.Draw;
            winnerUnit = null;
            loserUnit = null;
            winnerSkill = selectedSkill;
            loserSkill = EnemyAI.SelectSkill(enemyUnit);
        }

        ClashResult result = ClashResolver.Resolve(winnerSkill, loserSkill, speedWinner);

        string winnerName = winnerUnit == allyUnit ? "아군" : "적";
        string loserName = loserUnit == allyUnit ? "아군" : "적";

        string logMessage = "[ " + turnCount + "턴 ]\n"
            + "속도 — 아군:" + allySpeed + " vs 적:" + enemySpeed + "\n";

        if (speedWinner == ClashWinner.Draw)
        {
            logMessage += "속도 동일! 양쪽 스킬 파괴, 데미지 없음.";
        }
        else
        {
            logMessage += winnerName + " 속도 승리! [" + winnerSkill.skillName + "] 위력:" + result.attackerPower + "\n";
            logMessage += loserName + "의 [" + loserSkill.skillName + "] 파괴!\n";
            logMessage += loserName + "에게 " + result.finalDamage + " 데미지!";

            if (result.wasDefending)
                logMessage += " (방어로 50% 감소)";

            loserUnit.TakeDamage(result.finalDamage);
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
        // BottomPanel의 버튼들을 비활성화
        GameObject.Find("SkillButton1").GetComponent<UnityEngine.UI.Button>().interactable = false;
        GameObject.Find("SkillButton2").GetComponent<UnityEngine.UI.Button>().interactable = false;
        GameObject.Find("SkillButton3").GetComponent<UnityEngine.UI.Button>().interactable = false;
        GameObject.Find("ExecuteButton").GetComponent<UnityEngine.UI.Button>().interactable = false;
    }
}