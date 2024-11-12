using System;

[Serializable]
public class CustomPair
{
	public string Key;

	public string Value;

	public CustomPair(string key, string value)
	{
		Key = key;
		Value = value;
	}
}
