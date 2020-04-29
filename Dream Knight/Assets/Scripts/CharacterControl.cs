using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public int charTypeID = 0;
    public Character character = null;
    private GameObject gameController;
    private CharacterControl meleeTarget;
    public Animator myAnimator;
    private GameController main;
    private ParticleSystem.Particle[] effectParticles;
    private List<Ability> myAbilities = new List<Ability>();
    [SerializeField] private List<Ability> activeEffects = new List<Ability>();
    [SerializeField] private float range, attackCooldown, gameRunSpeed, baseSpeed, speedModifiers, damageModifiers, evadeModifiers, hitchance, evasion, resistance, regen;
    [SerializeField] private bool inCombat, isAlive, waiting, myTurn, gameActive;
    [SerializeField] private float currentHP, baseMaxHP, baseDamage, shieldHealth;
    [SerializeField] private bool isPlayer;
    public bool readyToSpawn;
    [SerializeField] private GameObject projectilePrefab, effectCircle, healthBar, xpOrbs;
    private string title;
    private GameObject[] healthbarComponents;
    [SerializeField] private int directionModifier, experience;
    private Coroutine mettle;

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
        damageModifiers = 1;
        evadeModifiers = 1; // TODO adapt to levelling up et al.
        experience = character.experience;
        resistance = character.resistance;
        readyToSpawn = false;
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
    void FixedUpdate()
    {
        if (gameActive)
        {
            if (myTurn && isAlive && !inCombat && !waiting)
            {
                AdvanceMe();
                ScanForTarget();
            }
            EffectTick();
        }
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
        if (activeEffects.Contains(activeEffects.Find(a => a.buffTarget.Equals("evasion"))))
        {
            evadeModifiers = TriggerEffect(activeEffects.Find(a => a.buffTarget.Equals("evasion")));
        }
        else
        {
            evadeModifiers = 1; // TODO: adapt to levelling up et al.
        }
        return evasion * evadeModifiers;
    }

    public float DamageDealt()
    {
        if (activeEffects.Contains(activeEffects.Find(a => a.buffTarget.Equals("damage"))))
        {
            damageModifiers = TriggerEffect(activeEffects.Find(a => a.buffTarget.Equals("damage")));
        }
        else
        {
            damageModifiers = 1; // TODO: adapt to levelling up et al.
        }
        return baseDamage * damageModifiers;
    }

    public void MeleeCombat(GameObject target)
    {
        meleeTarget = target.GetComponent<CharacterControl>();
        mettle = StartCoroutine(Mettle());
        SetAnimationTrigger("Attack");
    }

    IEnumerator Mettle()
    {
        float compensationTime = 0;
        while (isAlive && meleeTarget.IsAlive())
        {
            if (myAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals(character.prefab + "_Hurt"))
            {
                compensationTime += Time.deltaTime;
                yield return null;
            }

            //meleeTarget.TriggerBloodEffect(Mathf.FloorToInt(damage), gameRunSpeed);

            float adjustedTime = attackCooldown - compensationTime;
            compensationTime = 0;
            yield return new WaitForSeconds(adjustedTime);
            if (isAlive) SetAnimationTrigger("Attack");
        }
    }

    private void Strike()
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
            else
            {
                meleeTarget.SetAnimationTrigger("GetHit");
            }
        }
        else
        {
            damage = 0;
            // Miss
        }
    }

    public void SetAnimationTrigger(string newState)
    {
        myAnimator.SetTrigger(newState);
    }

    public bool DamageTaken(float damage, int crit)
    {
        if (isAlive)
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
        return false;
    }

    private void ScanForTarget()
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + (range * directionModifier), transform.position.y), -Vector2.up, 100, LayerMask.GetMask("Default"));
        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            Debug.Log(hit.collider.gameObject.name + " " + myAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
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
                //myAnimator.speed = 1f / attackCooldown;
            }
        }
        else
        {
            
        }
    }

    private void CriticalDeath()
    {

    }

    private void Death()
    {
        SetAnimationTrigger("Die");
        if (character.id != 0) StartCoroutine(BurstIntoTreats());
        else main.GameOver();
        if (isAlive)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            main.EndCombat(GetComponent<CharacterControl>());
            GameObject ps = Instantiate(xpOrbs, transform.position, Quaternion.identity);
            ps.GetComponent<XPOrbScript>().Initialise(experience);
            isAlive = false;
        }
    }

    IEnumerator BurstIntoTreats()
    {
        healthBar.SetActive(false);
        yield return new WaitForSeconds(5);
        
        Vector3 size = transform.localScale;
        for (int i = 0; i < 10; i++)
        {
            transform.localScale *= 0.9f;
            yield return null;
        }
        transform.position = new Vector3(0, -100, 0);
        transform.localScale = size;
        healthBar.SetActive(true);
        GetComponent<BoxCollider2D>().enabled = true;
        readyToSpawn = true;
    }

    IEnumerator FlashOnHit(float damage, int crit)
    {
        foreach (SpriteRenderer toFlash in GetComponentsInChildren<SpriteRenderer>())
        { 
            if (!toFlash.gameObject.CompareTag(gameObject.tag)) { continue; }
            float flash = 0.5f;
            toFlash.color = new Color(1, flash, flash);
        }
        yield return new WaitForSeconds(crit * 0.05f);
        foreach (SpriteRenderer toFlash in GetComponentsInChildren<SpriteRenderer>())
        {
            if (!toFlash.gameObject.CompareTag(gameObject.tag)) { continue; }
            toFlash.color = Color.white;
        }
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
        StopCoroutine(mettle);

        if (isAlive && myTurn) { SetAnimationTrigger("Move"); }
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
        }

        TriggerEffect(effect);

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
        activeEffects.RemoveAll(effect => effect.duration < 0);
    }

    public float TriggerEffect(Ability effect)
    {

        switch(effect.buffTarget)
        {
            case "health":
                if (effect.buffType.Equals("add"))
                {
                    if (effect.strength >= 0) { currentHP += effect.strength; } else { DamageTaken(-effect.strength, 1); }
                }
                if (currentHP > baseMaxHP) { currentHP = baseMaxHP; }
                SetHealthBar("barHealth", currentHP / baseMaxHP, 5);
                SetHealthBar("barDamage", currentHP / baseMaxHP, 1f);
                return 0;
            case "evasion":
                if (effect.buffType.Equals("add") || effect.buffType.Equals("multi"))
                {
                    evadeModifiers += effect.strength;
                }

                if (effect.buffType.Equals("multi") || effect.buffType.Equals("mixed"))
                {
                    evadeModifiers *= effect.multi;
                }
                if (effect.durationType.Equals("use"))
                {
                    effect.duration--;
                }
                if (effect.duration >= 0)
                {
                    GameObject.Find("ShieldEffect").GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    GameObject.Find("ShieldEffect").GetComponent<SpriteRenderer>().enabled = false;
                }
                return evadeModifiers;
            case "damage":
                if (effect.buffType.Equals("add") || effect.buffType.Equals("multi"))
                {
                    damageModifiers += effect.strength;
                }

                if (effect.buffType.Equals("multi") || effect.buffType.Equals("mixed"))
                {
                    damageModifiers *= effect.multi;
                }
                if (effect.durationType.Equals("use"))
                {
                    effect.duration--;
                }
                if (effect.duration >= 0)
                {
                    GameObject.Find("SwordEffect").GetComponent<SpriteRenderer>().enabled = true;
                } else
                {
                    GameObject.Find("SwordEffect").GetComponent<SpriteRenderer>().enabled = false;
                }
                return damageModifiers;
            case "shield":
                if (effect.buffType.Equals("add") || effect.buffType.Equals("multi"))
                {
                    shieldHealth += effect.strength;
                }

                if (effect.buffType.Equals("multi") || effect.buffType.Equals("mixed"))
                {
                    shieldHealth *= effect.strength;
                }
                SetHealthBar("barShield", shieldHealth / baseMaxHP, 1f);
                return 0;
            default:
                return 0;
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
        //SetAnimationTrigger("idle");
    }

    public void AdvanceMe()
    {
        transform.Translate((speedModifiers * baseSpeed * directionModifier + baseSpeed) * Time.deltaTime, 0, 0);
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
        //myAnimator.speed = newSpeed * speedModifiers;
        gameRunSpeed = newSpeed * speedModifiers;
        //ParticleSystem myBloodLoss = GetComponent<ParticleSystem>();
        //var main = myBloodLoss.main;
        //main.simulationSpeed = newSpeed;
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
        if (ab.buff)
        {
            if (ab.target.Equals("self"))
            {
                ApplyEffect(new Ability(ab));
            }
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
        SetHealthBar("barHealth", 1, 0);
        SetHealthBar("barDamage", 1, 0);
    }

    private void SetHealthBar(string name, float newValue, float speed)
    {
        foreach (GameObject bar in healthbarComponents)
        {
            if (bar.name.Equals(name) || name.Equals("all"))
            {
                bar.GetComponent<HealthBarScript>().SetSize(newValue, speed);
                if (name.Equals("barShield"))
                {
                    SpriteRenderer shieldBubble = GameObject.Find("ShieldBubble").GetComponent<SpriteRenderer>();
                    shieldBubble.color = new Color(shieldBubble.color.r, shieldBubble.color.g, shieldBubble.color.b, (float)shieldHealth/(float)baseMaxHP);
                }
                break;
            }
        }
    }

    public void SetGameActive(bool b)
    {
        gameActive = b;
    }
}
