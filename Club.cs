using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Club
{
	public string id;

	public string name;

	public string president;

	public string vicePresident;

	public string slogan;

	public string picture;

	public int level;

	public int currentContribution;

	public int nextContribution;

	public int currentMember;

	public int maxNumber;

	public string createdDate;

	public List<string> users;

	public List<int> presidentClothings;

	public string presidentGender;

	public bool isPresidentMale()
	{
		return presidentGender.Equals("male");
	}

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
