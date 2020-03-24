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
    // Dead enemy dot color #5E0A0A
    private Color deadEnemy = new Color(.368f, .04f, .04f, .9f);
    private List<GameObject> enemyDots = new List<GameObject>();
    private List<GameObject> enemies;
    float[] mapBounds = {-627.0f, 627.0f};
    float maxX; 
    // Start is called before the first frame update
    void Start()
    {
        gc = GameObject.Find("GameController");
        Debug.Log("Initializing MiniMap...");
        enemies = gc.GetComponent<GameController>().GetEnemies();
        // Add the pre-existing dot

        foreach (GameObject enemy in enemies)
        {
            // Create minimap enemy dots
            GameObject newDot = GameObject.Instantiate(enemyDot, enemyDot.transform.position, Quaternion.identity) as GameObject;
            enemyDots.Add(newDot);
            newDot.transform.localScale = Vector3.one;            
            newDot.GetComponent<RectTransform>().sizeDelta = new Vector2(20,30);
            newDot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            newDot.transform.SetParent(GameObject.Find("MiniMap").transform);
            newDot.GetComponent<RectTransform>().anchoredPosition = new Vector2(627,-5f); //SetMapToMinimap(newDot.transform.position.x, maxX, mapBounds);

            // Find the last enemy's position
            if (enemy.transform.position.x > maxX)
            {
                maxX = enemy.transform.position.x;
            }

        }
        Destroy(enemyDot,0f);
        Debug.Log("Max X " + maxX);
    }

    // Update is called once per frame
    void Update()
    {
        // Move all dots according to where they are on the level
        for (int i = 0; i < enemies.Count; i++)
        {
            enemyDots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(MapToMinimap(enemies[i].transform.position.x, maxX, mapBounds),-5f);
            // Check alive
            if(!enemies[i].GetComponent<CharacterControl>().IsAlive())
            {
                enemyDots[i].GetComponent<Image>().color = deadEnemy;
            }
        }
        // Move the player too
            playerDot.GetComponent<RectTransform>().anchoredPosition = new Vector2(MapToMinimap(GameObject.Find("Character").transform.position.x, maxX, mapBounds), -5f);

    }
    float MapToMinimap(float xPos, float maxX, float[] mapBounds)
    {

        Debug.Log(maxX + " MAXX");
        // should map from the enemy positions to the minimap bounds
        float mappedXPos = xPos.Remap(0f, maxX, mapBounds[0], mapBounds[1]);
        Debug.Log(mappedXPos + " MAPPED POS X");
        return mappedXPos;
    }

}
