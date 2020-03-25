using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using LitJson;
using System.Collections.Generic;
using System.IO;

public class EnemyDatabase : MonoBehaviour {
	private List<Enemy> database = new List<Enemy>();
	private JsonData enemyData;

	void Start()
	{
        enemyData = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/StreamingAssets/Enemies.json"));
		ConstructEnemyDatabase();	

       //DEMO 
       Debug.Log("My enemies:");
       foreach(Enemy i in database)
       {
           Debug.Log(i.Title);
       } 
	}

	public Enemy FetchEnemyById(int id)
	{
		for (int i = 0; i < database.Count; i++)
		{
			if (database[i].Id == id)
			{
				return database[i];
			}
		}

		return null;
	}
	
	void ConstructEnemyDatabase()
	{
		for (int i = 0; i < enemyData.Count; i++)
		{
			Enemy newEnemy = new Enemy();
            // Set Enemy attributes
			newEnemy.Id = (int)enemyData[i]["id"];
            newEnemy.Title = enemyData[i]["title"].ToString();
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

			database.Add(newEnemy);
		}
	}
}

public class Enemy
{
	public int Id { get; set; }
	public string Title { get; set; }
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
		this.Id = -1;
	}
}