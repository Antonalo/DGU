using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Quest
{
	public enum QuestMode
	{
		AU = 0,
		TIET_TAU = 1,
		TOUCH = 2,
		EXP = 3,
		GOLD = 4,
		PERFECT = 5,
		LEVEL = 6,
		ONLINE_TIME = 7,
		NPC = 8
	}

	public enum QuestType
	{
		MAIN = 0,
		SIDE = 1,
		DAILY = 2,
		BOSS = 3,
		CLUB = 4,
		COUPLE = 5,
		WEEKLY = 6
	}

	public string id;

	public string title;

	public string description;

	public int acquireAmt;

	public string mode;

	public string type;

	public int point;

	public int currentAcquireAmt;

	public bool completed;

	public bool rewarded;

	public List<RewardItem> rewardItems;

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
