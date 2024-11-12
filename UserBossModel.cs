using System;
using UnityEngine;

[Serializable]
public class UserBossModel
{
	public string userId;

	public int bossId;

	public int intimacyPercent;

	public int luckyDrawCount;

	public new string ToString()
	{
		return JsonUtility.ToJson(this);
	}
}
