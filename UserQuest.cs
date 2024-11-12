using System;
using UnityEngine;

[Serializable]
public class UserQuest
{
	public string questId;

	public string username;

	public int currentAcquireAmt;

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
