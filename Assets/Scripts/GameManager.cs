using System;
using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

public class GameManager : NetworkManager
{
    public enum GameState
    {
        Loading,
        Playing,
        OtherDisconnected,
        Finished
    }

    public static GameManager instance => (GameManager)singleton;
    public static event Action<GameState> StateChanged;

    public TroopSO[] troops;
    public Castle[] castles;
    public LayerMask[] enemyLayer;
    public Color[] teamColor;
    public List<Troop> activeTroops = new();
    public GameObject healthbar;

    public float attackSpeed = 0.5f;
    private GameState state = GameState.Loading;

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == onlineScene)
        {
            state = GameState.Loading;
            castles = FindObjectsByType<Castle>(FindObjectsSortMode.None);
            if (castles[0].name == "EnemyCastle")
            {
                (castles[0], castles[1]) = (castles[1], castles[0]);
            }
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        var player = conn.connectionId == 0 ? 0 : 1;
        var go = castles[player].gameObject;

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        go.name += $" [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, go);
        if (numPlayers == 2)
        {
            state = GameState.Playing;
            StateChanged?.Invoke(state);
            StartCoroutine(AttackTimer());
        }
    }

    /* public override void OnServerDisconnect(NetworkConnectionToClient conn)
     {
         state = GameState.OtherDisconnected;
         StateChanged?.Invoke(state);
     }*/

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
        castles[1 - player].FinishGame(true);
        Time.timeScale = 0;
        Debug.Log($"Yay, player {1 - player} won");
    }
}