using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarScript : MonoBehaviour
{
    private float oldSize, newSize, changeVelocity, changeProgress, changeSize, scaleMultiplier = 2.9f;
    private Vector3 mySize;
    private bool changingSize;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (changingSize)
        {
            changeProgress += Mathf.Abs(Time.deltaTime * changeVelocity);
            if (Mathf.Abs(changeProgress) >= Mathf.Abs(changeSize))
            {
                transform.localScale = new Vector3(newSize * scaleMultiplier, transform.localScale.y, transform.localScale.z);
                changingSize = false;
                changeProgress = 0;
            }
            else
            {
                transform.localScale += new Vector3(changeVelocity * Time.deltaTime * scaleMultiplier, 0, 0);
            }
        }
    }

    void Awake()
    {
        mySize = transform.localScale;
        changingSize = false;
        scaleMultiplier = 2.9f;
    }

    public void SetSize(float target, float changeSpeed)
    {
        newSize = target;
        if (changeSpeed == 0)
        {
            transform.localScale = new Vector3(newSize*scaleMultiplier, transform.localScale.y, transform.localScale.z);
        }
        if (newSize > 1)
        {
            newSize = 1;
        }
        if (newSize < 0)
        {
            newSize = 0;
        }
        oldSize = transform.localScale.x / scaleMultiplier;
        changingSize = true;
        changeProgress = 0;
        changeVelocity = (newSize - oldSize) * changeSpeed;
        changeSize = newSize - oldSize;
    }
}
