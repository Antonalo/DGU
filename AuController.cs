using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuController : DanceModeController
{
	public delegate void OnPlayPerfectEffectDelegate(int perfectTime);

	[Header("[Buttons]")]
	public GameObject upPref;

	public GameObject downPref;

	public GameObject leftPref;

	public GameObject rightPref;

	public GameObject upDelPref;

	public GameObject downDelPref;

	public GameObject leftDelPref;

	public GameObject rightDelPref;

	public GameObject upCorrectPref;

	public GameObject leftCorrectPref;

	public GameObject downCorrectPref;

	public GameObject rightCorrectPref;

	private List<Sprite> danceScoreSprites = new List<Sprite>();

	public Sprite goSprite;

	[Header("[Audio]")]
	public AudioClip buttonPressedSound;

	public AudioClip resultPanelSound;

	public List<AudioClip> scoreSounds = new List<AudioClip>();

	private ParticleSystem spaceEffect;

	private ParticleSystem.EmissionModule emission;

	private TextMeshProUGUI levelNumber;

	private GameObject TimeLine;

	private GameObject Level_Text;

	private Transform SkillsObj;

	private Transform rounded_Rectangle;

	private Transform roundedBar_left;

	private Transform roundedBar_body;

	private Transform roundedBar_right;

	private Transform readyGo;

	private Transform runningBall;

	private RunningBallController runningBallController;

	private Transform stopBall;

	private Transform danceLevelComp;

	private Image danceScoreImg;

	private TextMeshProUGUI perfectTimesText;

	private GameObject perfectTimesComp;

	private Image delImg;

	private Transform danceObj;

	private bool readyGoRunned;

	private int correctedIndex;

	private GameObject Score;

	private int perfectTimes;

	private float score;

	private int curLevel;

	private int level = 1;

	private int noSkills;

	private float startingPoint;

	private int delNum;

	private List<bool> delIndicies = new List<bool>();

	private List<KeyCode> requiredSkills = new List<KeyCode>();

	private bool isSpacePressed;

	private float ballPosition;

	private AudioSource audioSource;

	private static int PERFECT = 0;

	private static int GREAT = 1;

	private static int GOOD = 2;

	private static int MISS = 3;

	private float volumnScale = 0.3f;

	private float levelScore = 1f;

	private DanceController danceController;

	private DanceBehaviour danceBehaviour;

	public event OnPlayPerfectEffectDelegate OnPlayPerfectEffect;

	private void Start()
	{
		myScore.reset();
		danceBehaviour = CommonUtils.GetDanceBehaviour();
		danceObj = GameObject.Find("Screen/DanceObj").transform;
		levelNumber = base.transform.Find("Level_Number").GetComponent<TextMeshProUGUI>();
		TimeLine = base.transform.Find("TimeLine").gameObject;
		Level_Text = base.transform.Find("Level_Text").gameObject;
		SkillsObj = base.transform.Find("Skills").transform;
		rounded_Rectangle = base.transform.Find("Rounded_Rectangle").transform;
		roundedBar_left = base.transform.Find("Rounded_Rectangle/left").transform;
		roundedBar_body = base.transform.Find("Rounded_Rectangle/body").transform;
		roundedBar_right = base.transform.Find("Rounded_Rectangle/right").transform;
		readyGo = base.transform.Find("Ready").transform;
		runningBall = base.transform.Find("Running-Ball").transform;
		runningBallController = runningBall.GetComponent<RunningBallController>();
		stopBall = base.transform.Find("Stop-Ball").transform;
		stopBall.gameObject.SetActive(value: false);
		danceLevelComp = base.transform.Find("DanceScore").transform;
		danceScoreImg = danceLevelComp.GetComponent<Image>();
		perfectTimesComp = base.transform.Find("DanceScore/PerfectTimes").gameObject;
		perfectTimesText = perfectTimesComp.GetComponent<TextMeshProUGUI>();
		delImg = GameObject.Find("Screen/DanceObj/Del_Img").GetComponent<Image>();
		delImg.gameObject.SetActive(value: false);
		audioSource = GetComponent<AudioSource>();
		spaceEffect = GameObject.Find("Particle_Canvas/VfxStarWide").GetComponent<ParticleSystem>();
		emission = spaceEffect.emission;
	}

	public void startDance(bool isDelEnabled)
	{
		Debug.Log("[AuController] startDance");
		myScore.reset();
		Score = GameObject.Find("Screen/DanceObj/Score").gameObject;
		Score.GetComponent<TextMeshProUGUI>().SetText("0");
		if (danceController == null)
		{
			danceController = CommonUtils.GetDanceController();
		}
		danceController.myScore = myScore;
		DelSprites = danceController.DelSprites;
		danceScoreSprites = danceController.danceScoreSprites;
		CommonUtils.GetDanceBehaviour().registerDance(myScore);
		isStartedDance = true;
		delLevel = 0;
		base.isDelEnabled = isDelEnabled;
		level = 1;
		curLevel = 0;
		levelScore = 1f;
	}

	public void endDance()
	{
		runningBallController.enabled = false;
		isStartedDance = false;
		perfectTimes = 0;
		readyGoRunned = false;
		level = 1;
		curLevel = 0;
	}

	private void Update()
	{
		if (isStartedDance)
		{
			if (curLevel != level)
			{
				initLevel(wrongKeyPressed: false);
				curLevel++;
			}
			if (!readyGoRunned)
			{
				PlayReadyGo();
				StartCoroutine(enableRunningBallController(1.2f));
			}
			HandleUserInput();
			HandleDel();
			onBallReachEnd();
		}
	}

	private void onBallReachEnd()
	{
		if (ballPosition > runningBall.position.x)
		{
			if (CommonUtils.GetGameController().GetComponent<TimeController>().isSongEnded())
			{
				rounded_Rectangle.gameObject.SetActive(value: false);
				destroyOldSkills();
				return;
			}
			level++;
			correctedIndex = 0;
			stopBall.gameObject.SetActive(value: false);
			if (!isSpacePressed)
			{
				onSpacePressed(MISS);
			}
			isSpacePressed = false;
		}
		ballPosition = runningBall.position.x;
	}

	private void HandleUserInput()
	{
		if (requiredSkills.Count != correctedIndex && (Input.GetKeyDown(Global.LEFT) || Input.GetKeyDown(Global.RIGHT) || Input.GetKeyDown(Global.UP) || Input.GetKeyDown(Global.DOWN)))
		{
			if (Input.GetKeyDown(requiredSkills[correctedIndex]))
			{
				Transform child = SkillsObj.GetChild(correctedIndex);
				child.GetComponent<Image>().sprite = InstantiateSkillPrefabById(requiredSkills[correctedIndex], isCorrect: true, isDel: false).GetComponent<Image>().sprite;
				child.Find("Glow").GetComponent<Animation>().Play("white_sun_glow");
				correctedIndex++;
				audioSource.PlayOneShot(buttonPressedSound, volumnScale);
			}
			else
			{
				initLevel(wrongKeyPressed: true);
				correctedIndex = 0;
			}
		}
		if (Input.GetKeyDown("space") && requiredSkills.Count == correctedIndex)
		{
			float x = runningBall.localPosition.x;
			if (235f <= x && x <= 250f)
			{
				onSpacePressed(PERFECT);
			}
			else if ((220f <= x && x < 235f) || (250f < x && x <= 265f))
			{
				onSpacePressed(GREAT);
			}
			else if ((190f <= x && x < 220f) || (265f < x && x <= 290f))
			{
				onSpacePressed(GOOD);
			}
			else
			{
				onSpacePressed(MISS);
			}
			correctedIndex = 0;
			stopBall.position = runningBall.position;
			stopBall.gameObject.SetActive(value: true);
			isSpacePressed = true;
		}
	}

	private void onSpacePressed(int danceLevel)
	{
		rounded_Rectangle.gameObject.SetActive(value: false);
		destroyOldSkills();
		if (requiredSkills.Count == 0)
		{
			return;
		}
		float num = 0f;
		if (danceLevel == 0)
		{
			if (perfectTimes == 9 || perfectTimes == 14 || perfectTimes == 19)
			{
				audioSource.PlayOneShot(scoreSounds[5], volumnScale);
			}
			else if (perfectTimes == 4)
			{
				audioSource.PlayOneShot(scoreSounds[4], volumnScale);
			}
			else
			{
				audioSource.PlayOneShot(scoreSounds[danceLevel], volumnScale);
			}
		}
		else
		{
			audioSource.PlayOneShot(scoreSounds[danceLevel], volumnScale);
		}
		danceScoreImg.sprite = danceScoreSprites[danceLevel];
		danceLevelComp.GetComponent<Animation>().Play();
		perfectTimesText.SetText("");
		float num2 = (float)delNum * Global.DEL_SCORE;
		if (delNum > noSkills)
		{
			num2 = (float)noSkills * Global.DEL_SCORE;
		}
		int skill = Random.Range(1, 12);
		switch (danceLevel)
		{
		case 0:
			perfectTimes++;
			myScore.perfect++;
			myScore.perfectX = ((myScore.perfectX < perfectTimes) ? perfectTimes : myScore.perfectX);
			if (perfectTimes >= 2)
			{
				perfectTimesText.SetText("x" + perfectTimes);
			}
			num = (float)noSkills * Global.PERFECT_SCORE + (float)((perfectTimes - 1) * (15 * noSkills)) + num2;
			danceBehaviour.dance(skill, hasNormalizedTime: true);
			emission.burstCount = Mathf.Clamp(100 * perfectTimes, 0, 1000);
			spaceEffect.Play();
			break;
		case 1:
			myScore.great++;
			num = (float)noSkills * Global.GREAT_SCORE + num2;
			perfectTimes = 0;
			danceBehaviour.dance(skill, hasNormalizedTime: true);
			emission.burstCount = 50;
			spaceEffect.Play();
			break;
		case 2:
			myScore.good++;
			num = (float)noSkills * Global.GOOD_SCORE + num2;
			perfectTimes = 0;
			danceBehaviour.dance(skill, hasNormalizedTime: true);
			break;
		default:
			myScore.miss++;
			perfectTimes = 0;
			danceBehaviour.dance(0, hasNormalizedTime: true);
			break;
		}
		num *= levelScore;
		StartCoroutine(SetScore(num, danceLevel));
	}

	private IEnumerator SetScore(float acquiredScore, int danceLevel)
	{
		myScore.currentLevel = curLevel;
		myScore.noSkills = noSkills;
		myScore.delLevel = delLevel;
		if (acquiredScore == 0f)
		{
			CommonUtils.GetDanceBehaviour().updateScore(myScore);
			yield return null;
		}
		TextMeshProUGUI scoreText = Score.GetComponent<TextMeshProUGUI>();
		float processTime = 1f;
		for (float timer = 0f; timer < processTime; timer += Time.deltaTime)
		{
			float t = timer / processTime;
			scoreText.SetText(((int)Mathf.Lerp(myScore.score, myScore.score + acquiredScore, t)).ToString());
			yield return null;
		}
		myScore.score += acquiredScore;
		scoreText.SetText(myScore.score.ToString());
		myScore.danceLevel = danceLevel;
		myScore.perfectTimes = perfectTimes;
		danceController.myScore = myScore;
		CommonUtils.GetDanceBehaviour().updateScore(myScore);
	}

	private void PlayReadyGo()
	{
		readyGoRunned = true;
		readyGo.GetComponent<Animation>().Play();
		readyGo.GetComponent<AudioSource>().Play();
		StartCoroutine(ReadyGoTransition(0.8f));
	}

	private IEnumerator enableRunningBallController(float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		runningBallController.enabled = true;
		runningBallController.setBallWhenStartingDance();
		ballPosition = runningBallController.transform.position.x;
	}

	private IEnumerator ReadyGoTransition(float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		readyGo.GetComponent<Image>().sprite = goSprite;
		readyGo.GetComponent<Animation>().Play();
	}

	private void HandleDel()
	{
		if (!isStartedDance || !isDelEnabled)
		{
			return;
		}
		if (Input.GetKeyDown(Global.MINUS))
		{
			if (delLevel == 0)
			{
				delLevel = 6;
			}
			else
			{
				delLevel--;
			}
		}
		else if (Input.GetKeyDown(Global.EQUALS))
		{
			if (delLevel == 6)
			{
				delLevel = 0;
			}
			else
			{
				delLevel++;
			}
		}
		if (delLevel != 0)
		{
			delImg.sprite = DelSprites[delLevel - 1];
			delImg.gameObject.SetActive(value: true);
		}
		else
		{
			delImg.gameObject.SetActive(value: false);
		}
	}

	private void initLevel(bool wrongKeyPressed)
	{
		if (!wrongKeyPressed)
		{
			delIndicies.Clear();
		}
		destroyOldSkills();
		string text = danceController.danceMoves[level - 1];
		if (text != "")
		{
			if (text.Equals(Global.BREAK_TIME))
			{
				requiredSkills = new List<KeyCode>();
				ToggleDanceUI(isEnable: false);
				base.transform.Find("CrazyTime").GetComponent<Animation>().Play("CrazyTime");
			}
			else if (text.Equals(Global.NEW_ROUND))
			{
				requiredSkills = new List<KeyCode>();
				ToggleDanceUI(isEnable: true);
			}
			else
			{
				requiredSkills = JsonHelper.FromJson<List<KeyCode>>(text);
			}
		}
		else
		{
			requiredSkills = new List<KeyCode>();
		}
		noSkills = requiredSkills.Count;
		if (noSkills == 0)
		{
			rounded_Rectangle.gameObject.SetActive(value: false);
		}
		else
		{
			rounded_Rectangle.gameObject.SetActive(value: true);
		}
		if (noSkills != 0)
		{
			levelNumber.SetText(noSkills.ToString() ?? "");
		}
		startingPoint = getStartingPoint(noSkills);
		roundedBar_body.GetComponent<RectTransform>().sizeDelta = new Vector2(30 + (noSkills - 1) * 70, roundedBar_body.GetComponent<RectTransform>().sizeDelta.y);
		delNum = delLevel;
		for (int i = 0; i < noSkills; i++)
		{
			bool flag = false;
			if (!wrongKeyPressed)
			{
				flag = Random.Range(1, noSkills - i) <= delNum;
				if (flag)
				{
					delNum--;
				}
				delIndicies.Add(flag);
			}
			else
			{
				flag = delIndicies[i];
			}
			Transform obj = InstantiateSkillPrefabById(requiredSkills[i], isCorrect: false, flag).transform;
			obj.SetParent(SkillsObj);
			obj.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	private GameObject InstantiateSkillPrefabById(KeyCode keyCode, bool isCorrect, bool isDel)
	{
		if (!isCorrect)
		{
			if (keyCode == Global.LEFT)
			{
				return Object.Instantiate(isDel ? rightDelPref : leftPref);
			}
			if (keyCode == Global.DOWN)
			{
				return Object.Instantiate(isDel ? upDelPref : downPref);
			}
			if (keyCode == Global.UP)
			{
				return Object.Instantiate(isDel ? downDelPref : upPref);
			}
			if (keyCode == Global.RIGHT)
			{
				return Object.Instantiate(isDel ? leftDelPref : rightPref);
			}
		}
		else
		{
			if (keyCode == Global.LEFT)
			{
				return leftCorrectPref;
			}
			if (keyCode == Global.DOWN)
			{
				return downCorrectPref;
			}
			if (keyCode == Global.UP)
			{
				return upCorrectPref;
			}
			if (keyCode == Global.RIGHT)
			{
				return rightCorrectPref;
			}
		}
		return null;
	}

	private float getStartingPoint(int noSkills)
	{
		return -97f - (float)((noSkills - 6) * 24);
	}

	private void ToggleDanceUI(bool isEnable)
	{
		TimeLine.SetActive(isEnable);
		Level_Text.SetActive(isEnable);
		levelNumber.gameObject.SetActive(isEnable);
		runningBall.GetComponent<Image>().enabled = isEnable;
		delImg.enabled = isEnable;
	}

	private void destroyOldSkills()
	{
		foreach (Transform item in SkillsObj)
		{
			Object.Destroy(item.gameObject);
		}
	}
}
