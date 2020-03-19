using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private GameObject gameController;
    private Animator myAnimator;
    private GameController main;
    private float range, attackCooldown;
    private bool inCombat, isAlive, waiting, myTurn;
    public int currentHP, maxHP, myDamage, mySpeed;
    public bool isPlayer;
    private int directionModifier;
    // Start is called before the first frame update
    void Start()
    {
        isAlive = true;
        maxHP = 1000;
        currentHP = maxHP;
        myDamage = 30;
        mySpeed = 1;
        inCombat = false;
        waiting = false;
        range = 2f;
        attackCooldown = 1f;
        directionModifier = -1;
        if (isPlayer)
        {
            myDamage = 100;
            attackCooldown = 0.6f;
            directionModifier = 1;
        }
        myAnimator = GetComponent<Animator>();
        myAnimator.speed = 1;
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
        }
    }

    public void SetGameController(GameObject go)
    {
        gameController = go;
        main = go.GetComponent<GameController>();
    }

    public int DamageDealt()
    {
        return myDamage;
    }

    public bool DamageTaken(int damage)
    {
        if (damage > maxHP)
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

    public int GetSpeed()
    {
        return mySpeed;
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
        transform.Translate(0.1f * modifiers * mySpeed * directionModifier, 0, 0);
        myAnimator.speed = modifiers * mySpeed + 1;
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
        myAnimator.speed = newSpeed;
    }
}
