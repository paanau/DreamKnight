using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public int charTypeID = 0;
    public Character character = null;
    private GameObject gameController;
    private Animator myAnimator;
    private GameController main;
    private ParticleSystem.Particle[] effectParticles;
    private List<Ability> myAbilities = new List<Ability>();
    [SerializeField] private float range, attackCooldown, gameRunSpeed, baseSpeed, speedModifiers, damageModifiers;
    [SerializeField] private bool inCombat, isAlive, waiting, myTurn;
    [SerializeField] private float currentHP, baseMaxHP, baseDamage, shieldHealth;
    [SerializeField] private bool isPlayer;
    [SerializeField] private GameObject projectilePrefab, effectCircle, healthBar;
    private GameObject[] healthbarComponents;
    private int directionModifier;

    // Start is called before the first frame update
    void Start()
    {
        SpawnCharacter(charTypeID);
    }

    public void SpawnCharacter(int typeID, float xPos = 20)
    {
    // Initialize character

    // Set position

    // Load abilities from the database
        try {
            character = GameObject.Find("GameController").GetComponent<CharacterDatabase>().FetchCharacterById(typeID);
            Debug.Log(character.title + "'s turn: " + character.myTurn);
        }
        catch {
            Debug.Log("Character with id " + typeID + " not found! By: " + gameObject.name);
        }

        isAlive = character.isAlive;
        baseMaxHP = character.baseMaxHP;
        currentHP = character.currentHP;
        baseDamage = character.baseDamage;
        baseSpeed = character.baseSpeed;
        inCombat = character.inCombat;
        waiting = character.waiting;
        range = character.range;
        attackCooldown = character.attackCooldown;
        directionModifier = character.directionModifier;
        gameRunSpeed = character.gameRunSpeed;
        speedModifiers = character.speedModifiers;
        damageModifiers = character.damageModifiers;

        InitialiseAbilities(typeID);
    }

    public void InitialiseAbilities(int charID)
    {
        AbilityDatabase adb = GameObject.Find("GameController").GetComponent<AbilityDatabase>();

        if (charID == 0)
        {
            myAbilities.Add(adb.FetchAbilityByType("Ranged"));
            myAbilities.Add(adb.FetchAbilityByType("Offense"));
            myAbilities.Add(adb.FetchAbilityByType("Defense"));
            myAbilities.Add(adb.FetchAbilityByType("Misc"));
        }
    }

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
        InitialiseHealthbars();
        myAnimator = GetComponent<Animator>();
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
            SetHealthBar("barShield", shieldHealth / baseMaxHP, 1f);
        }

        if (damage > baseMaxHP)
        {
            CriticalDeath();
        }
        currentHP -= damage;
        SetHealthBar("barHealth", currentHP / baseMaxHP, 5);
        SetHealthBar("barDamage", currentHP / baseMaxHP, 1f);
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

    public float UseAbility(string s)
    {
        Ability ab = SelectAbility(s);
        if (s == "w")
        {
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            ProjectileScript ps = projectile.GetComponent<ProjectileScript>();
            ps.GiveSettings((1 + myAnimator.speed) * 0.1f, 10, 100);
            ps.SetAnimationSpeed(gameRunSpeed);
            return ab.energyCost;
        }

        
        return 1;
    }

    private Ability SelectAbility(string s)
    {
        if (myAbilities.Count > 0)
        {
            switch (s)
            {
                case "w":
                    return myAbilities.Find(a => a.type == "Ranged");
                case "a":
                    return myAbilities.Find(a => a.type == "Defense");
                case "s":
                    return myAbilities.Find(a => a.type == "Misc");
                case "d":
                    return myAbilities.Find(a => a.type == "Offense");
            }

            return new Ability();
        }
        else
        {
            return new Ability();
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

        //if (speedModifiers > 1 || damageModifiers > 1 || shieldHealth > 0)
        //{
        //    float speedParticles = (speedModifiers - 1) * 10;
        //    float shieldParticles = shieldHealth / 5;
        //    float damageParticles = (damageModifiers - 1) * 2;
        //    float totalParticleCount = speedParticles + shieldParticles + damageParticles;

        //    float[] particleFloats = { speedParticles, shieldParticles, damageParticles };

        //    ParticleSystem ps = effectCircle.GetComponent<ParticleSystem>();
        //    var emis = ps.emission;
        //    emis.rateOverTime = Mathf.FloorToInt(totalParticleCount);

        //    var psmain = ps.main;
        //    Color[] assignableColours = { Color.green, Color.blue, Color.red };

            

        //    Gradient newGradient = new Gradient();
        //    GradientColorKey[] colorKey = new GradientColorKey[3];
        //    GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
        //    float runningTally = 0;
        //    for (int i = 0; i < particleFloats.Length; i++)
        //    {
        //        if (particleFloats[i] > 0)
        //        {
        //            colorKey[i].time = (particleFloats[i] + runningTally) / totalParticleCount;
        //            colorKey[i].color = assignableColours[i];
        //            alphaKey[i].alpha = 1;
        //            alphaKey[i].time = (particleFloats[i] + runningTally) / totalParticleCount;
        //            runningTally += particleFloats[i];
        //        }
        //        else
        //        {
        //            colorKey[i].time = (float)i / 2;
        //            colorKey[i].color = newGradient.Evaluate((float)i / 2);
        //            alphaKey[i].alpha = 1;
        //            alphaKey[i].time = (float)i / 2;
        //        }
        //    }
        //    newGradient.SetKeys(colorKey, alphaKey);
        //    psmain.startColor = newGradient;

        //    //float pstime = ps.time * 10;
        //    //if (pstime > 1)
        //    //{
        //    //    pstime -= Mathf.Floor(pstime);
        //    //}
        //    //if (pstime < speedParticles / totalParticleCount)
        //    //{
        //    //    psmain.startColor = assignableColours[0];
        //    //}
        //    //else if (pstime < (shieldParticles + speedParticles) / totalParticleCount)
        //    //{
        //    //    psmain.startColor = assignableColours[1];
        //    //}
        //    //else
        //    //{
        //    //    psmain.startColor = assignableColours[2];
        //    //}

        //}
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
