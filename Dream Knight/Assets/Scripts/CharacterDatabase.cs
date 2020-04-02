using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using LitJson;
using System.Collections.Generic;
using System.IO;

public class CharacterDatabase : MonoBehaviour {
	private List<Character> database = new List<Character>();
	private JsonData characterData;

	void Start()
	{
        characterData = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/StreamingAssets/Characters.json"));
		ConstructCharacterDatabase();	

       //DEMO 
       Debug.Log("My characters:");
       foreach(Character i in database)
       {
           Debug.Log(i.title);
       } 
	}

	public Character FetchCharacterById(int id)
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
	
	void ConstructCharacterDatabase()
	{
		for (int i = 0; i < characterData.Count; i++)
		{
			Character newCharacter = new Character();
            // Set Character attributes
			newCharacter.id = (int)characterData[i]["id"];
            newCharacter.title = characterData[i]["title"].ToString();
			// newItem.Title = itemData[i]["title"].ToString();
			// newItem.Value = (int)itemData[i]["value"];
			// newItem.Power = (int)itemData[i]["stats"]["power"];
			// newItem.Defense = (int)itemData[i]["stats"]["defense"];
			// newItem.Vitality = (int)itemData[i]["stats"]["vitality"];
			// newItem.Description = itemData[i]["description"].ToString();
			// newItem.Stackable = (bool)itemData[i]["stackable"];
			// newItem.Rarity = (int)itemData[i]["rarity"];
			// newItem.Slug = itemData[i]["slug"].ToString();
			//newItem.Sprite = Resources.Load<Sprite>("Sprites/Items/" + newItem.Slug);

			database.Add(newCharacter);
		}
	}
}

public class Enemy : Character
{
	public int id { get; set; }
	public string title { get; set; }


	// public int Value { get; set; }
	// public int Power { get; set; }
	// public int Defense { get; set; }
	// public int Vitality { get; set; }
	// public string Description { get; set; }
	// public bool Stackable { get; set; }
	// public int Rarity { get; set; }
	// public string Slug { get; set; }
	// public Sprite Sprite { get; set; }

	public Enemy()
	{
		this.id = -1;
	}
}

public class Player : Character
{
	public int id { get; set; }
	public string title { get; set; }

	public Player()
	{
		this.id = -1;
		this.baseDamage = 100;
		this.attackCooldown = 0.6f;
		this.directionModifier = 1;
	}
}