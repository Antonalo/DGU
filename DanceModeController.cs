using System.Collections.Generic;
using UnityEngine;

public abstract class DanceModeController : MonoBehaviour
{
	[HideInInspector]
	public ScoreBoardItem myScore = new ScoreBoardItem();

	protected List<Sprite> DelSprites = new List<Sprite>();

	protected bool isStartedDance;

	protected int delLevel;

	protected bool isDelEnabled;
}
