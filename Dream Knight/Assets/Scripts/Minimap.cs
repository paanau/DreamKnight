using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    // Display all level characters on the minimap and update periodically
    GameObject gc;
    GameObject miniMap;
    public GameObject playerDot;
    public GameObject enemyDot;

    private GameObject player;
    private Color aliveEnemy = new Color(.945f, .227f, .227f, 1.0f);
    // Dead enemy dot color #5E0A0A
    private Color deadEnemy = new Color(.368f, .04f, .04f, .9f);
    private List<GameObject> enemyDots = new List<GameObject>();
    private List<GameObject> enemies;
    float[] mapBounds = { -280.0f, 280.0f };
    float maxX; 
    // Start is called before the first frame update
    void Start()
    {
        gc = GameObject.Find("GameController");
        Debug.Log("Initializing MiniMap...");
        enemies = gc.GetComponent<GameController>().GetEnemies();
        // Add the pre-existing dot
        player = GameObject.Find("Character");
        foreach (GameObject enemy in enemies)
        {
            // Create minimap enemy dots
            GameObject newDot = GameObject.Instantiate(enemyDot, enemyDot.transform.position, Quaternion.identity) as GameObject;
            enemyDots.Add(newDot);
            newDot.transform.localScale = Vector3.one;            
            newDot.GetComponent<RectTransform>().sizeDelta = new Vector2(.5f,.5f);
            newDot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            newDot.transform.SetParent(GameObject.Find("MiniMap").transform);
            newDot.GetComponent<RectTransform>().anchoredPosition = new Vector2(300,0f); //SetMapToMinimap(newDot.transform.position.x, maxX, mapBounds);

            // Find the last enemy's position
            if (enemy.transform.position.x > maxX)
            {
                maxX = enemy.transform.position.x;
            }

        }
        Destroy(enemyDot,0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Move all dots according to where they are on the level
        for (int i = 0; i < enemies.Count; i++)
        {
            enemyDots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(MapToMinimap(enemies[i].transform.position.x, maxX, mapBounds),0f);
            
            // Only show enemies/objects if they are in the vicinity of the player
            if(enemies[i].transform.position.x - player.transform.position.x < -10 || enemies[i].transform.position.x - player.transform.position.x > 30)
            {
                enemyDots[i].SetActive(false);
            } else 
            {
                enemyDots[i].SetActive(true);

            }

            // Check alive
            if(!enemies[i].GetComponent<CharacterControl>().IsAlive())
            {
                enemyDots[i].GetComponent<Image>().color = deadEnemy;
            } else 
            {
                enemyDots[i].GetComponent<Image>().color = aliveEnemy;
            }
        }
        // Move the player too
        // Update: Player now static
        //   playerDot.GetComponent<RectTransform>().anchoredPosition = new Vector2(MapToMinimap(GameObject.Find("Character").transform.position.x, maxX, mapBounds), 0f);

    }

    public void ResetMinimap()
    {
        foreach (GameObject enemy in enemies)
        {
            // Find the last enemy's position
            if (enemy.transform.position.x > maxX)
            {
                maxX = enemy.transform.position.x;
            }
        }   
    }

    float MapToMinimap(float xPos, float maxX, float[] mapBounds)
    {
        float playerX = GameObject.Find("Character").transform.position.x;

        // should map from the enemy positions to the minimap bounds - relative to the player

        // Mapbounds stay the same, but the from range should remain relative to the player
        float mappedXPos = xPos.Remap(playerX - 10f, playerX + 30f, mapBounds[0], mapBounds[1]);
        return mappedXPos;
    }

}
