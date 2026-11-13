using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum BattleState { Inactive, PlayerTurn, EnemyTurn, Won, Lost }

[System.Serializable]
public class Combatant {
    public string name = "Fighter";
    public int maxHP = 30;
    public int attack = 6;
    [HideInInspector] public int hp;
    public bool IsDead => hp <= 0;
}

public class BattleManager : MonoBehaviour {
    public static BattleManager Instance { get; private set; }

    [Header("UI")]
    public GameObject battlePanel;
    public TMP_Text logText, playerHPText, enemyHPText;
    public Button attackButton, healButton;

    [Header("Combatants")]
    public Combatant player = new Combatant { name = "Sir Addsalot", maxHP = 40, attack = 8 };
    public Combatant defaultEnemy = new Combatant { name = "Enemy" };

    [Header("Events")]
    public UnityEvent onWin;
    public UnityEvent onLose;

    public BattleState State { get; private set; } = BattleState.Inactive;
    public static bool IsBattleActive => Instance != null && Instance.State != BattleState.Inactive;

    Combatant enemy;
    UnityEvent enemyOnWin, enemyOnLose;

    void Awake() {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        battlePanel.SetActive(false);
        attackButton.onClick.AddListener(OnAttack);
        healButton.onClick.AddListener(OnHeal);
    }

    public void StartBattle() => Begin(defaultEnemy);

    public void Begin(Combatant e) {
        enemy = e;
        enemy.hp = enemy.maxHP;
        player.hp = player.maxHP;

        battlePanel.SetActive(true);
        PauseController.SetPause(true);
        Log($"{enemy.name}  challenges you!");
        RefreshUI();
        State = BattleState.PlayerTurn;
        SetButtons(true);
    }

    void OnAttack() {
        if (State != BattleState.PlayerTurn) return;
        StartCoroutine(PlayerAction(() => {
            int dmg = Random.Range(player.attack - 2, player.attack + 3); //
            enemy.hp = Mathf.Max(0, enemy.hp - dmg);
            Log($"You  hit  {enemy.name}  for  {dmg}!");
        }));
    }

    void OnHeal() {
        if (State != BattleState.PlayerTurn) return;
        StartCoroutine(PlayerAction(() => {
            int amt = 8;
            player.hp = Mathf.Min(player.maxHP, player.hp + amt);
            Log($"You recover  {amt}  HP.");
        }));
    }

    IEnumerator PlayerAction(System.Action action) {
        SetButtons(false);
        action();
        RefreshUI();
        yield return new WaitForSecondsRealtime(0.8f);

        if (enemy.IsDead) { End(BattleState.Won); yield break; }

        State = BattleState.EnemyTurn;
        int dmg = Random.Range(enemy.attack - 2, enemy.attack + 3);
        player.hp = Mathf.Max(0, player.hp - dmg);
        Log($"{enemy.name}  hits you  for  {dmg}!");
        RefreshUI();
        yield return new WaitForSecondsRealtime(0.8f);

        if (player.IsDead) { End(BattleState.Lost); yield break; }

        State = BattleState.PlayerTurn;
        SetButtons(true);
    }

    void End(BattleState result) {
        State = result;
        Log(result == BattleState.Won ? "Victory!" : "You were defeated...");

        StartCoroutine(Finish(result));
    }

    IEnumerator Finish(BattleState result) {
        yield return new WaitForSecondsRealtime(1.2f);
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
        enemyHPText.text = $"{enemy.name}:  {enemy.hp}/{enemy.maxHP}";
    }
}