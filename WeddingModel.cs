using System;
using UnityEngine;

[Serializable]
public class WeddingModel
{
	public string coupleId;

	public int famousScore;

	public WeddingModel()
	{
	}

	public WeddingModel(string coupleId, int famousScore)
	{
		this.coupleId = coupleId;
		this.famousScore = famousScore;
	}

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
