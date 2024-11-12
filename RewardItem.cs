using System;
using UnityEngine;

[Serializable]
public class RewardItem
{
	public int itemId;

	public int expiryDays;

	public int quantity;

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
