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

    public void OnExecuteTurn()
    {
        if (selectedSkill == null)
        {
            Log("스킬을 먼저 선택하세요!");
            return;
        }

        SkillData enemySkill = EnemyAI.SelectSkill(enemyUnit);

        // 속도 비교 - 빠른 쪽이 먼저 공격
        Unit firstUnit;
        Unit secondUnit;
        SkillData firstSkill;
        SkillData secondSkill;

        int allySpeed = allyUnit.RollSpeed();
        int enemySpeed = enemyUnit.RollSpeed();
        Log("속도 — 아군:" + allySpeed + " vs 적:" + enemySpeed);

        if (allySpeed >= enemySpeed)
        {
            firstUnit = allyUnit;
            secondUnit = enemyUnit;
            firstSkill = selectedSkill;
            secondSkill = enemySkill;
        }
        else
        {
            firstUnit = enemyUnit;
            secondUnit = allyUnit;
            firstSkill = enemySkill;
            secondSkill = selectedSkill;
        }

        // 합(Clash) 판정
        ClashResult result = ClashResolver.Resolve(firstSkill, secondSkill);

        string firstName = firstUnit == allyUnit ? "아군" : "적";
        string secondName = secondUnit == allyUnit ? "아군" : "적";

        string logMessage = "속도 — 아군:" + allySpeed + " vs 적:" + enemySpeed + "\n"
    + firstName + " [" + firstSkill.skillName + "] 위력:" + result.attackerPower
    + " vs " + secondName + " [" + secondSkill.skillName + "] 위력:" + result.defenderPower + "\n";

        if (result.winner == ClashWinner.Attacker)
        {
            int damage = result.attackerPower;
            secondUnit.TakeDamage(damage);
            logMessage += firstName + " 승리! " + secondName + "에게" + damage + " 데미지!";
        }
        else if (result.winner == ClashWinner.Defender)
        {
            int damage = result.defenderPower;
            firstUnit.TakeDamage(damage);
            logMessage += secondName + "승리! " + firstName + " 에게 "+ damage + " 데미지!";
        }
        else
        {
            logMessage += "무승부! 데미지 없음.";
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