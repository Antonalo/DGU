using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MailBoxRequest
{
	public string senderId;

	public string receiverId;

	public string subject;

	public string content;

	public List<int> items;

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
