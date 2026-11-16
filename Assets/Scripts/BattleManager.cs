using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum BattleState { Inactive, PlayerTurn, EnemyTurn, Won, Lost }

[System.Serializable]
public class Combatant {
    public string name = "Fighter";
    public int maxHP = 30;
    public int attack = 6;
    [HideInInspector] public int hp;
    public bool IsDead => hp <= 0;
    public Slider hpSlider;
}

public class BattleManager : MonoBehaviour {
    public static BattleManager Instance { get; private set; }

    [Header("UI")]
    public GameObject battlePanel;
    public TMP_Text logText, playerHPText, enemyHPText, enterTooltip;
    public Button attackButton, healButton;

    [Header("Math Question UI")]
    public GameObject questionPanel;
    public TMP_Text questionText;
    public TMP_InputField answerInput;
    public Slider timerSlider;

    [Header("Math Settings")]
    [Tooltip("Seconds the player has to answer.")]
    public float answerTimeLimit = 10f;
    [Tooltip("Higher = bigger numbers. 1 = easy, 2 = medium, 3 = hard.")]
    [Range(1, 5)] public int difficulty = 1;

    [Header("Attack Scaling (multiplier on base damage)")]
    public float slowestAttackMultiplier = 0.5f;
    public float fastestAttackMultiplier = 2f;

    [Header("Defense Scaling (fraction of damage blocked)")]
    [Range(0, 1)] public float slowestBlock = 0.2f;
    [Range(0, 1)] public float fastestBlock = 0.9f;

    [Header("Combatants")]
    public Combatant player = new Combatant { name = "Sir Addsalot", maxHP = 40, attack = 8 };
    public Combatant defaultEnemy = new Combatant { name = "Enemy" };

    [Header("Events")]
    public UnityEvent onWin;
    public UnityEvent onLose;

    public BattleState State { get; private set; } = BattleState.Inactive;
    public static bool IsBattleActive => Instance != null && Instance.State != BattleState.Inactive;

    Combatant enemy;

    // Question state
    bool asking, submitted;
    int submittedValue;
    bool lastCorrect;
    float lastSpeed;

    void Awake() {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        battlePanel.SetActive(false);
        if (questionPanel) questionPanel.SetActive(false);
        attackButton.onClick.AddListener(OnAttack);
        healButton.onClick.AddListener(OnHeal);
        answerInput.onSubmit.AddListener(OnAnswerSubmitted);
    }

    public void StartBattle() => Begin(defaultEnemy);

    public void Begin(Combatant e) {
        enemy = e;
        enemy.hp = enemy.maxHP;
        player.hp = player.maxHP;

        battlePanel.SetActive(true);
        PauseController.SetPause(true);
        Log($"{enemy.name}  challenges  you!");
        RefreshUI();
        State = BattleState.PlayerTurn;
        SetButtons(true);
    }

    //=== Player actions ===

    void OnAttack() {
        if (State != BattleState.PlayerTurn) return;
        StartCoroutine(PlayerAttackRoutine());
    }

    void OnHeal() {
        if (State != BattleState.PlayerTurn) return;
        StartCoroutine(PlayerHealRoutine());
    }

    IEnumerator PlayerAttackRoutine() {
        SetButtons(false);
        Log("Solve  it  faster  to  hit  harder!");
        yield return AskQuestion();

        if (lastCorrect) {
            float mult = Mathf.Lerp(slowestAttackMultiplier, fastestAttackMultiplier, lastSpeed);
            int baseDmg = Random.Range(player.attack - 2, player.attack + 3);
            int dmg = Mathf.Max(1, Mathf.RoundToInt(baseDmg * mult));
            enemy.hp = Mathf.Max(0, enemy.hp - dmg);
            Log($"Correct!  You  hit  {enemy.name}  for  {dmg}!  (x{mult:0.0})");
        }
        else {
            Log("Wrong  or  too  slow,  your  attack  missed!");
        }
        RefreshUI();
        yield return WaitForContinue();
        Log("Enemy  attack  incoming!!  Prepare  to  defend...");
        yield return WaitForContinue();

        yield return EnemyPhase();
    }

    IEnumerator PlayerHealRoutine() {
        SetButtons(false);
        int amt = 8;
        player.hp = Mathf.Min(player.maxHP, player.hp + amt);
        Log($"You  recover  {amt}  HP.");
        RefreshUI();
        yield return WaitForContinue();

        yield return EnemyPhase();
    }

    //=== Enemy phase ===

