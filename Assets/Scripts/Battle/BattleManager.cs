using TMPro;
using UnityEngine;
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
    private int turnCount;

    private void Start()
    {
        if (allyUnit == null || enemyUnit == null)
        {
            ClearLog();
            Log("BattleManager에 유닛 참조가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        allyUnit.Initialize();
        enemyUnit.Initialize();

        UpdateUI();
        ClearLog();
        Log("전투 시작! 사용할 스킬을 선택하세요.");
    }

    public void OnSkillSelected(int skillIndex)
    {
        if (allyUnit == null || allyUnit.SkillSlots == null)
        {
            Log("아군 스킬 슬롯을 찾을 수 없습니다.");
            return;
        }

        if (skillIndex < 0 || skillIndex >= allyUnit.SkillSlots.Length)
        {
            Log("잘못된 스킬 번호입니다.");
            return;
        }

        selectedSkill = allyUnit.SkillSlots[skillIndex];

        if (selectedSkill == null)
        {
            Log("이 슬롯에는 스킬이 없습니다.");
            return;
        }

        Log("선택한 스킬: [" + selectedSkill.skillName + "]");
    }

    public void OnExecuteTurn()
    {
        if (!CanExecuteTurn())
        {
            return;
        }

        SkillData enemySkill = EnemyAI.SelectSkill(enemyUnit);

        if (enemySkill == null)
        {
            Log("적이 사용할 수 있는 스킬이 없습니다.");
            return;
        }

        turnCount++;

        int allySpeed = allyUnit.RollSpeed();
        int enemySpeed = enemyUnit.RollSpeed();

        TurnResolutionResult turnResult = BattleTurnResolver.ResolveTurn(
            selectedSkill,
            enemySkill,
            allySpeed,
            enemySpeed);

        if (turnResult.damageToEnemy > 0)
        {
            enemyUnit.TakeDamage(turnResult.damageToEnemy);
        }

        if (turnResult.damageToAlly > 0)
        {
            allyUnit.TakeDamage(turnResult.damageToAlly);
        }

        Log("[ " + turnCount + "턴 ]\n" + turnResult.logMessage);
        UpdateUI();
        CheckBattleEnd();

        selectedSkill = null;
    }

    private bool CanExecuteTurn()
    {
        if (selectedSkill == null)
        {
            Log("먼저 스킬을 선택하세요.");
            return false;
        }

        if (!allyUnit.IsAlive || !enemyUnit.IsAlive)
        {
            Log("이미 전투가 끝났습니다.");
            return false;
        }

        return true;
    }

    private void UpdateUI()
    {
        if (allyNameText != null)
        {
            allyNameText.text = allyUnit.UnitName;
        }

        if (allyHPText != null)
        {
            allyHPText.text = allyUnit.CurrentHP + "/" + allyUnit.MaxHP;
        }

        if (enemyNameText != null)
        {
            enemyNameText.text = enemyUnit.UnitName;
        }

        if (enemyHPText != null)
        {
            enemyHPText.text = enemyUnit.CurrentHP + "/" + enemyUnit.MaxHP;
        }
    }

    private void ClearLog()
    {
        if (battleLogText == null)
        {
            return;
        }

        battleLogText.text = string.Empty;
    }

    private void Log(string message)
    {
        if (battleLogText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(battleLogText.text))
        {
            battleLogText.text = message;
            return;
        }

        battleLogText.text += "\n\n" + message;
    }

    private void CheckBattleEnd()
    {
        if (!allyUnit.IsAlive && !enemyUnit.IsAlive)
        {
            Log("무승부! 양쪽 유닛이 모두 쓰러졌습니다.");
            DisableButtons();
            return;
        }

        if (!enemyUnit.IsAlive)
        {
            Log("승리! 적을 쓰러뜨렸습니다.");
            DisableButtons();
            return;
        }

        if (!allyUnit.IsAlive)
        {
            Log("패배... 아군이 쓰러졌습니다.");
            DisableButtons();
        }
    }

    private void DisableButtons()
    {
        SetButtonInteractable("SkillButton1", false);
        SetButtonInteractable("SkillButton2", false);
        SetButtonInteractable("SkillButton3", false);
        SetButtonInteractable("ExecuteButton", false);
    }

    private void SetButtonInteractable(string objectName, bool interactable)
    {
        GameObject buttonObject = GameObject.Find(objectName);

        if (buttonObject == null)
        {
            return;
        }

        Button button = buttonObject.GetComponent<Button>();

        if (button != null)
        {
            button.interactable = interactable;
        }
    }
}
