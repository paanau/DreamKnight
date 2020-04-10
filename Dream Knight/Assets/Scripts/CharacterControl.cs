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
    [SerializeField] private List<Ability> activeEffects = new List<Ability>();
    [SerializeField] private float range, attackCooldown, gameRunSpeed, baseSpeed, speedModifiers, damageModifiers, hitchance, evasion, resistance, regen;
    [SerializeField] private bool inCombat, isAlive, waiting, myTurn;
    [SerializeField] private float currentHP, baseMaxHP, baseDamage, shieldHealth;
    [SerializeField] private bool isPlayer;
    [SerializeField] private GameObject projectilePrefab, effectCircle, healthBar, xpOrbs;
    private string title;
    private GameObject[] healthbarComponents;
    [SerializeField] private int directionModifier, experience;

    // Start is called before the first frame update
    void Start()
    {
        SpawnCharacter(charTypeID);
    }

    public void SpawnCharacter(int typeID, float xPos = 15)
    {
        // Initialize character

        // Set position
        transform.position = transform.position.y > -20 ? transform.position : new Vector3(xPos, 0, 0);

        // Load abilities from the database
        try {
            character = GameObject.Find("GameController").GetComponent<CharacterDatabase>().FetchCharacterById(typeID);
            //Debug.Log(character.title + "'s turn: " + character.myTurn);
        }
        catch {
            Debug.Log("Character with id " + typeID + " not found! By: " + gameObject.name);
        }

        title = character.title;
        isAlive = character.isAlive;
        baseMaxHP = character.baseMaxHP;
        currentHP = character.currentHP;
        baseDamage = character.baseDamage;
        baseSpeed = character.baseSpeed;
        inCombat = character.inCombat;
        waiting = character.waiting;
        range = character.range;
        hitchance = character.hitchance;
        evasion = character.evasion;
        attackCooldown = character.attackCooldown;
        directionModifier = character.directionModifier;
        gameRunSpeed = character.gameRunSpeed;
        speedModifiers = character.speedModifiers;
        damageModifiers = character.damageModifiers;
        experience = character.experience;
        resistance = character.resistance;

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
                    //myAnimator.SetTrigger("enterCombat");
                    myAnimator.speed = 1f / attackCooldown;
                }
            }
            ManageAbilities();
        }
        EffectTick();
    }

    public void SetGameController(GameObject go)
    {
        gameController = go;
        main = go.GetComponent<GameController>();
        InitialiseHealthbars();
        myAnimator = GetComponent<Animator>();
    }

    public float GetHitChance()
    {
        return hitchance;
    }

    public float GetEvasion()
    {
        return evasion;
    }

    public float DamageDealt()
    {
        return baseDamage * damageModifiers;
    }

    public bool DamageTaken(float damage, int crit)
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
        //if (damage > baseMaxHP)
        //{
        //    CriticalDeath();
        //}
        currentHP -= damage;
        StartCoroutine(FlashOnHit(damage, crit));
        SetHealthBar("barHealth", currentHP / baseMaxHP, 5);
        SetHealthBar("barDamage", currentHP / baseMaxHP, 1f);
        if (currentHP <= 0)
        {
            // Death happens here
            Death();
            inCombat = false;
            return false;
        }
        return true;
    }

    private void CriticalDeath()
    {

    }

    private void Death()
    {
        if (character.id != 0) StartCoroutine(BurstIntoTreats());
        else main.GameOver();

        GameObject ps = Instantiate(xpOrbs, transform.position, Quaternion.identity);
        ps.GetComponent<XPOrbScript>().Initialise(character.experience);
    }

    IEnumerator BurstIntoTreats()
    {
        healthBar.GetComponent<Canvas>().enabled = false;
        Vector3 size = transform.localScale;
        for (int i = 0; i < 10; i++)
        {
            transform.localScale *= 0.9f;
            yield return null;
        }
        transform.position = new Vector3(0, -100, 0);
        transform.localScale = size;
        healthBar.GetComponent<Canvas>().enabled = true;
        yield return null;
    }

    IEnumerator FlashOnHit(float damage, int crit)
    {
        SpriteRenderer toFlash = GetComponent<SpriteRenderer>();
        float flash = 1 - (damage * 2 / baseMaxHP);
        toFlash.color = new Color(1,flash,flash);
        Debug.Log("I am " + title + " and I am flashing to " + toFlash.color);
        yield return new WaitForSeconds(crit * 0.05f);
        toFlash.color = Color.white;
    }

    public void GrantExperience(int newXP)
    {
        experience += newXP;
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
        //myAnimator.SetTrigger("leaveCombat");
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

    public void ApplyEffect(Ability effect)
    {
        if (effect.buff)
        {
            activeEffects.Add(effect);
            //switch(effect.buffTarget)
            //{
            //    case "health":
            //        break;
            //    case "defense":
            //        break;
            //    case "attack":
            //        break;
            //    case "shield":
            //        break;
            //}
        }
        else
        {
            TriggerEffect(effect);
        }
    }

    private void EffectTick()
    {
        foreach (Ability effect in activeEffects)
        {
            if (effect.durationType == "time")
            {
                effect.interval -= Time.deltaTime * gameRunSpeed;
                if (effect.interval <= 0)
                {
                    effect.interval = effect.baseInterval;
                    effect.duration -= effect.interval;
                    TriggerEffect(effect);
                }
            }
        }
        activeEffects.RemoveAll(effect => effect.duration <= 0);
    }

    public void TriggerEffect(Ability effect)
    {

        switch(effect.buffTarget)
        {
            case "health":
                if (effect.buffType == "add")
                {
                    if (effect.strength >= 0) { currentHP += effect.strength; } else { DamageTaken(-effect.strength, 1); }
                }
                SetHealthBar("barHealth", currentHP / baseMaxHP, 5);
                SetHealthBar("barDamage", currentHP / baseMaxHP, 1f);
                Debug.Log(gameObject.name + " got " + effect.strength + " HP!");
                break;
            default:
                break;
        }
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
        //myAnimator.SetTrigger("idle");
    }

    public void AdvanceMe(float modifiers)
    {
        //myAnimator.SetTrigger("move");
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

    public float UseAbility(string dir)
    {
        Ability ab = SelectAbility(dir);
        if (dir == "Ranged")
        {
            Debug.Log("UA: " + ab.Debug());
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            ProjectileScript ps = projectile.GetComponent<ProjectileScript>();
            ps.GiveSettings(new Ability(ab));
            ps.SetAnimationSpeed(gameRunSpeed);
            
        }
        return ab.energyCost;
    }

    public Ability SelectAbility(string dir)
    {
        if (myAbilities.Count > 0)
        {
            return myAbilities.Find(a => a.type == dir);
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
            speedModifiers -= Time.deltaTime * gameRunSpeed;
        }
        else
        {
            speedModifiers = 1;
        }
        if (damageModifiers > 1)
        {
            damageModifiers -= Time.deltaTime * gameRunSpeed;
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
            if (bar.name == name || name == "all")
            {
                bar.GetComponent<HealthBarScript>().SetSize(newValue, speed);
            }
        }
    }
}
