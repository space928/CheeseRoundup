using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameManager instance;
    public GameState state;
    public int level;

    [SerializeField] private GameObject cheesePrefab;
    [SerializeField] private Bounds cheeseArea;
    [SerializeField] private float cheesePerLevel = 0.2f; //1 cat every 5 levels
    [SerializeField] private UnityEvent onGameStart;
    [SerializeField] private UnityEvent<bool> onGameOver;
    [SerializeField] private MouseController MC;
    

    private List<GameObject> cheeseList = new List<GameObject>();

    public List<GameObject> CheeseList { get {return cheeseList;} }

    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public enum GameState
    {
        ReadyToPlay, Playing, GameoverWin, GameoverLose 
    }

    //Spawn Cat**
    public void SpawnCheese()
    {
        for(int i = 0; i < Mathf.Ceil(level*cheesePerLevel); i++)
        {
            Vector3 pos = new Vector3(Random.value*cheeseArea.extents.x+cheeseArea.center.x, 
            Random.value*cheeseArea.extents.y+cheeseArea.center.y, 
            Random.value*cheeseArea.extents.z+cheeseArea.center.z);

            cheeseList.Add(Instantiate(cheesePrefab, pos, Quaternion.identity));
        }
    }
    
    //Kill Cat
    public void RemoveCheese()
    {
        for(int i = cheeseList.Count - 1; i >= 0; i--){
            Destroy(cheeseList[i]);
            cheeseList.RemoveAt(i);
        }
    }

    //Create Level System
    public void Level()
    {

    }

    public void StartGame()
    {
        SpawnCheese();
        state = GameState.Playing;
        onGameStart.Invoke();
    }
    public void GameoverWin()
    {
        RemoveCheese();
        state = GameState.GameoverWin;
        onGameOver.Invoke(true);
    }

    public void GameOverLose()
    {
        RemoveCheese();
        state = GameState.GameoverLose;
        onGameOver.Invoke(false);
    }

    public void RestartGame()
    {
        RemoveCheese();
        state = GameState.ReadyToPlay;
        onGameStart.Invoke();
    }
}

