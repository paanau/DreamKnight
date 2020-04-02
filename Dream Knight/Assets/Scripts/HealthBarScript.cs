using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarScript : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSize(float newValue, float changeSpeed)
    {
        transform.localScale = new Vector3(newValue, transform.localScale.y, transform.localScale.z);
    }
}
