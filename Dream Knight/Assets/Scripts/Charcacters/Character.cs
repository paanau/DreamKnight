using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // Character is the top class; the player, enemies, bosses, are child classes

    public int id                   { get; set; }
    public string title             { get; set; }
    public bool isAlive             { get; set; }
    public bool inCombat            { get; set; }
    public bool waiting             { get; set; }    
    public float baseMaxHP          { get; set; }
    public float currentHP          { get; set; }
    public float shieldHealth       { get; set; }       
    public float baseDamage         { get; set; }
    public float baseSpeed          { get; set; }
    public float range              { get; set; }    
    public float attackCooldown     { get; set; }    
    public Animator myAnimator      { get; set; }
    public float animatorSpeed      { get; set; }
    public int gameRunSpeed         { get; set; }
    public int speedModifiers       { get; set; }
    public int damageModifiers      { get; set; }
    public int directionModifier    { get; set; }


    public Character()
    {
        // Basic statistics
        baseMaxHP = 1000;
        currentHP = baseMaxHP;
        baseDamage = 30;
        baseSpeed = 1;
        range = 2f;
        inCombat = false;
        waiting = false;
        range = 2f;
        attackCooldown = 1f;
        directionModifier = -1;
        try {
            myAnimator = GetComponent<Animator>();
            myAnimator.speed = 1;
        } catch 
        {
            Debug.Log("Animator component is missing");
        }
        gameRunSpeed = 1;
        speedModifiers = 1;
        damageModifiers = 1;
        
    }

    void Start () 
    {

    }

    

}
