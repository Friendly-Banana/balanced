using System;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Castle : Health
{
    public float energyRechargingRate = 0.5f;
    private readonly SyncList<int> troops = new();
    [SyncVar] private float maxEnergy;
    [SyncVar(hook = nameof(UpdateEnergy))] private float energy;

    #region Client

    [SerializeField] protected FilledBar energyBar;

    [SerializeField] private GameObject troopUIPrefab;
    [SerializeField] private Transform troopUIParent;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    #endregion

    #region Server

    private bool gameActive;
    private Transform troopParent;

    #endregion

    #region Client

    [TargetRpc]
    public void StartGame()
    {
        loadingPanel.SetActive(false);
    }

    [TargetRpc]
    public void FinishGame(bool won)
    {
        (won ? winPanel : losePanel).SetActive(true);
    }

    public override void OnStartLocalPlayer()
    {
        var gs = GameScript.singleton;
        troopUIPrefab = gs.troopUIPrefab;
        troopUIParent = gs.troopUIParent;
        loadingPanel = gs.loadingPanel;
        winPanel = gs.winPanel;
        losePanel = gs.losePanel;

        healthbar.SetValue(health / maxHealth);
        GetComponent<SpriteRenderer>().color *= GameManager.instance.teamColor[player];
        GetComponent<SpriteRenderer>().flipX = player == 0;

        troops.Callback += UpdateDeck;
        // Process initial SyncList payload
        for (int index = 0; index < troops.Count; index++)
            UpdateDeck(SyncList<int>.Operation.OP_ADD, index, -1, troops[index]);
    }

    private void UpdateEnergy(float _, float newEnergy)
    {
        energyBar.SetValue(newEnergy / maxEnergy);
    }

    private void UpdateDeck(SyncList<int>.Operation op, int index, int oldId, int newId)
    {
        switch (op)
        {
            case SyncList<int>.Operation.OP_ADD:
            case SyncList<int>.Operation.OP_INSERT:
                var troop = GameManager.instance.troops[newId];
                var go = Instantiate(troopUIPrefab, troopUIParent);
                go.name = newId.ToString();
                go.GetComponent<Image>().sprite = troop.sprite;
                go.GetComponentInChildren<TMP_Text>().text = troop.energyCost.ToString();
                go.GetComponent<Button>().onClick.AddListener(() => CmdSpawn(newId));
                break;
            case SyncList<int>.Operation.OP_REMOVEAT:
                Destroy(troopUIParent.Find(oldId.ToString()));
                break;
            case SyncList<int>.Operation.OP_SET:
                Debug.LogError("why set");
                break;
            case SyncList<int>.Operation.OP_CLEAR:
                Debug.LogWarning("why clear");
                troopUIParent.ClearChildren();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }
    }

    #endregion

    [Server]
    protected override void Die()
    {
        GameManager.instance.GameFinished(player);
        FinishGame(false);
    }

    public override void OnStartServer()
    {
        GameManager.StateChanged += state => gameActive = state == GameManager.GameState.Playing;
        troopParent = GameObject.FindGameObjectWithTag(player == 0 ? "mySpawn" : "enemySpawn").transform;
        troops.AddRange(Enumerable.Range(0, GameManager.instance.troops.Length));
    }

    [ServerCallback]
    private void Update()
    {
        energy = Mathf.Min(maxEnergy, energy + energyRechargingRate * Time.deltaTime);
    }

    [Command]
    private void CmdSpawn(int id)
    {
        if (troops.Contains(id) && gameActive)
        {
            var troop = GameManager.instance.troops[id];
            if (energy >= troop.energyCost)
            {
                energy -= troop.energyCost;
                var go = Instantiate(troop.prefab, troopParent);
                go.GetComponent<Troop>().player = player;
                NetworkServer.Spawn(go);
            }
        }
    }
}