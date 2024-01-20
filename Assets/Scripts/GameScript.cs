using UnityEngine;

public class GameScript : MonoBehaviour
{
    public static GameScript singleton;

    public GameObject troopUIPrefab;
    public Transform troopUIParent;
    public GameObject loadingPanel;
    public GameObject winPanel;
    public GameObject losePanel;

    private void Awake()
    {
        singleton = this;
    }
}