    IEnumerator EnemyPhase() {
        if (enemy.IsDead) { End(BattleState.Won); yield break; }

        State = BattleState.EnemyTurn;
        Log($"{enemy.name}  attacks!  Solve  it  fast  to  block!");
        yield return AskQuestion();

        int incoming = Random.Range(enemy.attack - 2, enemy.attack + 3);
        float blocked = lastCorrect ? Mathf.Lerp(slowestBlock, fastestBlock, lastSpeed) : 0f;
        int dmg = Mathf.Max(0, Mathf.RoundToInt(incoming * (1f - blocked)));
        player.hp = Mathf.Max(0, player.hp - dmg);

        if (lastCorrect) Log($"You  blocked  {Mathf.RoundToInt(blocked * 100)}%!  {enemy.name}  hits  you  for  {dmg}.");
        else Log($"Too  slow!  {enemy.name}  hits  you  for  {dmg}!");
        RefreshUI();
        yield return WaitForContinue();

        if (player.IsDead) { End(BattleState.Lost); yield break; }

        State = BattleState.PlayerTurn;
        SetButtons(true);

        Log("Choose  your  move");
    }

    // ---------- Math question ----------

    IEnumerator AskQuestion() {
        GenerateQuestion(out string text, out int answer);
        questionText.text = text;
        answerInput.text = "";
        questionPanel.SetActive(true);
        answerInput.ActivateInputField();

        submitted = false;
        asking = true;
        float elapsed = 0f;

        while (!submitted && elapsed < answerTimeLimit) {
            elapsed += Time.unscaledDeltaTime;
            if (timerSlider) timerSlider.value = 1f - Mathf.Clamp01(elapsed / answerTimeLimit);
            yield return null;
        }

        asking = false;
        questionPanel.SetActive(false);

        lastCorrect = submitted && submittedValue == answer;
        lastSpeed = Mathf.Clamp01(1f - elapsed / answerTimeLimit);
    }

    void OnAnswerSubmitted(string input) {
        if (!asking) return;
        if (int.TryParse(input.Trim(), out int value)) {
            submittedValue = value;
            submitted = true;
        }
        else {
            answerInput.ActivateInputField();
        }
    }

    void GenerateQuestion(out string text, out int answer) {
        int max = 10 * difficulty;
        switch (Random.Range(0, 3)) {
            case 0: { // addition
                    int a = Random.Range(1, max + 1), b = Random.Range(1, max + 1);
                    text = $"{a} + {b} = ?"; answer = a + b; break;
                }
            case 1: { // subtraction (never negative)
                    int a = Random.Range(1, max + 1), b = Random.Range(1, max + 1);
                    if (b > a) (a, b) = (b, a);
                    text = $"{a} - {b} = ?"; answer = a - b; break;
                }
            default: { // multiplication
                    int a = Random.Range(2, 6 + difficulty), b = Random.Range(2, 6 + difficulty);
                    text = $"{a} × {b} = ?"; answer = a * b; break;
                }
        }
    }

    // ---------- End of battle ----------

    void End(BattleState result) {
        State = result;
        Log(result == BattleState.Won ? "Victory!" : "You were defeated...");
        StartCoroutine(Finish(result));
    }

    IEnumerator Finish(BattleState result) {
        yield return WaitForContinue();
        battlePanel.SetActive(false);
        PauseController.SetPause(false);
        State = BattleState.Inactive;

        if (result == BattleState.Won) onWin?.Invoke();
        else onLose?.Invoke();
    }

    void SetButtons(bool on) { attackButton.interactable = on; healButton.interactable = on; }
    void Log(string msg) => logText.text = msg;

    void RefreshUI() {
        playerHPText.text = $"{player.name}:  {player.hp}/{player.maxHP}";
        player.hpSlider.value = (float)player.hp / player.maxHP;

        enemyHPText.text = $"{enemy.name}:  {enemy.hp}/{enemy.maxHP}";
        enemy.hpSlider.value = (float)enemy.hp / enemy.maxHP;
    }
    IEnumerator WaitForContinue() {
        yield return null; 
        enterTooltip.text = "Enter  to  continue...";
        while (true) {
            var kb = Keyboard.current;
            if (kb != null &&
                (kb.enterKey.wasPressedThisFrame ||
                 kb.numpadEnterKey.wasPressedThisFrame ||
                 kb.eKey.wasPressedThisFrame)) {
                break;
            }
            yield return null;
        }
        enterTooltip.text = "";
    }
}