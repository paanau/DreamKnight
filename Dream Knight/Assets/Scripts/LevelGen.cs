using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen : MonoBehaviour
{
    // Should create enemies and bosses. Start creating them a certain distance from the player start position 

    // Enemy types
    public GameObject[] enemyTypes;
    // Boss types
    public GameObject[] bossTypes;

    // Enemy list? 
    public List<GameObject> enemies;

    // Level types: standard - Straight up progression from easier to tougher, mixed - random mix of easier and more difficult baddies, 
    public enum levelTypes { standard, mixed };

    // Start is called before the first frame update
    private void Awake()
    {
        enemies = GameObject.Find("GameController").GetComponent<GameController>().GetEnemies();
        int[] bounds = { 0, 0 };
        CreateLevel(20, bounds, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    // Method to clear all enemies 
}
