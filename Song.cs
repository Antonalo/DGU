using System;
using UnityEngine;

[Serializable]
public class Song
{
	public int id;

	public string name;

	public string artist;

	public string bpm;

	public string level;

	public string duration;

	public string fileName;

	public Song(int id, string name, string artist, string bpm, string level, string duration, string fileName)
	{
		this.id = id;
		this.name = name;
		this.artist = artist;
		this.bpm = bpm;
		this.level = level;
		this.duration = duration;
		this.fileName = fileName;
	}

	public string ToJson()
	{
		return JsonUtility.ToJson(this);
	}
}
