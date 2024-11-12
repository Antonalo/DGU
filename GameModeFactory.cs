using System.Collections.Generic;
using UnityEngine;

public class GameModeFactory : MonoBehaviour
{
	private static List<CustomPair> modes;

	public static List<CustomPair> getGameModes()
	{
		if (modes == null)
		{
			modes = new List<CustomPair>();
			modes.Add(new CustomPair("AU", "AU"));
			modes.Add(new CustomPair("TIET_TAU", "TIET_TAU"));
		}
		return modes;
	}
}
