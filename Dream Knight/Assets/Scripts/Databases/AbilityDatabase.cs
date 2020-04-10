using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

public class AbilityDatabase : MonoBehaviour
{
    private List<Ability> database = new List<Ability>();
    private JsonData abilityData;
    private string path;

    // Start is called before the first frame update
    void Awake()
    {
        path = "jar:file://" + Application.dataPath + "!/assets/";
        //#if UNITY_EDITOR
        //        characterData = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/StreamingAssets/Characters.json"));
        //#elif UNITY_ANDROID
        //        characterData = JsonMapper.ToObject(File.ReadAllText(path + "/Characters.json"));
        //#endif
        abilityData = JsonMapper.ToObject(GameObject.Find("GameController").GetComponent<GameController>().abilitiesJSON.text);
        ConstructAbilityDatabase();

        //DEMO 
        //Debug.Log("My abilities:");
        foreach (Ability i in database)
        {
            //Debug.Log(i.title);
        }
    }

    public Ability FetchAbilityById(int id)
    {
        for (int i = 0; i < database.Count; i++)
        {
            if (database[i].id == id)
            {
                return database[i];
            }
        }

        return null;
    }

    public Ability FetchAbilityByType(string s)
    {
        for (int i = 0; i < database.Count; i++)
        {
            if (database[i].type == s)
            {
                return database[i];
            }
        }
        return database[0];
    }

    void ConstructAbilityDatabase()
    {
        for (int i = 0; i < abilityData.Count; i++)
        {
            Ability newAbility = new Ability();
            // Set Character attributes
            newAbility.id = (int)abilityData[i]["id"];
            newAbility.title = abilityData[i]["title"].ToString();
            try
            {
                newAbility.type = (string)abilityData[i]["type"];
            }
            catch { };
            try
            {
                newAbility.target = (string)abilityData[i]["target"];
            }
            catch { };
            try
            {
                newAbility.element = (string)abilityData[i]["element"];
            }
            catch { };
            try
            {
                newAbility.buffTarget = (string)abilityData[i]["buffTarget"];
            }
            catch { };
            try
            {
                newAbility.buffType = (string)abilityData[i]["buffType"];
            }
            catch { };
            try
            {
                newAbility.durationType = (string)abilityData[i]["durationType"];
            }
            catch { };
            try
            {
                newAbility.buff = (bool)abilityData[i]["buff"];
            }
            catch { };
            try
            {
                newAbility.energyCost = (float)abilityData[i]["energyCost"];
            }
            catch { };
            try
            {
                newAbility.strength = (float)abilityData[i]["strength"];
            }
            catch { };
            try
            {
                newAbility.procChance = (float)abilityData[i]["procChance"];
            }
            catch { };
            try
            {
                newAbility.procDuration = (float)abilityData[i]["procDuration"];
            }
            catch { };
            try
            {
                newAbility.procStrength = (float)abilityData[i]["procStrength"];
            }
            catch { };
            try
            {
                newAbility.procInterval = (float)abilityData[i]["procInterval"];
            }
            catch { };
            try
            {
                newAbility.interval = (float)abilityData[i]["interval"];
            }
            catch { };
            try
            {
                newAbility.baseInterval = (float)abilityData[i]["interval"];
            }
            catch { };
            try
            {
                newAbility.speed = (float)abilityData[i]["speed"];
            }
            catch { };
            try
            {
                newAbility.range = (float)abilityData[i]["range"];
            }
            catch { };
            try
            {
                newAbility.multi = (float)abilityData[i]["multi"];
            }
            catch { };
            try
            {
                newAbility.duration = (float)abilityData[i]["duration"];
            }
            catch { };
            try
            {
                newAbility.procEffect= (int)abilityData[i]["procEffect"];
            }
            catch { };
            database.Add(newAbility);
        }
    }
}
