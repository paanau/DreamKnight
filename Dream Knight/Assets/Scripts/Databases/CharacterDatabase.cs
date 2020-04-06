using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

public class CharacterDatabase : MonoBehaviour {
	private List<Character> database = new List<Character>();
	private JsonData characterData;

	void Awake()
	{
        characterData = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/StreamingAssets/Characters.json"));
		ConstructCharacterDatabase();	

       //DEMO 
       //Debug.Log("My characters:");
       foreach(Character i in database)
       {
           //Debug.Log(i.title);
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
			try	{
				newCharacter.isAlive = (bool)characterData[i]["isAlive"];
			} catch {};
			try {
			newCharacter.isPrivate = (bool)characterData[i]["isPrivate"];
			} catch {};
			try {
			newCharacter.inCombat = (bool)characterData[i]["isInCombat"];
			} catch {};
			try {
			newCharacter.myTurn = (bool)characterData[i]["isMyTurn"];
			} catch {};
			try {
			newCharacter.baseMaxHP = (float)characterData[i]["baseMaxHP"];
			} catch {};
			try {
			newCharacter.currentHP = (float)characterData[i]["currentHP"];
			} catch {};
			try {
			newCharacter.shieldHealth = (float)characterData[i]["shieldHealth"];
			} catch {};
			try {
			newCharacter.baseDamage = (float)characterData[i]["baseDamage"];
			} catch {};
			try {
			newCharacter.baseSpeed = (float)characterData[i]["baseSpeed"];
			} catch {};
			try {
			newCharacter.range = (float)characterData[i]["range"];
			} catch {};
            try
            {
                newCharacter.hitchance = (float)characterData[i]["hitchance"];
            }
            catch { };
            try
            {
                newCharacter.evasion = (float)characterData[i]["evasion"];
            }
            catch { };
            try {
			newCharacter.attackCooldown = (float)characterData[i]["attackCooldown"];
			} catch {};
			try {
			newCharacter.directionModifier = (int)characterData[i]["directionModifier"];
			} catch {};
			//newCharacter.myAnimator = (Animator)characterData[i]["myAnimator"];
			//newCharacter.myAnimator.speed = (float)characterData[i]["myTurn"];
			try {
			newCharacter.gameRunSpeed = (float)characterData[i]["gameRunSpeed"];
			} catch {};
			try {
				newCharacter.speedModifiers = (float)characterData[i]["speedModifiers"];
			} catch {};
			try {
			newCharacter.damageModifiers = (float)characterData[i]["damageModifiers"];
			} catch {};
			database.Add(newCharacter);
		}
	}
}

// public class Enemy : Character
// {
// 	public int id { get; set; }
// 	public string title { get; set; }


// 	// public int Value { get; set; }


// 	public Enemy()
// 	{
// 		this.id = -1;
// 	}
// }

// public class Player : Character
// {
// 	public int id { get; set; }
// 	public string title { get; set; }

// 	public Player()
// 	{
// 		this.id = -1;
// 		this.baseDamage = 100;
// 		this.attackCooldown = 0.6f;
// 		this.directionModifier = 1;
// 	}
// }