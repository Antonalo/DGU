using System;
using UnityEngine;

[Serializable]
public class Friend
{
	public string id;

	public string username;

	public string name;

	public string gender;

	public string dob;

	public string email;

	public string picture;

	public int peerId;

	public int vip;

	public int level;

	public string clubName;

	public bool online;

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
