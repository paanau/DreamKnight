using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    public float speed = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
           if(Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(new Vector3(1, 0, 0) * speed);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(new Vector3(1, 0, 0) * -speed);
        }

    }
}
