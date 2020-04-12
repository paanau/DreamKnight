﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using UnityEngine;
using LitJson;

public class GameController : MonoBehaviour
{
    [SerializeField] private bool gameActive, playerTurn, chargeActive, poweringUp, inCombat, firstTurn;
    private bool chargeButton, slowdownActive;
    public float chargeEnergy, maxEnergy, enemyEnergy, playerChargeRate, playerDepleteRate, enemyDepleteRate;
    [SerializeField] private float rushSpeedBoost, rushDamageBoost, playerAttackCooldown, targetAttackCooldown, gameRunSpeed;
    [SerializeField] private GameObject playerCharacter, mainCamera, pauseSelectionUI, enemyPrefab, experienceOrbs;
    private CharacterControl playerController, currentMeleeTargetController;
    private GameObject currentMeleeTarget;
    private PlayerInput playerInput;
    private List<GameObject> enemies = new List<GameObject>();
    private int test, swipeSensitivity = 250, playerExperience, newExperience;
    private Vector2 touchDelta;
    public TextAsset abilitiesJSON, charactersJSON, itemsJSON;

    void Awake(){

        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();

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
            if (playerCharacter.transform.position.x >= mainCamera.transform.position.x - 3)
            {
                mainCamera.transform.position = new Vector3(playerCharacter.transform.position.x + 3, 2.5f, -10);
            }
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
        for (int i = 1; i < 20; i++)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, new Vector3(i*20, 0, 0), Quaternion.identity);
            newEnemy.name = "Enemy " + i;
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


    public void EndCombat(CharacterControl cc)
    {
        inCombat = false;
        playerController.EndCombat();
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
        GetComponent<LevelController>().UpdateEnemies(enemies);
        gameRunSpeed = 1;
        ChangeSpeeds();
        firstTurn = false;
    }

    public void GameOver()
    {
        gameRunSpeed = 0.05f;
        gameActive = false;
        StartCoroutine(PlayerDeathZoomIn());
    }

    IEnumerator PlayerDeathZoomIn()
    {
        Vector3 target = playerCharacter.transform.position;
        target = new Vector3(target.x, target.y, mainCamera.transform.position.z);
        while (Vector3.Distance(mainCamera.transform.position, target) > 0.1f)
        {
            mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, new Vector3(target.x, target.y, mainCamera.transform.position.z), 0.04f);
            mainCamera.GetComponent<Camera>().orthographicSize *= 0.99f;
            yield return null;
        }
    }

    public void OnCharge()
    {
        //Debug.Log(Touch.activeFingers.Count);
        Touch.onFingerDown += ctx =>
        {
            chargeButton = true;
            touchDelta = Vector2.zero;
        };
        Touch.onFingerUp += ctx =>
        {
            chargeButton = false;
            foreach (Touch t in Touch.activeTouches) if (t.isInProgress) chargeButton = true;
        };
        //if (Touch.activeFingers.Count != 0)
        //{
        //    chargeButton = true;
        //}
        //else
        //{
        //    chargeButton = false;
        //}
        
        if (gameActive)
        {
            if (!chargeButton && chargeActive)
            {
                if (touchDelta.magnitude >= swipeSensitivity)
                {
                    CalculateDelta();
                }
                else
                {
                    //PauseForAbility();
                }
                
            }
            
        }
        else
        {
            OnRestart();
        }
    }

    public void OnRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMove(InputControl<Vector2> iv)
    {
        touchDelta += iv.ReadValue();
    }

    public void OnMove(InputValue iv)
    {
        touchDelta += iv.Get<Vector2>();
    }

    public void OnMove()
    {
        touchDelta += Pointer.current.delta.ReadValue();
    }

    private void CalculateDelta()
    {
        if (playerTurn)
        {
            UseAbility(touchDelta);
        }
        else
        {
            UseItem(touchDelta);
        }
    }

    private void UseAbility(Vector2 direction)
    {
        float energyCost = 0;
        string dir = "";
        if (direction != Vector2.zero)
        {
            if (direction.x != 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                dir = direction.x < 0 ? "Defense" : "Offense";
            }
            if (direction.y != 0 && Mathf.Abs(direction.x) < Mathf.Abs(direction.y))
            {
                dir = direction.y < 0 ? "Misc" : "Ranged";
            }
            if (playerController.SelectAbility(dir).energyCost < chargeEnergy)
            {
                energyCost = playerController.UseAbility(dir);
                slowdownActive = false;
                ChangeSpeeds();
                TogglePauseSelectionUI(false);
                chargeEnergy -= energyCost;
            }
        }
    }

    private void UseAbility(string direction)
    {
        float energyCost = 0;
        string dir = "";
        if (direction.Equals("Left") || direction.Equals("Right"))
        {
            dir = direction.Equals("Left") ? "Defense" : "Offense";
        }
        if (direction.Equals("Up") || direction.Equals("Down"))
        {
            dir = direction.Equals("Down") ? "Misc" : "Ranged";
        }
        if (playerController.SelectAbility(dir).energyCost < chargeEnergy)
        {
            energyCost = playerController.UseAbility(dir);
            slowdownActive = false;
            ChangeSpeeds();
            TogglePauseSelectionUI(false);
            chargeEnergy -= energyCost;
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
            playerController.AdvanceModifiers(gameRunSpeed * (rushSpeedBoost * chargeEnergy / maxEnergy));
        }
    }

    private void EnemiesAct()
    {
        foreach (GameObject enemy in enemies)
        {
            CharacterControl ec = enemy.GetComponent<CharacterControl>();
            if (ec.CanAdvance())
            {
                ec.AdvanceModifiers(gameRunSpeed);
            }
            if (ec.CanFight())
            {
                //DoCombat();
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
        if (!slowdownActive)
        {
            slowdownActive = true;
            ChangeSpeeds();
            TogglePauseSelectionUI(true);
        }
        else
        {
            Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(Touchscreen.current.primaryTouch.position.ReadValue());
            Debug.Log(ray);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100); //, LayerMask.GetMask("PauseUI"));
            if (hit.collider != null)
            {
                string s = hit.collider.gameObject.name; //.Substring(15);
                Debug.Log(s);
                UseAbility(s);
            }

            slowdownActive = false;
            ChangeSpeeds();
            TogglePauseSelectionUI(false);
        }
    }

    //private void PauseForItem()
    //{
    //    if (!slowdownActive && chargeButton)
    //    {
    //        slowdownActive = true;
    //        ChangeSpeeds();
    //        TogglePauseSelectionUI(true);
    //    }
    //    else if (slowdownActive && chargeButton)
    //    {
    //        slowdownActive = false;
    //        ChangeSpeeds();
    //        TogglePauseSelectionUI(false);
    //    }
    //}

    public void UpdateExperience(int xp)
    {
        if (playerExperience == newExperience) StartCoroutine(ExperienceTick(xp));
        else newExperience += xp;
    }

    IEnumerator ExperienceTick(int xp)
    {
        newExperience = playerExperience + xp;
        TextMesh XPCount = GameObject.Find("XPCount").GetComponent<TextMesh>();
        while (xp > 0)
        {
            xp = newExperience - playerExperience;
            // Debug.Log(playerExperience + " XP plus " + xp + "/" + xp / 5 + " toward " + newExperience);
            if (xp > 10)
            {
                playerExperience += xp / 5;
                xp -= xp / 5;
            }
            else
            {
                playerExperience += 1;
                xp -= 1;
            }
            XPCount.text = "XP: " + playerExperience;
            yield return null;
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
