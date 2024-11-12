using System;
using UnityEngine;

[Serializable]
public class ExpAndGold
{
	[HideInInspector]
	public string userId;

	[HideInInspector]
	public int bossId = -1;

	public int gainedIntimacyPercent;

	public int gold;

	public int exp;

	public int ruby;

	public string sceneName;

	public float positionX;

	public float positionY;

	public float positionZ;

	[HideInInspector]
	public bool pkMode;

	[HideInInspector]
	public bool winner;

	public ExpAndGold()
	{
		userId = Global.localPlayerId;
	}

	public ExpAndGold(string userId, int gold, int ruby, int exp)
	{
		this.userId = userId;
		this.gold = gold;
		this.ruby = ruby;
		this.exp = exp;
	}

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
