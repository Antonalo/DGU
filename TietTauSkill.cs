using System;
using UnityEngine;

[Serializable]
public class TietTauSkill
{
	public float time;

	public bool isLongTap;

	public float tapTime;

	public KeyCode keyCode = Global.randomKeyCode();

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
