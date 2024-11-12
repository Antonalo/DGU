using System;
using UnityEngine;

[Serializable]
public class AchievementModel
{
	public string id;

	public string description;

	public int acquireAmount;

	public string badgeSprite;

	public string username;

	public int currentAcquireAmt;

	public bool completed;

	public string type;

	public bool wearing;

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
