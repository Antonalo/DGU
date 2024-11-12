using System;
using System.Collections.Generic;

[Serializable]
public class GameInfoPacket
{
	public int Id;

	public GameInfoType Type;

	public string DanceMode = "AU - EASY";

	public int SongId = 1;

	public bool isDel;

	public string SceneName = "";

	public string Title = "";

	public List<string> readyPlayerIds = new List<string>();

	public List<string> playerIds = new List<string>();

	public bool IsPasswordProtected;

	public int MaxPlayers;

	public int FemaleNum;

	public int MaleNum;

	public string RoomMasterId;

	public int speed;

	public int difficulty;

	public int maxHP;

	public int currentHP;

	public int ruby;

	public int gold;

	public string time;

	public float countDownInSec = -1f;

	public int luckyDrawCount;

	public string ToJson()
	{
		return JsonHelper.ToJson(this);
	}
}
