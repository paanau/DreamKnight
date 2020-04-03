using System.Collections;
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

    public float damage { get; set; } // Damage dealt to target.
    public float procChance { get; set; } // Chance of causing an additional effect on the target.
    public float procDuration { get; set; } // Duration of additional effect.
    public float procStrength { get; set; } // Strength of additional effect.
    public float procInterval { get; set; } // Interval between ticks of additional effect.
    public float interval { get; set; } // Interval between ticks of ability.
    public float speed { get; set; } // Speed at which ability (generally the projectile) moves.
    public float range { get; set; } // Range of ability.
    public float flatStrength { get; set; } // Additive strength of buff.
    public float multiStrength { get; set; } // Multiplicative strength of buff.
    public float duration { get; set; } // Duration of the ability.

    public int procEffect { get; set; } // Ability ID that is triggered on the target.

    public Ability()
    {

    }
    public Ability(int i)
    {
        id = i;
    }
}
