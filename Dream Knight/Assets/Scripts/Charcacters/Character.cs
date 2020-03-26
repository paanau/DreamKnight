using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // Character is the top class; the player, enemies, bosses, and treasures are children

    public int id           { get; set; }
    public string title     { get; set; }
    public bool isAlive     { get; set; }
    public bool inCombat    { get; set; }
    public bool waiting     { get; set; }    
    public int gameRunSpeed { get; set; }
    public int baseMaxHP    { get; set; }
    public int currentHP    { get; set; }
    public int baseDamage   { get; set; }
    public int baseSpeed    { get; set; }
    public float range      { get; set; }    
    public int speedModifiers { get; set; }
    public int damageModifiers { get; set; }

    public Character()
    {
        baseMaxHP = 1000;
        currentHP = baseMaxHP;
        baseDamage = 30;
        baseSpeed = 1;
        range = 2f;
        //attackCooldown = 1f;
        //directionModifier = -1;

        //myAnimator = GetComponent<Animator>();
        //myAnimator.speed = 1;
    }

    void Start () 
    {
        Debug.Log(new Character().baseMaxHP);
        Debug.Log(new Enemy().baseMaxHP);
    }

    

}
