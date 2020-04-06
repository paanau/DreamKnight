using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPOrbScript : MonoBehaviour
{
    private ParticleSystem ps;
    private float timeElapsed, speed;
    private ParticleSystem.Particle[] particles;
    private float experience;
    // Start is called before the first frame update
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        int size = ps.emission.GetBurst(0).maxCount;
        particles = new ParticleSystem.Particle[size];
        speed = Random.Range(0.05f, 0.13f);
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > 1)
        {
            ps.GetParticles(particles);
            Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].position = Vector3.MoveTowards(particles[i].position, playerPosition, speed);
                if (Vector3.Distance(particles[i].position, playerPosition) < 0.5f)
                {
                    
                }
            }
            ps.SetParticles(particles);
        }
    }

    public void Initialise(int experience)
    {

    }
}
