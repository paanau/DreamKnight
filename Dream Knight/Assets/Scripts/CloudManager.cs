using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
    public GameObject[] cloudObject;
    private List<float> speeds = new List<float>();
    float[] speedLimits = {0.8f, 1.2f};
    public GameObject targetArea;
    List<GameObject> fogClouds = new List<GameObject>();
    public enum colors {blue, green, pink, red, purple, white };
    // Spawn all clouds at the start
    // Move the ones that are not on the screen in front of the player

    // Start is called before the first frame update
    void Start()
    {
       // colorOptions[0, 1] = new Color(1,0.5f,0.5f,0.6f);// Blue


        Debug.Log("Making clouds");
        // Instantiate clouds
        foreach (GameObject cloud in cloudObject)
        {
            float xLeft = targetArea.transform.position.x - targetArea.transform.localScale.x / 2;
            float xRight = targetArea.transform.position.x + targetArea.transform.localScale.x / 2;
            float yBottom = targetArea.transform.position.y - targetArea.transform.localScale.y / 2;
            float yTop = targetArea.transform.position.y + targetArea.transform.localScale.y / 2;
            //Debug.Log(xLeft.ToString() + " " + xRight.ToString() + " " + yBottom.ToString() + " " + yTop.ToString());
            GameObject newCloud = GameObject.Instantiate(cloud, new Vector3(Random.Range(xLeft, xRight), Random.Range(yBottom, yTop), 0), Quaternion.identity);
            fogClouds.Add(newCloud);
            newCloud.SetActive(true);
            newCloud.transform.SetParent(GameObject.Find("Clouds").transform);

            // Set cloud speed
            speeds.Add(Random.Range(speedLimits[0], speedLimits[1]));

        }

        // Testing colour adjustment
        SetCloudColor(Color.red);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int i = 0;
        foreach (GameObject cloud in fogClouds)
        {
            // Move all the clouds
            cloud.transform.Translate(new Vector2(-speeds[i], 0) * Time.deltaTime);
            i += 1;
            // Debug.Log(cloud.transform.position.x - Camera.main.transform.position.x);
            // If offscreen to the left, move back into bounds
            if(cloud.transform.position.x - Camera.main.transform.position.x < -20)
            {            
                
                float xLeft = targetArea.transform.position.x - targetArea.transform.localScale.x / 2;
                float xRight = targetArea.transform.position.x + targetArea.transform.localScale.x / 2;
                float yBottom = targetArea.transform.position.y - targetArea.transform.localScale.y / 2;
                float yTop = targetArea.transform.position.y + targetArea.transform.localScale.y / 2;
                
                cloud.transform.position = new Vector2(Random.Range(xLeft, xRight),  Random.Range(yBottom, yTop));
            }
        }
    }

    public void SetCloudColor(Color color)
    {
        foreach (GameObject cloud in fogClouds)
        {
            // Retain the existing transparency
            SpriteRenderer sprite = cloud.GetComponent<SpriteRenderer>();
            float alpha = sprite.color.a;
            sprite.color = new Color(color.r, color.g, color.b, alpha);
        }
    }
}
