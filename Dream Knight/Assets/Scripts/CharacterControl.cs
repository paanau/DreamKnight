using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private GameObject gameController;
    private Animator myAnimator;
    private GameController main;
    private ParticleSystem.Particle[] effectParticles;
    [SerializeField] private float range, attackCooldown, gameRunSpeed, baseSpeed, speedModifiers, damageModifiers;
    [SerializeField] private bool inCombat, isAlive, waiting, myTurn;
    [SerializeField] private float currentHP, baseMaxHP, baseDamage, shieldHealth;
    [SerializeField] private bool isPlayer;
    [SerializeField] private GameObject fireB, effectCircle, healthBar;
    private GameObject[] healthbarComponents;
    private int directionModifier;

    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;
        baseMaxHP = 1000;
        currentHP = baseMaxHP;
        baseDamage = 30;
        baseSpeed = 1;
        inCombat = false;
        waiting = false;
        range = 2f;
        attackCooldown = 1f;
        directionModifier = -1;
        if (isPlayer)
        {
            baseDamage = 100;
            attackCooldown = 0.6f;
            directionModifier = 1;
        }
        myAnimator = GetComponent<Animator>();
        myAnimator.speed = 1;
        gameRunSpeed = 1;
        speedModifiers = 1;
        damageModifiers = 1;
    }

