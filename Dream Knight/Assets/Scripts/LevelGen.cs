using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen : MonoBehaviour
{
    // Should create enemies and bosses. Start creating them a certain distance from the player start position 

    // Enemy types
    public GameObject[] enemyTypes;
    // Boss types
    public GameObject bossTypes;

    // Enemy list? 
    public List<GameObject> enemies;

    // Level types: standard - Straight up progression from easier to tougher, mixed - random mix of easier and more difficult baddies, 
    public enum levelTypes { standard, mixed };

    // Start is called before the first frame update
    private void Awake()
    {
        enemies = GameObject.Find("GameController").GetComponent<GameController>().GetEnemies();
        int[] bounds = { 0, 0 };
        CreateLevel(20,bounds, 0);
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
        int enemiesPerTier = enemyCount % (enemyBounds[1] - enemyBounds[0] + 1);
        int enemiesCreated;
        // Get player x-position
        float playerX = GameObject.Find("Character").transform.position.x; 
        for(int i = 0; i < enemyCount; i++)

        {
            if(levelType == levelTypes.standard)
            {

                GameObject newEnemy = Instantiate(enemyTypes[0], new Vector2(startBuffer + i * Random.Range(enemyBuffer[0], enemyBuffer[1]), 0), Quaternion.identity);
                enemies.Add(newEnemy);
            }
        }
    }

    // Method to clear all enemies 
}
