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

    [SerializeField]private GameObject cheesePrefab;
    [SerializeField]private Bounds cheeseArea;
    [SerializeField]private float cheesePerLevel;
    [SerializeField]private UnityEvent onGameStart;
    [SerializeField]private UnityEvent onGameOver;

    private List<GameObject> cheeseList = new List<GameObject>();

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
        ReadyToPlay, Playing, Gameover
    }

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
    
    public void RemoveCheese()
    {
        foreach(var cheese in cheeseList)
        {
            Destroy(cheese);
        }
    }


    public void StartGame()
    {
        SpawnCheese();
        onGameStart.Invoke();
    }
    

    void EndGame()
    {
        RemoveCheese();
        onGameOver.Invoke();
    }
}

