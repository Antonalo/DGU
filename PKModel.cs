using System;
using UnityEngine;

[Serializable]
public class PKModel
{
	public int rank;

	public string name;

	public int score;

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
