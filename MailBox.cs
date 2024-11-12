using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MailBox
{
	public string id;

	public string senderId;

	public string receiverId;

	public string senderName;

	public string receiverName;

	public string subject;

	public string content;

	public bool isRead;

	public List<int> items;

	public DateTime createdDate;

	public string toString()
	{
		return JsonUtility.ToJson(this);
	}
}
