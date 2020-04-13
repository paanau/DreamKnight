using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    // Should create enemies and bosses. Start creating them a certain distance from the player start position 

    // Enemy types
    public GameObject[] enemyTypes;
    // Boss types
    public GameObject[] bossTypes;

    // Enemy list? 
    public List<GameObject> enemies;

    private GameObject player;
    // How far ahead the enemies should be placed so they fall outwith the minimap and don't just appear in the middle of it
    private float mapBuffer;

    // Timer to run the enemy reallocation
    float tick = 2.0f;

    // Spawn bias to spawn more difficult enemies
    int killCount = 0;

    // Start is called before the first frame update
    private void Awake()
    {
        enemies = GameObject.Find("GameController").GetComponent<GameController>().GetEnemies();
        int[] bounds = { 0, 0 };
        //CreateLevel(20, bounds, 0);
    }

    private void Start(){

        player = GameObject.Find("Character");
    }
    // Update is called once per frame
    void Update()
    {
        tick -= Time.deltaTime;
        if(tick < 0)
        {
            UpdateEnemies(enemies);
            tick = 2.0f;
        }
    }

    // Logic:
    // Track enemies
    // When they are way behind the player, move them to the end of the line 
    // Reorder the array 
    // Sometimes add bosses
    // Sometimes add bonuses 
    public void UpdateEnemies(List<GameObject> enemies)
    {
        player = GameObject.Find("Character");
        foreach (GameObject enemy in enemies)
        {
            // if (Vector3.Distance(enemy.transform.position, player.transform.position) < 30) enemy.GetComponent<SpriteRenderer>().enabled = true;

            // -20 is to check when the enemy has gone far enough behind the player/camera
            if(enemy.transform.position.y < -20 && enemy.GetComponent<CharacterControl>().readyToSpawn)
            {
                Debug.Log("I am down!");
                // move to the end
                // Check what level enemy should be set at 
                // Check if should change to a BOSS or TREASURE
                // Update minimap
                float xPos = FindMaxX(enemies); 
                // Either move the enemy out of the minimap bounds far ahead, or then a bit ahead of the currently most distant enemy
                if(xPos < (player.transform.position.x + mapBuffer))
                {
                    xPos = player.transform.position.x + mapBuffer;
                }
                xPos = xPos + Random.Range(4.0f, 8.0f);
                int typeID = Random.Range(0, 100) > killCount ? 1 : 2;
                enemy.GetComponent<CharacterControl>().SpawnCharacter(typeID, xPos);
                enemy.transform.localScale = new Vector3(4f, 4f, 4f);
                enemy.GetComponent<CharacterControl>().SetGameController(GameObject.Find("GameController"));
                //enemy.transform.position = new Vector2( enemy.transform.position.y);
                // Reactivate enemy 
                //enemy.GetComponent<CharacterControl>().SetAlive(true);

            }
        }
    }

    // Level types: standard - Straight up progression from easier to tougher, mixed - random mix of easier and more difficult baddies, 
    public enum levelTypes { standard, mixed };

    // Method to create the level
    // enemyBounds = Lowest and highest enemy levels
    public void CreateLevel(int enemyCount, int[] enemyBounds, int bossType, levelTypes levelType = levelTypes.standard)
    {
        // How much after the player's x-pos should the first enemy be created?
        float startBuffer = 10;
        // Space between enemies
        float[] enemyBuffer = { 4.0f, 8.0f };
        int enemyLevel = enemyBounds[0];
        if(enemyBounds[1] == 0 || enemyBounds[1] < enemyBounds[0])
        {
            enemyBounds[1] = enemyBounds[0];
        }
        int enemiesPerTier = enemyCount % (enemyBounds[1] - enemyBounds[0] + 1);

        // Get player x-position
        GameObject newEnemy = null;
        GameObject oldEnemy;

        float playerX = GameObject.Find("Character").transform.position.x; 
        for(int i = 0; i < enemyCount; i++)
        {
            if(levelType == levelTypes.standard)
            {
                // Different location for first enemy. All the others are relative to the most recent enemy

                if (i == 0)
                {
                    newEnemy = Instantiate(enemyTypes[enemyLevel], new Vector2(startBuffer + i * Random.Range(enemyBuffer[0], enemyBuffer[1]), 0), Quaternion.identity);
                }
                else
                {
                    Debug.Log(newEnemy.transform.position.x + " WOOOO");
                    oldEnemy = newEnemy;
                    newEnemy = Instantiate(enemyTypes[enemyLevel], new Vector2(oldEnemy.transform.position.x + Random.Range(enemyBuffer[0], enemyBuffer[1]), 0), Quaternion.identity);
                }

                enemies.Add(newEnemy);

                if (i > 0)
                {
                    // Time to create tougher enemies?
                    if (enemiesPerTier % i == 0)
                    {
                        Debug.Log("Tougher enemies!");
                        // Can't be a higher level than the upper bound, or amount of actual enemies to choose from
                        if (enemyBounds[1] > enemies.Count - 1)
                        {
                            enemyLevel = enemyTypes.Length - 1;

                        }
                        else
                        {
                            enemyLevel++;
                        }
                    }
                }
            }

            // On the final loop
            // add boss, if exists
            if (i == enemyCount - 1 && bossType > 0)
            {
                GameObject newBoss = Instantiate(bossTypes[bossType], new Vector2(newEnemy.transform.position.x + Random.Range(enemyBuffer[0], enemyBuffer[1]), 0), Quaternion.identity);
            }
        }


    }

    // Method to find the furthest enemy
    public float FindMaxX(List<GameObject> enemies)
    {        
        float maxX = 0;
        foreach (GameObject enemy in enemies){
        if (enemy.transform.position.x > maxX)
            {
                maxX = enemy.transform.position.x;
            }
        }
        return maxX;
    }
}
