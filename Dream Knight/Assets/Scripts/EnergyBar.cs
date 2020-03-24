using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    // Displaying the multifunction energy bar. The bar has both the player and enemy charge. The player charges and when the player has let go
    // The charge will dissipate, revealing an enemy bar of the same charge. When the player charge is depleted, then the enemy bar will start depleting
    
    // Get player energy and enemy energy from the GameController
    // Assume they are correct 
    // Energy bar bounds - hardcoding for 1920x1080 to begin with
    // Bar Empty, Bar Full
    private float[] energyBarBounds = { 1495, 0};
    private float maxPlayerEnergy = 20.0f;
    private float currentPlayerEnergy;
    private float currentEnemyEnergy;
       
    public GameObject playerBar;
    public GameObject enemyBarCharge;
    float tester = 1.0f;
    private GameController gameController;

    int barMode = 0; // 0 player charging, 1 player depleting, 2 enemy depleting 

    // Start is called before the first frame update
    void Start()
    {
        currentPlayerEnergy = 0;
        playerBar.GetComponent<RectTransform>().SetLeft(energyBarBounds[1]);
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    float MapEnergy(float currentEnergy, float maxPlayerEnergy)
    {
        // make sure current energy can never be greater than maxPlayerEnergy
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxPlayerEnergy);
        // should map from empty bar to full one
        float normalizedEnergy = currentEnergy / maxPlayerEnergy; // 0 - 1;
        return normalizedEnergy.Remap(0, 1, energyBarBounds[0], energyBarBounds[1]);
    }

    public void runDemo()
    {
        Debug.Log("Player energy " + currentPlayerEnergy);
        // First update the player bar
        if ((currentPlayerEnergy < 10) && barMode == 0)
        {
            currentPlayerEnergy += Time.deltaTime * 6;
            Debug.Log("Charging");
            currentEnemyEnergy = currentPlayerEnergy;

        }
        else
        {
            Debug.Log("Switching to depletion");
            barMode = 1;    
        }

       if (barMode == 1 && currentPlayerEnergy > 0)
       {
           currentPlayerEnergy -= Time.deltaTime * 4;
       }
       else if (barMode == 1)
       {
            barMode = 2;     
       }

       if (barMode == 2 && currentEnemyEnergy > 0)
        {
            currentEnemyEnergy -= Time.deltaTime * 4;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        currentPlayerEnergy = gameController.chargeEnergy;
        currentEnemyEnergy = gameController.enemyEnergy;
        maxPlayerEnergy = gameController.maxEnergy;
        //runDemo();
        playerBar.GetComponent<RectTransform>().SetRight(MapEnergy(currentPlayerEnergy, maxPlayerEnergy));
        enemyBarCharge.GetComponent<RectTransform>().SetRight(MapEnergy(currentEnemyEnergy, maxPlayerEnergy));
    }


}
