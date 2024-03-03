using System;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class Castle : Health
{
    [SerializeField] private float energyRechargingRate = 0.5f;
    [SyncVar] [SerializeField] private float maxEnergy = 20;

    [SyncVar(hook = nameof(UpdateEnergy))] [SerializeField]
    private float energy;

    private readonly SyncHashSet<int> deck = new();

    #region Client

    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] protected FilledBar energyBar;

    [SerializeField] private GameObject troopUIPrefab;
    [SerializeField] private Transform troopUIParent;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject loadingPanel;

    #endregion

    #region Client

    [TargetRpc]
    public void GetReady()
    {
        loadingPanel.SetActive(false);
    }

    [TargetRpc]
    public void FinishGame(bool won)
    {
        (won ? winPanel : losePanel).SetActive(true);
    }

    [Client]
    protected override void UpdateHealthbar(float _, float newHealth)
    {
        base.UpdateHealthbar(_, newHealth);
        healthText.text = $"{newHealth:0.}/{maxHealth:0.}";
    }

    private void UpdateEnergy(float _, float newEnergy)
    {
        energyBar.SetValue(newEnergy / maxEnergy);
        energyText.text = newEnergy.ToString("f1");
    }

    public override void OnStartClient()
    {
        UpdateHealthbar(0, health);

        if (isLocalPlayer)
        {
            deck.Callback += UpdateDeck;
            // Process initial SyncList payload
            foreach (var id in deck)
            {
                UpdateDeck(SyncSet<int>.Operation.OP_ADD, id);
            }
        }
    }

    private void UpdateDeck(SyncSet<int>.Operation op, int id)
    {
        switch (op)
        {
            case SyncSet<int>.Operation.OP_ADD:
                var troop = GameManager.instance.troops[id];
                var go = Instantiate(troopUIPrefab, troopUIParent);
                go.name = id.ToString();
                go.GetComponent<Image>().sprite = troop.sprite;
                go.GetComponentInChildren<TMP_Text>().text = troop.energyCost.ToString();
                go.GetComponent<Button>().onClick.AddListener(() => CmdSpawn(id));
                break;
            case SyncSet<int>.Operation.OP_REMOVE:
                Destroy(troopUIParent.Find(id.ToString()));
                break;
            case SyncSet<int>.Operation.OP_CLEAR:
                Debug.LogWarning("why clear");
                troopUIParent.ClearChildren();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }
    }

    #endregion

    #region Server

    private bool gameActive;
    public Transform troopParent;

    [Server]
    protected override void Die()
    {
        GameManager.instance.GameFinished(player);
        FinishGame(false);
    }

    public override void OnStartServer()
    {
        GameManager.StateChanged += state =>
        {
            gameActive = state == GameManager.GameState.Playing;
            if (gameActive)
            {
                GetReady();
            }
            else if (state == GameManager.GameState.OtherDisconnected)
            {
                loadingPanel.SetActive(true);
                loadingPanel.GetComponentInChildren<TMP_Text>().text = "Other player disconnected, waiting...";
            }
        };
        deck.AddRange(Enumerable.Range(0, GameManager.instance.troops.Length));
    }

    private void Update()
    {
        if (isServer && gameActive)
        {
            energy = Mathf.Min(maxEnergy, energy + energyRechargingRate * Time.deltaTime);
        }
    }

    [Command]
    private void CmdSpawn(int id)
    {
        if (deck.Contains(id) && gameActive)
        {
            var troop = GameManager.instance.troops[id];
            if (energy >= troop.energyCost)
            {
                energy -= troop.energyCost;
                var go = Instantiate(troop.prefab, troopParent.position, Quaternion.identity);
                var t = go.GetComponent<Troop>();
                t.player = player;
                NetworkServer.Spawn(go);
            }
        }
    }

    #endregion
}