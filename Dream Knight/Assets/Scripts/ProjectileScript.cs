using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float gameRunSpeed;
    private Animator myAnimator;
    private Ability myAbility;

    // Start is called before the first frame update
    void Awake()
    {
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (myAbility.range >= 0)
        {
            transform.Translate(new Vector2(myAbility.speed * gameRunSpeed, 0));
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, 0), -Vector2.up);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                if (hit.collider.gameObject.tag == "Enemy")
                {
                    HitTarget(hit.collider.gameObject);
                }
            }
            myAbility.range -= Time.deltaTime * gameRunSpeed;
        }
        else
        {
            RemoveMe();
        }
    }

    public void GiveSettings(Ability whatAmI)
    {
        myAbility = whatAmI;
        myAnimator = GetComponent<Animator>();
    }

    private void HitTarget(GameObject go)
    {
        CharacterControl targetCC = go.GetComponent<CharacterControl>();
        targetCC.DamageTaken((int)myAbility.strength);
        if (Random.Range(0f, 100f) < myAbility.procChance)
        {
            Ability procEffect = GameObject.Find("GameController").GetComponent<AbilityDatabase>().FetchAbilityById(myAbility.procEffect);
            procEffect.duration = myAbility.procDuration;
            procEffect.interval = myAbility.procInterval;
            procEffect.baseInterval = myAbility.procInterval;
            procEffect.strength = myAbility.procStrength;
            targetCC.ApplyEffect(procEffect);
        }
        RemoveMe();
    }

    public void SetAnimationSpeed(float newSpeed)
    {
        myAnimator.speed = newSpeed;
        gameRunSpeed = newSpeed;
    }

    private void RemoveMe()
    {
        myAnimator.Play("FireboltHit");
        myAbility.range = -1;
    }

    public void FinishMe()
    {
        Destroy(gameObject);
    }
}
