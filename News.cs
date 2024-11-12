using System;
using UnityEngine;

[Serializable]
public class News
{
	public string id;

	public string type;

	public string authorId;

	public string image;

	public string subject;

	public string description;

	public string content;

	public string postedDate;

	public new string ToString()
	{
		return JsonUtility.ToJson(this);
	}
}
