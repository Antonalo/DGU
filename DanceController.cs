using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DanceController : MonoBehaviourPunCallbacks
{
	public delegate void OnTriggerComboDelegate(int combo);

	[HideInInspector]
	public ScoreBoardItem myScore = new ScoreBoardItem();

	[Header("[Sprites]")]
	public List<Sprite> DelSprites = new List<Sprite>();

	public List<Sprite> danceScoreSprites = new List<Sprite>();

	[Header("[Audio]")]
	public AudioClip resultPanelSound;

	public Transform screen;

	private Transform danceObj;

	private Transform auObj;

	private Transform scorePosition;

	private Transform resultPanel;

	private Text countDownText;

	private float countDownTime = 12f;

	private bool startCountDown;

	private GameController gameController;

	public List<ScoreBoardItem> scoreBoard = new List<ScoreBoardItem>();

	[HideInInspector]
	public List<string> danceMoves = new List<string>();

	private AuController auController;

	public event OnTriggerComboDelegate OnTriggerCombo;

	public void generateDanceMoves(int currentModeIndex)
	{
		if (currentModeIndex < 3)
		{
			base.photonView.RPC("RpcGenerateDanceMoves", RpcTarget.MasterClient, currentModeIndex);
		}
	}

	[PunRPC]
	private void RpcGenerateDanceMoves(int currentModeIndex)
	{
		danceMoves.Clear();
		switch (currentModeIndex)
		{
		case 0:
		{
			for (int j = 0; j < 6; j++)
			{
				danceMoves.Add(Global.NEW_ROUND);
				generateLevel(1, 2);
				generateLevel(2, 4);
				generateLevel(3, 6);
				generateLevel(4, 6);
				generateLevel(5, 8);
				generateLevel(6, 8);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
			}
			break;
		}
		case 1:
		{
			for (int k = 0; k < 6; k++)
			{
				danceMoves.Add(Global.NEW_ROUND);
				generateLevel(3, 4);
				generateLevel(4, 4);
				generateLevel(5, 6);
				generateLevel(6, 6);
				generateLevel(7, 8);
				generateLevel(8, 8);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
			}
			break;
		}
		case 2:
		{
			for (int i = 0; i < 6; i++)
			{
				danceMoves.Add(Global.NEW_ROUND);
				generateLevel(6, 4);
				generateLevel(7, 6);
				generateLevel(8, 8);
				generateLevel(9, 10);
				generateLevel(10, 10);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
				danceMoves.Add(Global.BREAK_TIME);
			}
			break;
		}
		}
		base.photonView.RPC("RpcGenerateDanceMovesCallBack", RpcTarget.Others, JsonHelper.ToJson(danceMoves));
	}

	[PunRPC]
	private void RpcGenerateDanceMovesCallBack(string danceMovesJson)
	{
		danceMoves = JsonHelper.FromJson<List<string>>(danceMovesJson);
	}

	private void generateLevel(int level, int time)
	{
		for (int i = 0; i < time; i++)
		{
			if (i % 2 == 1)
			{
				danceMoves.Add("");
				continue;
			}
			List<KeyCode> list = new List<KeyCode>();
			for (int j = 0; j < level; j++)
			{
				list.Add(Global.randomKeyCode());
			}
			danceMoves.Add(JsonHelper.ToJson(list));
		}
	}

	private void Start()
	{
		gameController = CommonUtils.GetGameController();
		resultPanel = GameObject.Find("OnScreenUI/Result").transform;
		countDownText = resultPanel.Find("Close/Text").GetComponent<Text>();
		danceObj = screen.Find("DanceObj").transform;
		auObj = screen.Find("DanceObj/AU").transform;
		auController = auObj.GetComponent<AuController>();
		scorePosition = danceObj.Find("Score_Position").transform;
		startCountDown = false;
	}

	public void startDance()
	{
		auObj.gameObject.SetActive(value: true);
		auController.startDance(isDelEnabled: false);
		danceObj.GetComponent<Animation>().Play("DanceObj_MoveIn");
	}

	public void endDance()
	{
		generateResultPanel();
		danceObj.GetComponent<Animation>().Play("DanceObj_MoveOut");
		GetComponent<AudioSource>().Stop();
		GetComponent<AudioSource>().clip = resultPanelSound;
		GetComponent<AudioSource>().Play();
		CommonUtils.GetMainController().setResultCameraForOnScreenUI(isOn: true);
		CommonUtils.GetCharacterInfoController().gameObject.SetActive(value: false);
		CommonUtils.GetLocalPlayer().transform.Find("EFFECT").gameObject.SetActive(value: false);
		resultPanel.GetComponent<Animation>().Play("Result_Show");
		startCountDown = true;
		auObj.gameObject.SetActive(value: true);
		auController.endDance();
	}

	public void closeResultPanel()
	{
		resultPanel.GetComponent<Animation>().Play("Result_Hide");
		CommonUtils.GetAnimator().SetTrigger("Idle");
		gameController.ScreenIcons.GetComponent<Animation>().Play("Icon_MoveIn");
		gameController.Profile.GetComponent<Animation>().Play("Profile_MoveIn");
		CommonUtils.GetMediaPlayerController().PlaySong();
		CommonUtils.GetMediaPlayerController().GetComponent<Animation>().Play("MediaPlayer_MoveIn");
		CommonUtils.GetLobbyController().canvas.SetActive(value: true);
		CommonUtils.GetMainController().resultCamera.gameObject.SetActive(value: false);
		CommonUtils.GetCharacterInfoController().gameObject.SetActive(value: true);
		CommonUtils.GetMainController().setResultCameraForOnScreenUI(isOn: false);
		CommonUtils.GetLocalPlayer().transform.Find("EFFECT").gameObject.SetActive(value: true);
		CommonUtils.GetMainController().enterNormalMode();
		startCountDown = false;
		countDownTime = 15f;
	}

	private void generateResultPanel()
	{
		Transform transform = resultPanel.Find("Rows");
		foreach (Transform item in transform)
		{
			Object.Destroy(item.gameObject);
		}
		int num = 1;
		string id = CommonUtils.GetMainController().user.id;
		_ = CommonUtils.GetDanceBehaviour().selectedRoomId;
		foreach (ScoreBoardItem item2 in scoreBoard)
		{
			if (!item2.id.StartsWith("Boss"))
			{
				GameObject gameObject = null;
				gameObject = ((num >= 4) ? Object.Instantiate(Resources.Load("Prefabs/ResultPanel/Rank0") as GameObject) : Object.Instantiate(Resources.Load("Prefabs/ResultPanel/Rank" + num) as GameObject));
				gameObject.transform.SetParent(transform);
				gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
				gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, 0f);
				gameObject.transform.Find("Position").GetComponent<TextMeshProUGUI>().text = num.ToString() ?? "";
				gameObject.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item2.name;
				gameObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = item2.score.ToString() ?? "";
				gameObject.transform.Find("PerfectX").GetComponent<TextMeshProUGUI>().text = item2.perfectX.ToString() ?? "";
				gameObject.transform.Find("Perfect").GetComponent<TextMeshProUGUI>().text = item2.perfect.ToString() ?? "";
				gameObject.transform.Find("Miss").GetComponent<TextMeshProUGUI>().text = item2.miss.ToString() ?? "";
				if (item2.id.Equals(id))
				{
					resultPanel.Find("Frame/Score_Details/Score").GetComponent<TextMeshProUGUI>().SetText(item2.score.ToString() ?? "");
					resultPanel.Find("Frame/Score_Details/Perfect").GetComponent<TextMeshProUGUI>().SetText(item2.perfect.ToString() ?? "");
					resultPanel.Find("Frame/Score_Details/Great").GetComponent<TextMeshProUGUI>().SetText(item2.great.ToString() ?? "");
					resultPanel.Find("Frame/Score_Details/Miss").GetComponent<TextMeshProUGUI>().SetText(item2.miss.ToString() ?? "");
					resultPanel.Find("Frame/Score_Details/Combo").GetComponent<TextMeshProUGUI>().SetText(item2.perfectX.ToString() ?? "");
					resultPanel.Find("Frame/Score_Details/Cool").GetComponent<TextMeshProUGUI>().SetText(item2.good.ToString() ?? "");
					string text = CommonUtils.calculateGold(item2).ToString() ?? "";
					string text2 = CommonUtils.calculateExp(item2).ToString() ?? "";
					resultPanel.Find("Frame/Gold_Val").GetComponent<TextMeshProUGUI>().SetText(text ?? "");
					resultPanel.Find("Frame/Exp_Val").GetComponent<TextMeshProUGUI>().SetText(text2 ?? "");
				}
				num++;
			}
		}
	}

	private void Update()
	{
		if (startCountDown)
		{
			countDownText.text = "CLOSE  (" + Mathf.RoundToInt(countDownTime) + ")";
			countDownTime -= Time.deltaTime;
			if (countDownTime <= 0f)
			{
				closeResultPanel();
			}
		}
	}
}
