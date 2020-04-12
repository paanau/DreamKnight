using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPOrbScript : MonoBehaviour
{
    private ParticleSystem ps;
    private float timeElapsed, speed;
    private ParticleSystem.Particle[] particles;
    private CharacterControl player;
    private int value, size;

    // Start is called before the first frame update
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        size = ps.emission.GetBurst(0).maxCount;
        particles = new ParticleSystem.Particle[size];
        speed = Random.Range(0.05f, 0.13f);
        player = GameObject.Find("Character").GetComponent<CharacterControl>();
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > 1)
        {
            ps.GetParticles(particles);
            Vector3 playerPosition = player.transform.position;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].position = Vector3.MoveTowards(particles[i].position, playerPosition, speed*timeElapsed);
                if (Vector3.Distance(particles[i].position, playerPosition) < 0.2f)
                {
                    particles[i].remainingLifetime = -1;
                    particles[i].startSize = 0;
                    if (size > 0)
                    {
                        player.GrantExperience(value);
                        size--;
                    }
                }
            }
            ps.SetParticles(particles);
            if (particles.Length == 0) { Destroy(gameObject); }
        }
    }

    public void Initialise(int experience)
    {
        player = GameObject.Find("Character").GetComponent<CharacterControl>();
        ps = GetComponent<ParticleSystem>();
        size = 0;
        for (int i = 15; i < 51; i++)
        {
            size = experience % i == 0 ? i : size;
            if (size != 0) break; 
            if (i == 50)
            {
                size = 50;
                experience = Mathf.FloorToInt((float)experience / size) * size;
            }
        }
        particles = new ParticleSystem.Particle[size];
        speed = Random.Range(0.05f, 0.13f);
        value = experience / size;
        
        ParticleSystem.Burst b = ps.emission.GetBurst(0);
        b.minCount = (short)size;
        b.maxCount = (short)size;
        b.count = size;
        ps.emission.SetBurst(0, b);

        Debug.Log("Bursting from " + ps.emission.GetBurst(0).minCount + " to " + ps.emission.GetBurst(0).maxCount);

        ps.GetParticles(particles);
        for (int i = 0; i < size; i++)
        {
            particles[i].startSize = 0.4f + 0.01f * value;
        }
        
        ps.SetParticles(particles);
    }
}
