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
    [SerializeField] private float cheeseRatioToWin = 0.9f; //1 cat every 5 levels
    [SerializeField] private UnityEvent onGameStart;
    [SerializeField] private UnityEvent onGameOver;
    [SerializeField] private MouseController mouseController;
    [SerializeField] private Material cheeseIndicatorMaterial;
    [SerializeField] private Material floorMaterial;
    [SerializeField] private GameObject gameOverLoseScreen;
    [SerializeField] private GameObject gameOverWinScreen;
    

    private List<GameObject> cheeseList = new List<GameObject>();

    public List<GameObject> CheeseList { get {return cheeseList;} }

    void Awake()
    {
        instance = this;
        gameOverLoseScreen.SetActive(false);
        gameOverWinScreen.SetActive(false);
        mouseController.CanMove = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public enum GameState
    {
        ReadyToPlay, Playing, LevelWin, GameoverLose 
    }

    public void OnCheeseAreaChanged(float cheeseArea, float maxArea)
    {
        if(cheeseArea/maxArea < 1- cheeseRatioToWin)
        {
            LevelWin();
        }

        cheeseIndicatorMaterial.SetFloat("_WipeAmount", 1 - cheeseArea / maxArea);
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
        gameOverLoseScreen.SetActive(false);
        gameOverWinScreen.SetActive(false);
        SpawnCheese();
        state = GameState.Playing;
        onGameStart.Invoke();
        mouseController.CanMove = true;
    }
    public void LevelWin()
    {
        mouseController.CanMove = false;
        RemoveCheese();
        state = GameState.LevelWin;

        gameOverWinScreen.SetActive(true);

        onGameOver.Invoke();

        // Set a new background colour
        floorMaterial.color = Color.HSVToRGB(Random.value, 0.62f, 0.69f);
    }

    public void GameOverLose()
    {
        mouseController.CanMove = false;
        RemoveCheese();
        state = GameState.GameoverLose;

        gameOverLoseScreen.SetActive(true);

        onGameOver.Invoke();

        // Set the background material colour to red
        floorMaterial.color = Color.HSVToRGB(0, 0.72f, 0.5f);
    }

    public void RestartGame()
    {
        mouseController.CanMove = false;
        RemoveCheese();
        state = GameState.ReadyToPlay;
        onGameStart.Invoke();
        gameOverLoseScreen.SetActive(false);
        gameOverWinScreen.SetActive(false);
    }
}

