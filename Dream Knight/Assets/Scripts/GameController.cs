using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private bool gameActive, playerTurn, chargeActive, poweringUp, inCombat, firstTurn;
    private bool chargeButton, slowdownActive;
    public float chargeEnergy, maxEnergy, enemyEnergy, playerChargeRate, playerDepleteRate, enemyDepleteRate;
    [SerializeField] private float rushSpeedBoost, rushDamageBoost, playerAttackCooldown, targetAttackCooldown, gameRunSpeed;
    [SerializeField] private GameObject playerCharacter, mainCamera, pauseSelectionUI, enemyPrefab;
    private CharacterControl playerController, currentMeleeTargetController;
    private GameObject currentMeleeTarget;
    private PlayerInput playerInput;
    private List<GameObject> enemies = new List<GameObject>();


    void Awake(){

        SpawnCharacters();

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<CharacterControl>().SetGameController(gameObject);
            // Populate list of enemies. TODO autogeneration of enemies
            enemies.Add(enemy);
        }
       
        // Sort list in order of x-position
        if (enemies.Count > 0) {
            enemies.Sort(delegate(GameObject a, GameObject b) {
            return (a.transform.position.x).CompareTo(b.transform.position.x);
            });
        }

        playerController = playerCharacter.GetComponent<CharacterControl>();
        playerController.SetGameController(gameObject);

        StartPlayerTurn();
    }
    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        gameActive = true;
        playerTurn = true;
        chargeActive = false;
        poweringUp = false;
        firstTurn = true;
        slowdownActive = false;
        chargeEnergy = 0;
        maxEnergy = 10000;
        rushSpeedBoost = 5f;
        rushDamageBoost = 2f;
        playerChargeRate = 100;
        playerDepleteRate = 10;
        enemyDepleteRate = 20;
        gameRunSpeed = 1f;
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
                        chargeEnergy -= playerDepleteRate * gameRunSpeed;
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

    private void SpawnCharacters()
    {
        for (int i = 1; i < 10; i++)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, new Vector3(i*15, 0, 0), Quaternion.identity);
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
        playerAttackCooldown = playerController.GetCooldown();
        targetAttackCooldown = currentMeleeTargetController.GetCooldown();
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
        foreach (GameObject enemy in enemies)
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
            playerAttackCooldown += playerController.GetCooldown() * gameRunSpeed;
            currentMeleeTargetController.TriggerBloodEffect(Mathf.FloorToInt(playerController.DamageDealt() * rushDamageBoost), gameRunSpeed);
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
            targetAttackCooldown += currentMeleeTargetController.GetCooldown() * gameRunSpeed;
            playerController.TriggerBloodEffect(currentMeleeTargetController.DamageDealt(), gameRunSpeed);
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
        gameRunSpeed = 1;
        ChangeSpeeds();
        firstTurn = false;
    }

    public void OnCharge()
    {
        chargeButton = !chargeButton;

        if (playerTurn && chargeActive)
        {
            PauseForAbility();
        }
        if (!playerTurn)
        {
            PauseForItem();
        }
    }

    public void OnRestart()
    {
        SceneManager.LoadScene(0);
    }

    public void OnMove(InputValue iv)
    {
        Vector2 direction = iv.Get<Vector2>();
        if (playerTurn)
        {
            UseAbility(direction);
        }
        else
        {
            UseItem(direction);
        }
    }

    private void UseAbility(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            if (direction.x != 0)
            {
                if (direction.x < 0)
                {
                    playerController.UseAbility("a");
                    // Defense
                }
                else
                {
                    playerController.UseAbility("d");
                    // Melee attack
                }
            }
            if (direction.y != 0)
            {
                if (direction.y < 0)
                {
                    playerController.UseAbility("s");
                    // Vehicle
                }
                else
                {
                    // Ranged attack
                    playerController.UseAbility("w");
                    Debug.Log("Pew!");
                }
            }
            slowdownActive = false;
            ChangeSpeeds();
            TogglePauseSelectionUI(false);
        }
    }

    private void UseItem(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            if (direction.x != 0)
            {
                if (direction.x < 0)
                {
                    // Left
                }
                else
                {
                    // Right
                }
            }
            if (direction.y != 0)
            {
                if (direction.y < 0)
                {
                    // Down
                }
                else
                {
                    // Up
                }
            }
        }
    }

    private void PowerUpSequence()
    {
        if (chargeButton)
        {
            if (chargeEnergy < maxEnergy)
            {
                chargeEnergy += playerChargeRate;
            }
            poweringUp = true;
        }
        if (!chargeButton && poweringUp)
        {
            enemyEnergy = chargeEnergy;
            poweringUp = false;
            chargeActive = true;
            gameRunSpeed = 1;
            ActivateProjectiles();
        }
    }

    private void PlayerRush()
    {
        if (!inCombat)
        {
            playerController.AdvanceMe(gameRunSpeed * (rushSpeedBoost * chargeEnergy / maxEnergy));

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
        foreach (GameObject enemy in enemies)
        {
            CharacterControl ec = enemy.GetComponent<CharacterControl>();
            if (ec.CanAdvance())
            {
                ec.AdvanceMe(gameRunSpeed);
            }
            if (ec.CanFight())
            {
                DoCombat();
            }
        }
        enemyEnergy -= enemyDepleteRate * gameRunSpeed;
    }

    public List<GameObject> GetEnemies()
    {
        return enemies;
    }

    private void PauseForAbility()
    {
        if (!slowdownActive && chargeButton)
        {
            slowdownActive = true;
            ChangeSpeeds();
            TogglePauseSelectionUI(true);
        }
        else if (slowdownActive && chargeButton)
        {
            slowdownActive = false;
            ChangeSpeeds();
            TogglePauseSelectionUI(false);
        }
    }

    private void PauseForItem()
    {
        if (!slowdownActive && chargeButton)
        {
            slowdownActive = true;
            ChangeSpeeds();
            TogglePauseSelectionUI(true);
        }
        else if (slowdownActive && chargeButton)
        {
            slowdownActive = false;
            ChangeSpeeds();
            TogglePauseSelectionUI(false);
        }
    }

    private void ActivateEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            CharacterControl ec = enemy.GetComponent<CharacterControl>();
            ec.SetAnimationSpeed(gameRunSpeed);
            if (ec.IsAlive())
            {
                ec.SetMyTurn(!playerTurn);
            }
        }
    }

    private void ActivateProjectiles()
    {
        foreach (GameObject projectile in GameObject.FindGameObjectsWithTag("Projectile"))
        {
            ProjectileScript ps = projectile.GetComponent<ProjectileScript>();
            ps.SetAnimationSpeed(gameRunSpeed);
        }
    }

    private void ChangeSpeeds()
    {
        if (slowdownActive)
        {
            gameRunSpeed = 0.05f;
        }
        else
        {
            gameRunSpeed = 1;
        }
        if (playerTurn && !firstTurn && !chargeActive)
        {
            gameRunSpeed = 0;
        }
        ActivateEnemies();
        ActivateProjectiles();
        if (playerController == null)
        {
            playerController = playerCharacter.GetComponent<CharacterControl>();
            playerController.SetGameController(gameObject);
        }
        playerController.SetMyTurn(playerTurn);
        playerController.SetAnimationSpeed(gameRunSpeed);
    }

    private void TogglePauseSelectionUI(bool newState)
    {
        pauseSelectionUI.GetComponent<Canvas>().enabled = newState;
    }
}
