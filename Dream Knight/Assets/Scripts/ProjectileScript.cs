using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [SerializeField] private float mySpeed, myRange, gameRunSpeed;
    [SerializeField] private int myDamage;
    private Animator myAnimator;

    // Start is called before the first frame update
    void Awake()
    {
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (myRange > 0)
        {
            transform.Translate(new Vector2(mySpeed * gameRunSpeed, 0));

            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, 0), -Vector2.up);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                if (hit.collider.gameObject.tag == "Enemy")
                {
                    HitTarget(hit.collider.gameObject);
                }
            }
            myRange -= Time.deltaTime * gameRunSpeed;
        }
        else
        {
            RemoveMe();
        }
    }

    public void GiveSettings(float speed, float range, int damage)
    {
        mySpeed = speed;
        myRange = range;
        myDamage = damage;
        myAnimator = GetComponent<Animator>();
    }

    private void HitTarget(GameObject go)
    {
        CharacterControl targetCC = go.GetComponent<CharacterControl>();
        targetCC.DamageTaken(myDamage);
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
        myRange = 0;
    }

    public void FinishMe()
    {
        Destroy(gameObject);
    }
}
