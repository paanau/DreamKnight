using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private GameObject gameController;
    private Animator myAnimator;
    private GameController main;
    private float range, attackCooldown, gameRunSpeed, baseSpeed, speedModifiers, damageModifiers;
    private bool inCombat, isAlive, waiting, myTurn;
    [SerializeField] private int currentHP, baseMaxHP, baseDamage, shieldHealth;
    [SerializeField] private bool isPlayer;
    [SerializeField] private GameObject fireB;
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

    // Update is called once per frame
    void Update()
    {
        if (myTurn && isAlive && !inCombat)
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + (range * directionModifier), 0), -Vector2.up);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
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
            if (speedModifiers > 1)
            {
                speedModifiers -= Time.deltaTime;
            } else
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
                damage -= shieldHealth;
                shieldHealth = 0;
            }
        }

        if (damage > baseMaxHP)
        {
            CriticalDeath();
        }
        currentHP -= damage;
        if (currentHP <= 0)
        {
            transform.Rotate(0, 0, -90);
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

    public void TriggerBloodEffect()
    {
        GetComponent<ParticleSystem>().Play();
    }

    public bool IsAlive()
    {
        return isAlive;
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
            speedModifiers = 2;
        }
        if (s == "a")
        {
            shieldHealth += 100;
        }
        if (s == "d")
        {
            damageModifiers = 10;
        }
    }
}
