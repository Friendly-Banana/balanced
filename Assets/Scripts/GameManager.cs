using System;
using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class GameManager : NetworkManager
{
    public enum GameState
    {
        Loading,
        Playing,
        Finished
    }

    public static GameManager instance => (GameManager)singleton;
    public static event Action<GameState> StateChanged;

    public TroopSO[] troops;
    public Transform[] castles;
    public LayerMask[] enemyLayer;
    public Color[] teamColor;
    public List<Troop> activeTroops = new();
    public GameObject healthbar;

    public float attackSpeed = 0.5f;
    private GameState state = GameState.Loading;
    private int readyPlayer = 0;

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == onlineScene)
        {
            readyPlayer = 0;
            state = GameState.Loading;
            castles = GameObject.FindGameObjectsWithTag("Castle").Select(x => x.transform).ToArray();
        }
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        readyPlayer++;
        if (readyPlayer == 2)
        {
            castles[0].GetComponent<Castle>().StartGame();
            castles[1].GetComponent<Castle>().StartGame();
            state = GameState.Playing;
            StateChanged?.Invoke(state);
            StartCoroutine(AttackTimer());
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        var player = conn.connectionId == 0 ? 0 : 1;
        var go = castles[player].gameObject;
        go.GetComponent<Castle>().player = player;

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        go.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, go);
    }

    IEnumerator AttackTimer()
    {
        while (state == GameState.Playing)
        {
            foreach (var troop in activeTroops)
            {
                troop.Attack();
            }

            yield return new WaitForSeconds(attackSpeed);
        }
    }

    public void GameFinished(int player)
    {
        if (state != GameState.Playing)
        {
            return;
        }

        state = GameState.Finished;
        StateChanged?.Invoke(state);
        castles[1 - player].GetComponent<Castle>().FinishGame(true);
        Time.timeScale = 0;
        Debug.Log($"Yay, player {1 - player} won");
    }
}