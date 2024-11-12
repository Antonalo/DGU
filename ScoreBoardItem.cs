using System;
using System.Collections.Generic;

[Serializable]
public class ScoreBoardItem
{
	public class ScoreBoardItemComparator : IComparer<ScoreBoardItem>
	{
		public int Compare(ScoreBoardItem item1, ScoreBoardItem item2)
		{
			if (item1.score < item2.score)
			{
				return 1;
			}
			if (item1.score > item2.score)
			{
				return -1;
			}
			return item2.name.CompareTo(item1.name);
		}
	}

	public string id;

	public string name;

	public float score;

	public int perfectX;

	public int perfect;

	public int great;

	public int good;

	public int miss;

	public int currentLevel;

	public int noSkills;

	public int danceLevel = -1;

	public int perfectTimes;

	public int delLevel;

	public void reset()
	{
		score = 0f;
		delLevel = 0;
		good = 0;
		great = 0;
		miss = 0;
		perfect = 0;
		perfectTimes = 0;
		perfectX = 0;
		name = Global.user.name;
		id = Global.localPlayerId;
		currentLevel = 0;
		noSkills = 0;
	}
}
