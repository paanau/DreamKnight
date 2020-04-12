using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public int charTypeID = 0;
    public Character character = null;
    private GameObject gameController;
    private CharacterControl meleeTarget;
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

        if (typeID > 0)
        {
            gameObject.name = title;
        }
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
        if (myTurn && isAlive)
        {
            if (!inCombat)
            {
                if (!waiting)
                {
                    AdvanceMe();
                    ScanForTarget();
                }
            }
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

    public void MeleeCombat(GameObject target)
    {
        meleeTarget = target.GetComponent<CharacterControl>();
        StartCoroutine(Mettle());
    }

    IEnumerator Mettle()
    {
        while (isAlive && meleeTarget.IsAlive())
        {
            float damage = DamageDealt();
            //if (main.playerTurn) { damage *= rushDamageBoost; }
            int critDegree = 1;
            float toHit = GetHitChance();
            float evade = meleeTarget.GetEvasion();
            float roll = Random.Range(0, 100);
            if (toHit + roll >= evade) // 50 + 90 >= 10
            {
                for (int i = 1; i < 5; i++)
                {
                    if (toHit + roll >= i * 100) // 140 >= 0, 100
                    {
                        critDegree *= 2;
                    }
                }
                damage *= critDegree;
                if (!meleeTarget.DamageTaken(damage, critDegree)) // Hit
                {
                    main.EndCombat(meleeTarget);
                    meleeTarget.EndCombat();
                    EndCombat();
                }
            }
            else
            {
                damage = 0;
                // Miss
            }
            meleeTarget.TriggerBloodEffect(Mathf.FloorToInt(damage), gameRunSpeed);
            yield return new WaitForSeconds(attackCooldown);
        }
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
        if (currentHP <= 0 && isAlive)
        {
            // Death happens here
            Death();
            inCombat = false;
            return false;
        }
        return true;
    }

    private void ScanForTarget()
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
                MeleeCombat(hit.collider.gameObject);
                meleeTarget = hit.collider.gameObject.GetComponent<CharacterControl>();
                meleeTarget.MeleeCombat(gameObject);
                main.InitiateCombat(gameObject, hit.collider.gameObject);
                //myAnimator.SetTrigger("enterCombat");
                myAnimator.speed = 1f / attackCooldown;
            }
        }
    }

    private void CriticalDeath()
    {

    }

    private void Death()
    {
        if (character.id != 0) StartCoroutine(BurstIntoTreats());
        else main.GameOver();
        if (isAlive)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            main.EndCombat(GetComponent<CharacterControl>());
            //GameObject ps = Instantiate(xpOrbs, transform.position, Quaternion.identity);
            //ps.GetComponent<XPOrbScript>().Initialise(experience);
            isAlive = false;
        }
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
    }

    IEnumerator FlashOnHit(float damage, int crit)
    {
        SpriteRenderer toFlash = GetComponent<SpriteRenderer>();
        float flash = 0.5f;
        toFlash.color = new Color(1,flash,flash);
        yield return new WaitForSeconds(crit * 0.05f);
        toFlash.color = Color.white;
    }

    public void GrantExperience(int newXP)
    {
        if (newXP > 0)
        {
            experience += newXP;
            main.UpdateExperience(newXP);
        }
    }

    public void EndCombat()
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

    public void AdvanceMe()
    {
        //myAnimator.SetTrigger("move");
        transform.Translate(0.1f * speedModifiers * baseSpeed * directionModifier, 0, 0);
        myAnimator.speed = speedModifiers * baseSpeed + 1;
    }

    public void AdvanceModifiers(float modifiers)
    {
        speedModifiers = modifiers;
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
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            ProjectileScript ps = projectile.GetComponent<ProjectileScript>();
            ps.GiveSettings(new Ability(ab));
            ps.SetAnimationSpeed(gameRunSpeed);
            
        }
        Debug.Log("UA: " + ab.Debug());
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
