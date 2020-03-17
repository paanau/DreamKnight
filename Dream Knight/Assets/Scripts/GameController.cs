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
        playerController.SetGameController(gameObject);

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<CharacterControl>().SetGameController(gameObject);
        }

        StartPlayerTurn();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameActive)
        {
            if (playerTurn)
            {
                if (!chargeActive)
                {
                    PowerUpSequence();
                }
                if (chargeActive)
                {
                    if (chargeEnergy > 0)
                    {
                        chargeEnergy -= playerDepleteRate;
                        PlayerRush();
                    }
                    else
                    {
                        EndPlayerTurn();
                    }
                }
            }
            if (!playerTurn)
            {
                if (enemyEnergy > 0)
                {
                    EnemiesAct();
                }
                else
                {
                    StartPlayerTurn();
                }
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
        currentMeleeTargetController.StopWalking();
        playerController.StopWalking();
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
            CharacterControl ec = enemy.GetComponent<CharacterControl>();
            if (ec.IsAlive() && ec.IsNotInCombat())
            {
                ec.StopWaiting();
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
        ChangeTurns();
    }
    
    private void EndPlayerTurn()
    {
        chargeEnergy = 0;
        playerTurn = false;
        chargeActive = false;
        ChangeTurns();
    }

    private void ChangeTurns()
    {
        playerController.SetMyTurn(playerTurn);
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            CharacterControl ec = enemy.GetComponent<CharacterControl>();
            if (ec.IsAlive())
            {
                ec.SetMyTurn(!playerTurn);
            }
        }
    }

    private void PowerUpSequence()
    {
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

    private void PlayerRush()
    {
        if (!inCombat)
        {
            playerController.AdvanceMe(rushSpeedBoost * chargeEnergy / maxEnergy);

            if (playerCharacter.transform.position.x >= mainCamera.transform.position.x - 3)
            {
                mainCamera.transform.position = new Vector3(playerCharacter.transform.position.x + 3, 2, -10);
            }
        }

        else
        {
            DoCombat();
        }
    }

    private void EnemiesAct()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            CharacterControl ec = enemy.GetComponent<CharacterControl>();
            if (ec.CanAdvance())
            {
                ec.AdvanceMe(1);
            }
            if (ec.CanFight())
            {
                DoCombat();
            }
        }
        enemyEnergy -= enemyDepleteRate;
    }
}
