using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    // Character is the top class; the player, enemies, bosses, are child classes

    public int id                   { get; set; }
    public string title             { get; set; }
    public bool isAlive             { get; set; }
    public bool isPrivate           { get; set; }
    public bool inCombat            { get; set; }
    public bool waiting             { get; set; }    
    public bool myTurn              { get; set; }
    public float baseMaxHP          { get; set; }
    public float currentHP          { get; set; }
    public float shieldHealth       { get; set; }       
    public float baseDamage         { get; set; }
    public float baseSpeed          { get; set; }
    public float range              { get; set; }    
    public float attackCooldown     { get; set; }    
    public float hitchance          { get; set; }    
    public float evasion            { get; set; }    
   // public Animator myAnimator      { get; set; }
  //  public float animatorSpeed      { get; set; }
    public float gameRunSpeed       { get; set; }
    public float speedModifiers     { get; set; }
    public float damageModifiers    { get; set; }
    public int directionModifier    { get; set; }
    public int experience           { get; set; }
    public int resistance           { get; set; }
    public string prefab            { get; set; }


    public Character()
    {
        // Basic statistics
        isAlive = true;
        baseMaxHP = 1000;
        currentHP = baseMaxHP;
        baseDamage = 20;
        baseSpeed = 1;
        range = 2f;
        inCombat = false;
        waiting = false;
        myTurn = false;
        range = 2f;
        hitchance = 10;
        evasion = 10;
        attackCooldown = 1f;
        directionModifier = -1;
        shieldHealth = 0;
        // try {
        //     myAnimator = GetComponent<Animator>();
        //     myAnimator.speed = 1;
        // } catch 
        // {
        //     Debug.Log("Animator component is missing");
        // }
        gameRunSpeed = 1;
        speedModifiers = 1;
        damageModifiers = 1;
        resistance = 0;
    }

}
