using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private bool gameActive, playerTurn, chargeActive, poweringUp, inCombat;
    public int chargeEnergy, maxEnergy, enemyEnergy, playerChargeRate, playerDepleteRate, enemyDepleteRate;
    private float rushSpeedBoost, rushDamageBoost, playerAttackCooldown, targetAttackCooldown;
    public GameObject playerCharacter, mainCamera;
    private CharacterControl playerController, currentMeleeTargetController;
    private GameObject currentMeleeTarget;

    // Start is called before the first frame update
    void Start()
    {
        gameActive = true;
        playerTurn = true;
        chargeActive = false;
        poweringUp = false;
        chargeEnergy = 0;
        maxEnergy = 10000;
        rushSpeedBoost = 5f;
        rushDamageBoost = 2f;
        playerChargeRate = 100;
        playerDepleteRate = 10;
        enemyDepleteRate = 20;
        playerController = playerCharacter.GetComponent<CharacterControl>();
        playerController.SetGameController(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTurn && gameActive)
        {
            if (!chargeActive) {
                if (Input.GetAxis("Charge") > 0)
                {
                    if (chargeEnergy < maxEnergy)
                    {
                        chargeEnergy += playerChargeRate;
                    }
                    poweringUp = true;
                }
                if (Input.GetAxis("Charge") <= 0 && poweringUp)
                {
                    enemyEnergy = chargeEnergy;
                    poweringUp = false;
                    chargeActive = true;
                }
            }
            if (chargeActive)
            {
                if (chargeEnergy > 0)
                {
                    if (!inCombat)
                    {
                        playerCharacter.transform.Translate(0.1f * rushSpeedBoost * chargeEnergy / maxEnergy, 0, 0);
                        if (playerCharacter.transform.position.x >= mainCamera.transform.position.x + 1)
                        {
                            mainCamera.transform.Translate(0.1f * rushSpeedBoost * chargeEnergy / maxEnergy, 0, 0);
                        }
                    }
                    
                    else
                    {
                        DoCombat();
                    }

                    chargeEnergy -= playerDepleteRate;
                }
                else
                {
                    EndPlayerTurn();
                }
            }
        }
        if (!playerTurn && gameActive)
        {
            if (enemyEnergy > 0)
            {
                foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                {
                    if (enemy.GetComponent<CharacterControl>().IsAlive() && enemy.GetComponent<CharacterControl>().IsNotInCombat() && enemy.GetComponent<CharacterControl>().IsNotWaiting())
                    {
                        enemy.transform.Translate(-0.1f * enemy.GetComponent<CharacterControl>().GetSpeed(), 0, 0);
                    }
                    if (!enemy.GetComponent<CharacterControl>().IsNotInCombat() && enemy.GetComponent<CharacterControl>().IsAlive())
                    {
                        DoCombat();
                    }
                }
                enemyEnergy -= enemyDepleteRate;
            }
            else
            {
                StartPlayerTurn();
            }
        }
    }

    public void InitiateCombat(GameObject attacker, GameObject target)
    {
        if (target == playerCharacter)
        {
            target = attacker;
        }
        currentMeleeTarget = target;
        currentMeleeTargetController = target.GetComponent<CharacterControl>();
        inCombat = true;
        playerAttackCooldown = 0;
        targetAttackCooldown = 0;
    }

    private void MeleeTick()
    {
        playerAttackCooldown -= Time.deltaTime;
        targetAttackCooldown -= Time.deltaTime;
    }

    private void PlayerDied()
    {
        chargeEnergy = 0;
        chargeActive = false;
        gameActive = false;
    }

    private void ActivateWaitingEnemies()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy.GetComponent<CharacterControl>().IsAlive() && enemy.GetComponent<CharacterControl>().IsNotInCombat())
            {
                enemy.GetComponent<CharacterControl>().StopWaiting();
            }
        }
    }

    private void DoCombat()
    {
        MeleeTick();
        if (playerAttackCooldown <= 0)
        {
            if (!currentMeleeTargetController.DamageTaken(Mathf.FloorToInt(playerController.DamageDealt() * rushDamageBoost)))
            {
                EndCombat();
                playerController.WonCombat();
                currentMeleeTargetController.LostCombat();
                targetAttackCooldown = 999f;
            }
            playerAttackCooldown += playerController.GetCooldown();
            currentMeleeTargetController.TriggerBloodEffect();
        }

        if (targetAttackCooldown <= 0)
        {
            if (!playerController.DamageTaken(currentMeleeTargetController.DamageDealt()))
            {
                EndCombat();
                playerController.LostCombat();
                currentMeleeTargetController.WonCombat();
                PlayerDied();
            }
            targetAttackCooldown += currentMeleeTargetController.GetCooldown();
            playerController.TriggerBloodEffect();
        }
    }

    private void EndCombat()
    {
        inCombat = false;
    }

    private void StartPlayerTurn()
    {
        enemyEnergy = 0;
        playerTurn = true;
    }
    
    private void EndPlayerTurn()
    {
        chargeEnergy = 0;
        playerTurn = false;
        chargeActive = false;
    }
}
