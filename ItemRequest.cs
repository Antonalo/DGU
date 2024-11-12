using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemRequest
{
	public List<int> ids;

	public bool isMale;

	public string name;

	public string type;

	public List<int> storeTypes;

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
