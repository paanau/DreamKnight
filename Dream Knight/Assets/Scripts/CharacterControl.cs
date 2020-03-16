using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private GameObject gameController;
    private GameController main;
    private float range, attackCooldown;
    private bool inCombat, isAlive, waiting;
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
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x + (range * directionModifier), transform.position.y), -Vector2.up);
        if (hit.collider != null && !inCombat && !waiting && hit.collider.gameObject != gameObject)
        {
            if (gameObject.tag == "Enemy" && hit.collider.gameObject.tag == "Enemy")
            {
                waiting = true;
            }
            else
            {
                inCombat = true;
                main.InitiateCombat(gameObject, hit.collider.gameObject);
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
        inCombat = false;
    }

    public void LostCombat()
    {
        inCombat = false;
        GetComponent<BoxCollider2D>().enabled = false;
        transform.position = new Vector2(transform.position.x, 0);
        isAlive = false;
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
}
