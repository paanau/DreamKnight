﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
	private float length, startpos;
	public GameObject cam;
	public float parallaxEffect;
	
    // Start is called before the first frame update
    void Start()
    {
		startpos = transform.position.x;
		length = GetComponent<SpriteRenderer>().bounds.size.x * 2;
    }

    // Update is called once per frame
    void Update()
    {
		float temp = ((cam.transform.position.x - 3) * (1 - parallaxEffect));
        float dist = ((cam.transform.position.x - 3) * parallaxEffect);
			
		transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);
		
		if(temp > startpos + length -3) startpos += 1 * length;
		else if (temp < startpos - length -3) startpos -= 1 * length;
	
	}
}
