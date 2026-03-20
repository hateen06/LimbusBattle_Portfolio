using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    private const int MaxLogEntries = 6;

    [Header("유닛")]
    [SerializeField] private Unit allyUnit;
    [SerializeField] private Unit enemyUnit;

    [Header("UI - 텍스트")]
    [SerializeField] private TextMeshProUGUI allyNameText;
    [SerializeField] private TextMeshProUGUI allyHPText;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI battleLogText;

    private readonly List<string> battleLogs = new List<string>();
    private SkillData selectedSkill;
    private int turnCount;

    private void Start()
    {
        if (allyUnit == null || enemyUnit == null)
        {
            ClearLog();
            Log("※ 전투 준비 실패");
            enabled = false;
            return;
        }

        allyUnit.Initialize();
        enemyUnit.Initialize();

        UpdateUI();
        ClearLog();
        Log("▶ 전투 시작");
    }

    public void OnSkillSelected(int skillIndex)
    {
        if (allyUnit == null || allyUnit.SkillSlots == null)
        {
            Log("※ 아군 스킬 슬롯 없음");
            return;
        }

        if (skillIndex < 0 || skillIndex >= allyUnit.SkillSlots.Length)
        {
            Log("※ 잘못된 스킬 선택");
            return;
        }

        selectedSkill = allyUnit.SkillSlots[skillIndex];

        if (selectedSkill == null)
        {
            Log("※ 비어 있는 슬롯");
            return;
        }

        Log("▷ 선택: " + selectedSkill.skillName);
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
            Log("※ 적 스킬 없음");
            return;
        }

        turnCount++;

        int allySpeed = allyUnit.RollSpeed();
        int enemySpeed = enemyUnit.RollSpeed();

        TurnResolutionResult turnResult = BattleTurnResolver.ResolveTurn(
            allyUnit,
            selectedSkill,
            enemyUnit,
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

        Log("[ " + turnCount + "턴 ] " + turnResult.logMessage);
        UpdateUI();
        CheckBattleEnd();

        selectedSkill = null;
    }

    private bool CanExecuteTurn()
    {
        if (selectedSkill == null)
        {
            Log("※ 스킬을 먼저 선택하세요");
            return false;
        }

        if (!allyUnit.IsAlive || !enemyUnit.IsAlive)
        {
            Log("※ 전투 종료");
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
        battleLogs.Clear();

        if (battleLogText != null)
        {
            battleLogText.text = string.Empty;
        }
    }

    private void Log(string message)
    {
        if (battleLogText == null)
        {
            return;
        }

        battleLogs.Add(message);

        while (battleLogs.Count > MaxLogEntries)
        {
            battleLogs.RemoveAt(0);
        }

        battleLogText.text = string.Join("\n", battleLogs);
    }

    private void CheckBattleEnd()
    {
        if (!allyUnit.IsAlive && !enemyUnit.IsAlive)
        {
            Log("◆ 무승부");
            DisableButtons();
            return;
        }

        if (!enemyUnit.IsAlive)
        {
            Log("★ 승리");
            DisableButtons();
            return;
        }

        if (!allyUnit.IsAlive)
        {
            Log("X 패배");
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
