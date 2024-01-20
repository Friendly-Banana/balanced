using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    public TMP_InputField nameInput;
    public Transform lobbyParent;
    public GameObject lobbyPrefab;

    public MyND networkDiscovery;
    readonly Dictionary<long, string> discoveredServers = new();

    public string code;

    public string LoadPlayerName() => PlayerPrefs.GetString(Utils.PLAYER_NAME_KEY, "Player");

    public void SaveName(string name)
    {
        Utils.PlayerName = name;
        PlayerPrefs.SetString(Utils.PLAYER_NAME_KEY, name);
        PlayerPrefs.Save();
    }

    private void Start()
    {
        nameInput.text = Utils.PlayerName = LoadPlayerName();
    }

    public void HostLobby()
    {
        GameManager.instance.StopClient();
        networkDiscovery.AdvertiseServer();
        GameManager.instance.StartHost();
    }

    public void SaveCode(string code)
    {
        this.code = code;
    }

    public void JoinWithCode()
    {
        GameManager.instance.networkAddress = Utils.CodeToAddress(code);
        GameManager.instance.StartClient();
    }

    public void FindGames()
    {
        discoveredServers.Clear();
        lobbyParent.ClearChildren();
        networkDiscovery.StartDiscovery();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void OnServerFound(ServerResponse response)
    {
        discoveredServers[response.serverId] = response.serverName;
        lobbyParent.ClearChildren();

        foreach (var info in discoveredServers.Values)
        {
            GameObject btn = Instantiate(lobbyPrefab, lobbyParent);
            btn.GetComponentInChildren<TMP_Text>().text = info;
            btn.GetComponent<Button>().onClick.AddListener(() => GameManager.instance.StartClient(response.uri));
        }
    }
}