/*
    public void SpawnCharacter()
    {
        // Defaults
        isAlive = true;
        inCombat = false;
        waiting = false;
        myAnimator = GetComponent<Animator>();
        myAnimator.speed = 1;
        gameRunSpeed = 1;

        // Dependent on character specs
        baseMaxHP = 1000;
        currentHP = baseMaxHP;
        baseDamage = 30;
        baseSpeed = 1;
        range = 2f;
        attackCooldown = 1f;
        directionModifier = -1;
        if (isPlayer)
        {
            baseDamage = 100;
            attackCooldown = 0.6f;
            directionModifier = 1;
        }
        
        speedModifiers = 1;
        damageModifiers = 1;
    }
*/
    // Update is called once per frame
    void Update()
    {
        if (myTurn && isAlive && !inCombat)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + (range * directionModifier), transform.position.y), -Vector2.up);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                Debug.Log(hit.collider.gameObject.name);
                if (gameObject.tag == "Enemy" && hit.collider.gameObject.tag == "Enemy")
                {
                    waiting = true;
                }
                else
                {
                    inCombat = true;
                    main.InitiateCombat(gameObject, hit.collider.gameObject);
                    myAnimator.SetTrigger("enterCombat");
                    myAnimator.speed = 1f / attackCooldown;
                }
            }
            ManageAbilities();
        }
    }

    public void SetGameController(GameObject go)
    {
        gameController = go;
        main = go.GetComponent<GameController>();
    }

    public int DamageDealt()
    {
        int damageDealt = Mathf.FloorToInt(baseDamage * damageModifiers);
        if (damageModifiers > 2)
        {
            damageModifiers -= 1;
        }
        else
        {
            damageModifiers = 1;
        }
        return damageDealt;
    }

    public bool DamageTaken(int damage)
    {
        if (shieldHealth > 0)
        {
            if (shieldHealth >= damage)
            {
                shieldHealth -= damage;
                damage = 0;
            }
            else
            {
                damage -= Mathf.FloorToInt(shieldHealth);
                shieldHealth = 0;
            }
            if (shieldHealth < baseMaxHP)
            {
                
            }
        }

        if (damage > baseMaxHP)
        {
            CriticalDeath();
        }
        currentHP -= damage;
        if (currentHP <= 0)
        {
            // Death happens here
            // transform.Rotate(0, 0, -90);
            inCombat = false;
            return false;
        }
        return true;
    }

    private void CriticalDeath()
    {

    }

    public void WonCombat()
    {
        EndCombat();
    }

    public void LostCombat()
    {
        EndCombat();
        GetComponent<BoxCollider2D>().enabled = false;
        transform.position = new Vector2(transform.position.x, 0);
        isAlive = false;
    }

    private void EndCombat()
    {
        inCombat = false;
        myAnimator.SetTrigger("leaveCombat");
    }

    public float GetCooldown()
    {
        return attackCooldown;
    }

    public void TriggerBloodEffect(float damage, float runSpeed)
    {
        ParticleSystem myBloodLoss = GetComponent<ParticleSystem>();
        var em = myBloodLoss.emission;
        em.rateOverTimeMultiplier = 5 * damage;
        var main = myBloodLoss.main;
        main.simulationSpeed = runSpeed;
        myBloodLoss.Play();
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    public void SetAlive(bool alive)
    {
        isAlive = alive;
    }

    public bool IsNotInCombat()
    {
        return !inCombat;
    }

    public float GetSpeed()
    {
        return baseSpeed * speedModifiers;
    }

    public void StopWaiting()
    {
        waiting = false;
    }

    public bool IsNotWaiting()
    {
        return !waiting;
    }

    public void SetMyTurn(bool value)
    {
        myTurn = value;
        myAnimator.SetTrigger("idle");
    }

    public void AdvanceMe(float modifiers)
    {
        myAnimator.SetTrigger("move");
        transform.Translate(0.1f * modifiers * speedModifiers * baseSpeed * directionModifier, 0, 0);
        myAnimator.speed = modifiers * baseSpeed + 1;
    }

    public bool CanAdvance()
    {
        if (isAlive && !waiting && !inCombat)
        {
            return true;
        }
        return false;
    }

    public bool CanFight()
    {
        if (isAlive && inCombat && myTurn)
        {
            return true;
        }
        return false;
    }

    public void SetAnimationSpeed(float newSpeed)
    {
        myAnimator.speed = newSpeed * speedModifiers;
        gameRunSpeed = newSpeed * speedModifiers;
        ParticleSystem myBloodLoss = GetComponent<ParticleSystem>();
        var main = myBloodLoss.main;
        main.simulationSpeed = newSpeed;
    }

    public void UseAbility(string s)
    {
        if (s == "w")
        {
            GameObject fb = Instantiate(fireB, transform.position, Quaternion.identity);
            ProjectileScript ps = fb.GetComponent<ProjectileScript>();
            ps.GiveSettings((1 + myAnimator.speed) * 0.1f, 10, 100);
            ps.SetAnimationSpeed(gameRunSpeed);
        }
        if (s == "s")
        {
            speedModifiers += 2;
            AddParticlesToEffectCircle(0, 20);
        }
        if (s == "a")
        {
            shieldHealth += 100;
            AddParticlesToEffectCircle(1, 20);
        }
        if (s == "d")
        {
            damageModifiers += 10;
            AddParticlesToEffectCircle(2, 20);
        }
    }

    private void ManageAbilities()
    {
        if (speedModifiers > 1)
        {
            speedModifiers -= Time.deltaTime;
        }
        else
        {
            speedModifiers = 1;
        }
        if (damageModifiers > 1)
        {
            damageModifiers -= Time.deltaTime;
        }
        else
        {
            damageModifiers = 1;
        }

        if (speedModifiers > 1 || damageModifiers > 1 || shieldHealth > 0)
        {
            float speedParticles = (speedModifiers - 1) * 10;
            float shieldParticles = shieldHealth / 5;
            float damageParticles = (damageModifiers - 1) * 2;
            float totalParticleCount = speedParticles + shieldParticles + damageParticles;

            float[] particleFloats = { speedParticles, shieldParticles, damageParticles };

            ParticleSystem ps = effectCircle.GetComponent<ParticleSystem>();
            var emis = ps.emission;
            emis.rateOverTime = Mathf.FloorToInt(totalParticleCount);

            var psmain = ps.main;
            Color[] assignableColours = { Color.green, Color.blue, Color.red };

            

            Gradient newGradient = new Gradient();
            GradientColorKey[] colorKey = new GradientColorKey[3];
            GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
            float runningTally = 0;
            for (int i = 0; i < particleFloats.Length; i++)
            {
                if (particleFloats[i] > 0)
                {
                    colorKey[i].time = (particleFloats[i] + runningTally) / totalParticleCount;
                    colorKey[i].color = assignableColours[i];
                    alphaKey[i].alpha = 1;
                    alphaKey[i].time = (particleFloats[i] + runningTally) / totalParticleCount;
                    runningTally += particleFloats[i];
                }
                else
                {
                    colorKey[i].time = (float)i / 2;
                    colorKey[i].color = newGradient.Evaluate((float)i / 2);
                    alphaKey[i].alpha = 1;
                    alphaKey[i].time = (float)i / 2;
                }
            }
            newGradient.SetKeys(colorKey, alphaKey);
            psmain.startColor = newGradient;

            //float pstime = ps.time * 10;
            //if (pstime > 1)
            //{
            //    pstime -= Mathf.Floor(pstime);
            //}
            //if (pstime < speedParticles / totalParticleCount)
            //{
            //    psmain.startColor = assignableColours[0];
            //}
            //else if (pstime < (shieldParticles + speedParticles) / totalParticleCount)
            //{
            //    psmain.startColor = assignableColours[1];
            //}
            //else
            //{
            //    psmain.startColor = assignableColours[2];
            //}

        }
    }

    private void AddParticlesToEffectCircle(int category, int amount)
    {
        ParticleSystem ps = effectCircle.GetComponent<ParticleSystem>();
        var emis = ps.emission;
        var rot = emis.rateOverTime;
        rot.constant += amount;
        emis.rateOverTime = rot;
    }

    private void InitialiseHealthbars()
    {
        healthbarComponents = new GameObject[healthBar.transform.childCount];
        for (int i = 0; i < healthBar.transform.childCount; i++)
        {
            healthbarComponents[i] = healthBar.transform.GetChild(i).gameObject;
        }
    }

    private void SetHealthBar(string name, float newValue, float speed)
    {
        foreach (GameObject bar in healthbarComponents)
        {
            if (bar.name == name)
            {
                bar.GetComponent<HealthBarScript>().SetSize(newValue, speed);
            }
        }
    }
}
