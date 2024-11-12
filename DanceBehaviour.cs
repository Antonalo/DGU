using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DanceBehaviour : MonoBehaviourPunCallbacks
{
	private Vector3 originalPos;

	private Vector3 originalRotation;

	private bool isPerformingCombo;

	private bool triggeredCombo;

	private int comboIndex;

	private ParticleSystem DAX_Frost_Shield_00;

	public Sprite highlightedPosition;

	public Sprite normalPosition;

	private ScoreBoardItem myScore;

	public int selectedRoomId = -1;

	public GameInfoType selectedRoomType;

	private GameObject scorePosition;

	public List<Gradient> lightBeamGradients = new List<Gradient>();

	private GameObject versusBar;

	private float normalizedTime;

	private void Start()
	{
		CommonUtils.GetDanceController().OnTriggerCombo += triggerCombo;
		scorePosition = GameObject.Find("Screen/DanceObj/Score_Position");
		isPerformingCombo = false;
		triggeredCombo = false;
	}

	public void endDance()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.CurrentRoom.IsOpen = true;
			string text = sendDanceResultToServer();
			base.photonView.RPC("RpcEndDance", RpcTarget.All, text);
		}
	}

	private string sendDanceResultToServer()
	{
		List<ExpAndGold> list = new List<ExpAndGold>();
		List<ScoreBoardItem> scoreBoard = CommonUtils.GetDanceController().scoreBoard;
		scoreBoard.Sort(new ScoreBoardItem.ScoreBoardItemComparator());
		foreach (ScoreBoardItem item in scoreBoard)
		{
			Debug.Log(">>> scoreboardItems: " + item.id);
			bool winner = scoreBoard[0].id.Equals(item.id);
			ExpAndGold expAndGold = new ExpAndGold();
			expAndGold.userId = item.id;
			expAndGold.exp = calculateExp(item);
			expAndGold.winner = winner;
			if (expAndGold.userId.Equals(Global.localPlayerId) && expAndGold.gainedIntimacyPercent >= 100)
			{
				CommonUtils.GetGameController().displayQuestNotif(isDisplay: true);
			}
			list.Add(expAndGold);
		}
		string text = JsonHelper.ToJson(list);
		StartCoroutine(updateGoldAndExp(text));
		return text;
	}

	[PunRPC]
	private void RpcEndDance(string payloadStr)
	{
		Debug.Log(">>> RpcEndDance called");
		List<ExpAndGold> list = JsonHelper.FromJson<List<ExpAndGold>>(payloadStr);
		playPerfectEffect(Global.localPlayerId, 0);
		foreach (ExpAndGold item in list)
		{
			_ = (bool)GameObject.Find(item.userId);
			if (item.userId.Equals(Global.localPlayerId))
			{
				EventUtils.triggerUserUpdatedEvent(updateClothes: false);
				string translationText = LeanLocalization.GetTranslationText("GainMsg");
				string translationText2 = LeanLocalization.GetTranslationText("Exp");
				CommonUtils.GetChatController().OnGetMessages("ALL", new string[1] { "-1|SYSTEM" }, new object[1] { translationText + " " + item.exp + " " + translationText2 });
			}
		}
		bool flag = list[0].userId.Equals(Global.localPlayerId);
		Debug.Log("[RpcEndDanceCallBack] isWinner: " + flag);
		Animator animator = CommonUtils.GetAnimator();
		animator.SetInteger("skill", -1);
		animator.SetBool("Dance", value: false);
		if (flag)
		{
			animator.SetTrigger("Victory");
		}
		else
		{
			animator.SetTrigger("Lose");
		}
		foreach (Transform item2 in scorePosition.transform)
		{
			Object.Destroy(item2.gameObject);
		}
		if (!PhotonNetwork.IsMasterClient)
		{
			CommonUtils.GetLobbyController().OnReadyClick();
		}
	}

	private int calculateExp(ScoreBoardItem score)
	{
		return Mathf.RoundToInt(score.score / 30000f + 0.4f * (float)score.perfectX);
	}

	public void registerDance(ScoreBoardItem myScore)
	{
		CommonUtils.GetDanceController().scoreBoard.Clear();
		this.myScore = myScore;
		getCharOriginalPosition();
		base.photonView.RPC("RpcRegisterDance", RpcTarget.All, JsonHelper.ToJson(myScore));
	}

	[PunRPC]
	private void RpcRegisterDance(string myScoreJson)
	{
		ScoreBoardItem item = JsonHelper.FromJson<ScoreBoardItem>(myScoreJson);
		CommonUtils.GetDanceController().scoreBoard.Add(item);
		Debug.Log("CommonUtils.GetDanceController().scoreBoard: " + CommonUtils.GetDanceController().scoreBoard);
		CommonUtils.GetAnimator().SetBool("Dance", value: true);
		initScoreBoard(CommonUtils.GetDanceController().myScore);
	}

	public void updateScore(ScoreBoardItem myScore)
	{
		this.myScore = myScore;
		base.photonView.RPC("RpcUpdateScore", RpcTarget.All, JsonHelper.ToJson(myScore));
	}

	[PunRPC]
	private void RpcUpdateScore(string myScoreJson)
	{
		ScoreBoardItem myScore = JsonHelper.FromJson<ScoreBoardItem>(myScoreJson);
		List<ScoreBoardItem> scoreBoard = CommonUtils.GetDanceController().scoreBoard;
		scoreBoard.RemoveAll((ScoreBoardItem score) => score.id.Equals(myScore.id));
		scoreBoard.Add(myScore);
		CommonUtils.GetDanceController().scoreBoard.Sort(new ScoreBoardItem.ScoreBoardItemComparator());
		updateScoreBoard(CommonUtils.GetDanceController().myScore, myScore.id);
	}

	private IEnumerator updateGoldAndExp(string jsonReq)
	{
		UnityWebRequest request = CommonUtils.prepareRequest(Global.USER_URL + "expandgold", "POST", jsonReq);
		yield return request.SendWebRequest();
		if (JsonUtility.FromJson<Response<object>>(request.downloadHandler.text).status.code != 0)
		{
			Debug.LogError("Fail to Update User gold and exp");
			CommonUtils.showPopupMsg("Server Error");
		}
		else
		{
			Debug.Log("Updated User gold and exp successfully");
		}
	}

	private void initScoreBoard(ScoreBoardItem myScore)
	{
		if (scorePosition == null)
		{
			scorePosition = GameObject.Find("Screen/DanceObj/Score_Position");
		}
		List<ScoreBoardItem> scoreBoard = CommonUtils.GetDanceController().scoreBoard;
		for (int i = 0; i < scoreBoard.Count; i++)
		{
			string id = scoreBoard[i].id;
			if (!(null != scorePosition.transform.Find(id)))
			{
				GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/ScorePosition/Item") as GameObject);
				gameObject.transform.SetParent(scorePosition.transform);
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				switch (i)
				{
				case 0:
					gameObject.transform.Find("Position1").gameObject.SetActive(value: false);
					gameObject.transform.Find("Position2").gameObject.SetActive(value: false);
					gameObject.transform.Find("Position3").gameObject.SetActive(value: false);
					break;
				case 1:
					gameObject.transform.Find("Position0").gameObject.SetActive(value: false);
					gameObject.transform.Find("Position2").gameObject.SetActive(value: false);
					gameObject.transform.Find("Position3").gameObject.SetActive(value: false);
					break;
				case 2:
					gameObject.transform.Find("Position0").gameObject.SetActive(value: false);
					gameObject.transform.Find("Position1").gameObject.SetActive(value: false);
					gameObject.transform.Find("Position3").gameObject.SetActive(value: false);
					break;
				default:
					gameObject.transform.Find("Position0").gameObject.SetActive(value: false);
					gameObject.transform.Find("Position1").gameObject.SetActive(value: false);
					gameObject.transform.Find("Position2").gameObject.SetActive(value: false);
					break;
				}
				gameObject.gameObject.name = id;
			}
		}
		updateScoreBoard(myScore, myScore.id);
	}

	private void updateScoreBoard(ScoreBoardItem myScore, string senderId)
	{
		if (scorePosition == null)
		{
			scorePosition = GameObject.Find("Screen/DanceObj/Score_Position");
		}
		List<ScoreBoardItem> scoreBoard = CommonUtils.GetDanceController().scoreBoard;
		for (int i = 0; i < scoreBoard.Count; i++)
		{
			GameObject gameObject = scorePosition.transform.GetChild(i).gameObject;
			if (scoreBoard[i].id.Equals(myScore.id))
			{
				gameObject.GetComponent<Image>().sprite = highlightedPosition;
			}
			else
			{
				gameObject.GetComponent<Image>().sprite = normalPosition;
			}
			ScoreBoardItem scoreBoardItem = scoreBoard[i];
			gameObject.transform.Find("Name").GetComponent<Text>().text = scoreBoardItem.name;
			gameObject.transform.Find("Score").GetComponent<Text>().text = scoreBoardItem.score.ToString() ?? "";
			gameObject.name = scoreBoard[i].id;
			if (scoreBoardItem.id == base.name)
			{
				playPerfectEffect(scoreBoardItem.id, scoreBoardItem.perfectTimes);
			}
			if (scoreBoardItem.danceLevel >= 0 && senderId.Equals(scoreBoardItem.id))
			{
				StartCoroutine(showDanceLevel(scoreBoardItem));
			}
		}
	}

	private void updateVersusBar(float bossScore, float teamScore)
	{
		if (versusBar == null)
		{
			versusBar = GameObject.Find("Screen/DanceObj/Versus_Bar");
		}
		versusBar.transform.Find("Boss_Score").GetComponent<TextMeshProUGUI>().SetText(bossScore.ToString());
		versusBar.transform.Find("Team_Score").GetComponent<TextMeshProUGUI>().SetText(teamScore.ToString());
		float t;
		if (teamScore == 0f)
		{
			t = 1f;
			if (bossScore == 0f)
			{
				t = 0.5f;
			}
		}
		else
		{
			t = bossScore / (bossScore + teamScore);
		}
		versusBar.transform.Find("Slider").GetComponent<Slider>().value = Mathf.Lerp(0f, 1f, t);
	}

	private void playPerfectEffect(string userId, int perfectTimes)
	{
		GameObject.Find(userId);
		if (perfectTimes > 2)
		{
			_ = 4;
		}
	}

	private IEnumerator showDanceLevel(ScoreBoardItem scoreBoardItem)
	{
		GameObject danceScoreImg = GameObject.Find(scoreBoardItem.id + "/CharacterInfo/DanceScore");
		if (danceScoreImg.transform.Find("PerfectTimes") == null)
		{
			yield return null;
		}
		TextMeshProUGUI component = danceScoreImg.transform.Find("PerfectTimes").GetComponent<TextMeshProUGUI>();
		danceScoreImg.GetComponent<Image>().sprite = CommonUtils.GetDanceController().danceScoreSprites[scoreBoardItem.danceLevel];
		if (scoreBoardItem.perfectTimes > 1)
		{
			component.SetText("x" + scoreBoardItem.perfectTimes);
		}
		else
		{
			component.SetText("");
		}
		danceScoreImg.GetComponent<Animation>().Play();
	}

	public void triggerCombo(int comboIndex)
	{
		triggeredCombo = true;
		this.comboIndex = comboIndex;
		CommonUtils.GetAnimator().SetInteger("Combo", comboIndex);
	}

	public void getCharOriginalPosition()
	{
		originalPos = base.transform.localPosition;
		originalRotation = base.transform.eulerAngles;
	}

	public void dance(int skill, bool hasNormalizedTime)
	{
		if (skill != 0)
		{
			if (hasNormalizedTime)
			{
				normalizedTime = CommonUtils.GetAnimator().GetCurrentAnimatorStateInfo(2).normalizedTime;
				CommonUtils.GetAnimator().Play(CommonUtils.GetAnimator().GetInteger("Song").ToString(), 2, normalizedTime);
			}
			else
			{
				CommonUtils.GetAnimator().Play(CommonUtils.GetAnimator().GetInteger("Song").ToString());
			}
		}
		CommonUtils.GetAnimator().SetInteger("skill", skill);
	}

	private void endCombo()
	{
		CommonUtils.GetAnimator().SetTrigger("EndCombo");
	}
}
