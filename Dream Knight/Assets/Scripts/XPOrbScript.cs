using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPOrbScript : MonoBehaviour
{
    private ParticleSystem ps;
    private float timeElapsed, speed;
    private ParticleSystem.Particle[] particles;
    private CharacterControl player;
    private int[] values;
    private int value;
    // Start is called before the first frame update
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        int size = ps.emission.GetBurst(0).maxCount;
        particles = new ParticleSystem.Particle[size];
        speed = Random.Range(0.05f, 0.13f);
        player = GameObject.Find("Character").GetComponent<CharacterControl>();
        values = new int[size];
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
                particles[i].position = Vector3.MoveTowards(particles[i].position, playerPosition, speed);
                if (Vector3.Distance(particles[i].position, playerPosition) < 0.2f)
                {
                    particles[i].remainingLifetime = 0;
                    player.GrantExperience(values[i]);
                }
            }
            ps.SetParticles(particles);
        }
    }

    public void Initialise(int experience)
    {
        player = GameObject.Find("Character").GetComponent<CharacterControl>();
        ps = GetComponent<ParticleSystem>();
        int size = ps.emission.GetBurst(0).maxCount;
        particles = new ParticleSystem.Particle[size];
        speed = Random.Range(0.05f, 0.13f);
        values = new int[size];
        int addOne = 0;
        float experiencePerOrb = (float)value / (float)size;
        int minXP = Mathf.FloorToInt(experiencePerOrb);
        addOne = Mathf.FloorToInt(experiencePerOrb - minXP);
        ps.GetParticles(particles);
        for (int i = 0; i < size; i++)
        {
            values[i] = minXP;
            if (addOne > 0)
            {
                values[i]++;
                addOne--;
            }
            particles[i].startSize = 0.4f + 0.01f * values[i];
        }
        ps.SetParticles(particles);
    }
}
