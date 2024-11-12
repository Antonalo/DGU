using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class User
{
	public string id;

	public string username;

	public string name;

	public string gender;

	public string dob;

	public string email;

	public string picture;

	public string password;

	public string aboutMe;

	public bool isFacebookLogin;

	public string role = "USER";

	public bool isAdmin;

	public int peerId;

	public string token;

	public string title;

	public int gold;

	public int ruby;

	public int vip;

	public int currentVIP;

	public int nextVIP;

	public int level;

	public string levelName;

	public int currentExp;

	public int nextExp;

	public string clubId;

	public string clubName;

	public string clubTitle;

	public int contribution;

	public string coupleId;

	public string coupleName;

	public int coupleLevel;

	public string coupleTitle;

	public string coupleUserId;

	public int coupleNextExp;

	public int coupleCurrentExp;

	public int luckyDrawCount;

	public string coupleSignature;

	public int famousScore;

	public List<string> albumns = new List<string>(5);

	public string lastTimeLoggedin;

	public bool isMale()
	{
		return gender.Equals("male");
	}

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
