﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    public int id { get; set; } // Ability unique ID.
    public string title { get; set; } // Ability name.
    public string type { get; set; } // Ability category: Ranged, Defense, Offense, Misc, Effect.
    public string target { get; set; } // Whom the ability targets: self, closest, direct...
    public string element { get; set; } // Which element the ability uses, if any: fire, water, air, earth, light, dark?
    public string buffTarget { get; set; } // Which attribute a buff affects.
    public string buffType { get; set; } // How does the buff affect? add(itive), multi(plicative), mixed (<- is applied flat bonus first, then multiplied)
    public string durationType { get; set; } // How is the length of the effect measured? instantaneous, use, time
    
    public bool buff { get; set; } // Is ability a buff for the user?

    public float energyCost { get; set; } // Energy cost of ability.
    public float strength { get; set; } // Strength of the ability, i.e. damage dealt to target.
    public float procChance { get; set; } // Chance of causing an additional effect on the target.
    public float procDuration { get; set; } // Duration of additional effect.
    public float procStrength { get; set; } // Strength of additional effect.
    public float procInterval { get; set; } // Interval between ticks of additional effect.
    public float interval { get; set; } // Interval between ticks of ability.
    public float baseInterval { get; set; }
    public float speed { get; set; } // Speed at which ability (generally the projectile) moves.
    public float range { get; set; } // Range of ability.
    public float multi { get; set; } // Multiplicative strength of buff.
    public float duration { get; set; } // Duration of the ability.

    public int procEffect { get; set; } // Ability ID that is triggered on the target.

    public Ability()
    {

    }

    public Ability(Ability ab)
    {
        id = ab.id;
        title = ab.title;
        try
        {
            type = (string)ab.type;
        }
        catch { };
        try
        {
            target = (string)ab.target;
        }
        catch { };
        try
        {
            element = (string)ab.element;
        }
        catch { };
        try
        {
            buffTarget = (string)ab.buffTarget;
        }
        catch { };
        try
        {
            buffType = (string)ab.buffType;
        }
        catch { };
        try
        {
            durationType = (string)ab.durationType;
        }
        catch { };
        try
        {
            buff = (bool)ab.buff;
        }
        catch { };
        try
        {
            energyCost = (float)ab.energyCost;
        }
        catch { };
        try
        {
            strength = (float)ab.strength;
        }
        catch { };
        try
        {
            procChance = (float)ab.procChance;
        }
        catch { };
        try
        {
            procDuration = (float)ab.procDuration;
        }
        catch { };
        try
        {
            procStrength = (float)ab.procStrength;
        }
        catch { };
        try
        {
            procInterval = (float)ab.procInterval;
        }
        catch { };
        try
        {
            interval = (float)ab.interval;
        }
        catch { };
        try
        {
            baseInterval = (float)ab.interval;
        }
        catch { };
        try
        {
            speed = (float)ab.speed;
        }
        catch { };
        try
        {
            range = (float)ab.range;
        }
        catch { };
        try
        {
            multi = (float)ab.multi;
        }
        catch { };
        try
        {
            duration = (float)ab.duration;
        }
        catch { };
        try
        {
            procEffect = (int)ab.procEffect;
        }
        catch { };
    }

    public string Debug()
    {
        string s = "";

        s += "ID: " + id + ", I am " + title + ", a " + type + " ability.\n";
        s += "I cost " + energyCost + " to use and reach " + range;

        return s;
    }
}
