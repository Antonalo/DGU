using System;
using UnityEngine;

public static class JsonHelper
{
	[Serializable]
	private class Wrapper<T>
	{
		public T Items;
	}

	public static T FromJson<T>(string json)
	{
		return JsonUtility.FromJson<Wrapper<T>>(json).Items;
	}

	public static string ToJson<T>(T items)
	{
		return JsonUtility.ToJson(new Wrapper<T>
		{
			Items = items
		});
	}

	public static string ToJson<T>(T items, bool prettyPrint)
	{
		return JsonUtility.ToJson(new Wrapper<T>
		{
			Items = items
		}, prettyPrint);
	}
}
