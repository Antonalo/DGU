using System.Collections;
using Lean.Localization;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviourPunCallbacks
{
	public delegate void OnEnterDanceModeDelegate();

	public delegate void OnEndDanceDelegate();

	public GameObject danceBtn;

	private Transform Intro;

	public AudioClip DanceIntroSound;

	public GameObject adminController;

	private TimeController timeController;

	private Transform songPlayer;

	[HideInInspector]
	public bool IsDancing;

	[HideInInspector]
	public GameObject ScreenIcons;

	[HideInInspector]
	public GameObject Profile;

	public GameObject blurBackground;

	private GameObject LoadingPanel;

	private GameObject PopupMsg;

	private GameObject userProfile;

	public Transform Screen;

	public GameObject NormalMsg;

	private GameObject friendList;

	[HideInInspector]
	public GameObject SettingsScreen;

	[HideInInspector]
	public string selectedUserId;

	[Header("=== ICON NOTIFICATIONS === ")]
	public GameObject userProfileNotif;

	public GameObject questNotif;

	public GameObject friendNotif;

	public GameObject clubNotif;

	public GameObject bagNotif;

	public GameObject mailNotif;

	[Header("========= POP UP ========= ")]
	public GameObject RoomInvitationPopup;

	public GameObject CoupleOfferPopup;

	public GameObject BreakupPopup;

	public GameObject LevelUpPopup;

	public GameObject RewardPopup;

	public GameObject PKRewardPopup;

	public GameObject PKRankingPopup;

	public GameObject ConfirmPopup;

	[Header("========= SCREEN ========= ")]
	public GameObject WeddingScreen;

	[Header("========= BATTLE SONGS ========= ")]
	public AudioClip[] battleBackgroundSongs;

	private AudioSource audioSource;

	[HideInInspector]
	public static bool quitNow;

	[Header("========= SETTINGS ========= ")]
	public TextMeshProUGUI FPSText;

	public Text SongDescription;

	public TextMeshProUGUI SongBpm;

	private float deltaTime;

	private bool showFPS;

	private float updateInterval = 0.5f;

	private float accum;

	private float frames;

	private float timeleft;

	private bool isRandomSong = true;

	[HideInInspector]
	public GameInfoPacket room;

	public event OnEnterDanceModeDelegate OnEnterDanceMode;

	public event OnEndDanceDelegate OnEndDance;

	private void Start()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		Intro = Screen.Find("Intro").transform;
		songPlayer = GameObject.Find("SongPlayer").transform;
		audioSource = songPlayer.GetComponent<AudioSource>();
		timeController = GetComponent<TimeController>();
		ScreenIcons = Screen.transform.Find("Icons").gameObject;
		Profile = Screen.transform.Find("Profile").gameObject;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		Global.isSceneloaded = true;
	}

	private void Awake()
	{
	}

	public void deletePersistentObj()
	{
		Debug.Log("Deleting Persistent_Obj - count : " + GameObject.FindGameObjectsWithTag("Persistent_Obj").Length);
		GameObject[] array = GameObject.FindGameObjectsWithTag("Persistent_Obj");
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i]);
		}
	}

	private void Update()
	{
		if (IsDancing && (timeController.isTimeout() || Input.GetKeyDown(Global.MINUS)))
		{
			endDanceEvent();
		}
		if (showFPS)
		{
			deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
			FPSText.SetText("FPS: " + (int)(1f / deltaTime));
		}
	}

	private void LateUpdate()
	{
		if (showFPS)
		{
			timeleft -= Time.deltaTime;
			accum += Time.timeScale / Time.deltaTime;
			frames += 1f;
			if (timeleft <= 0f)
			{
				timeleft = updateInterval;
				accum = 0f;
				frames = 0f;
			}
		}
	}

	private void SetFPS()
	{
		if (showFPS && accum > 0f && frames > 0f)
		{
			FPSText.SetText("FPS: " + (accum / frames).ToString("f0"));
		}
	}

	public void setShowFPS(bool showFPS)
	{
		this.showFPS = showFPS;
		FPSText.gameObject.SetActive(showFPS);
	}

	private void endDanceEvent()
	{
		IsDancing = false;
		this.OnEndDance();
		CommonUtils.GetMainController().thirdPersonCamera.setIsDancing(isDancing: false);
		CommonUtils.GetDanceBehaviour().endDance();
		CommonUtils.GetDanceController().endDance();
	}

	public void startGame(Song danceSong)
	{
		CommonUtils.GetMainController().thirdPersonCamera.setIsDancing(isDancing: true);
		CommonUtils.GetMainController().thirdPersonCamera.playerPos = CommonUtils.GetLocalPlayer().transform.position;
		CommonUtils.GetMediaPlayerController().StopSong();
		CommonUtils.GetMediaPlayerController().GetComponent<Animation>().Play("MediaPlayer_MoveOut");
		ScreenIcons.GetComponent<Animation>().Play("Icon_MoveOut");
		Profile.GetComponent<Animation>().Play("Profile_MoveOut");
		this.OnEnterDanceMode();
		StartCoroutine(runIntroAndStartDancing(1.6f, danceSong));
	}

	private IEnumerator runIntroAndStartDancing(float delayTime, Song danceSong)
	{
		yield return new WaitForSeconds(delayTime);
		Intro.GetComponent<Animation>().Play();
		audioSource.PlayOneShot(DanceIntroSound);
		yield return new WaitForSeconds(DanceIntroSound.length);
		playDanceSong(danceSong);
		StartCoroutine(startDancing(3.5f));
	}

	private IEnumerator startDancing(float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		IsDancing = true;
		CommonUtils.GetDanceController().startDance();
	}

	public void playDanceSong(Song danceSong)
	{
		audioSource.clip = Resources.Load<AudioClip>(danceSong.fileName);
		audioSource.Play();
		Debug.Log("danceSong.fileName : " + danceSong.fileName);
		timeController.startGame(audioSource.clip.length);
	}

	public void onCloseLevelUpPopup()
	{
		LevelUpPopup.SetActive(value: false);
	}

	public void OnDanceHallBtnClicked()
	{
	}

	public void OnDanceHallClose()
	{
	}

	public void displaySettingsScreen(bool isShow)
	{
		SettingsScreen.SetActive(isShow);
	}

	public void closePopupMsg()
	{
		PopupMsg.SetActive(value: false);
	}

	public void openPopupMsg(string msg)
	{
		PopupMsg.SetActive(value: true);
		PopupMsg.transform.Find("Panel/Msg").GetComponent<TextMeshProUGUI>().text = msg;
	}

	public void addNormalMsg(string msg)
	{
		Transform parent = NormalMsg.transform.Find("Messages");
		GameObject obj = Object.Instantiate(Resources.Load("Prefabs/Normal_Msg") as GameObject, parent);
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		obj.transform.Find("Msg").GetComponent<Text>().text = msg;
	}

	public void showConfirmPopup(bool isDisplay, IEnumerator yesHandler = null, string title = "", string content = "")
	{
		ConfirmPopup.SetActive(isDisplay);
		ConfirmPopup.transform.Find("Panel/Msg").GetComponent<Text>().text = content;
		Button component = ConfirmPopup.transform.Find("Panel/Btn_Grp/OK_Btn").GetComponent<Button>();
		component.onClick.RemoveAllListeners();
		component.onClick.AddListener(delegate
		{
			if (yesHandler != null)
			{
				StartCoroutine(yesHandler);
			}
		});
		ConfirmPopup.transform.Find("Panel/Btn_Grp/NO_Btn").GetComponent<Button>().onClick.AddListener(delegate
		{
			ConfirmPopup.SetActive(value: false);
		});
	}

	public void setLoadingPanelActive(bool isShow)
	{
		LoadingPanel.SetActive(isShow);
	}

	public User getCurrentUser()
	{
		return CommonUtils.GetMainController().user;
	}

	public void displayUserProfileNotif(bool isDisplay)
	{
		userProfileNotif.SetActive(isDisplay);
	}

	public void displayQuestNotif(bool isDisplay)
	{
		questNotif.SetActive(isDisplay);
	}

	public void displayFriendNotif(bool isDisplay)
	{
		friendNotif.SetActive(isDisplay);
	}

	public void toggleBag(bool isOn)
	{
		toggleInventoryController(isOn, isShop: false, LeanLocalization.GetTranslationText("CannotEnterBagWhenReady"));
	}

	public void toggleInventory(bool isOn)
	{
		toggleInventoryController(isOn, isShop: true, LeanLocalization.GetTranslationText("CannotEnterShopWhenReady"));
	}

	private void toggleInventoryController(bool isOn, bool isShop, string errMsg)
	{
		if (!PhotonNetwork.IsMasterClient && CommonUtils.isLocalPlayerReady())
		{
			CommonUtils.addNormalMsg(errMsg);
			return;
		}
		if (isOn)
		{
			CommonUtils.GetMainController().enterIdleMode();
		}
		else
		{
			CommonUtils.GetMainController().enterNormalMode();
		}
		CommonUtils.GetMainController().setInventoryCameraForOnScreenUI(isOn);
		CommonUtils.GetInventoryController().toggle(isOn, isShop);
		CommonUtils.GetCharacterInfoController().gameObject.SetActive(!isOn);
		danceBtn.SetActive(!isOn);
		ScreenIcons.SetActive(!isOn);
		CommonUtils.GetLobbyController().canvas.SetActive(!isOn);
		Screen.gameObject.SetActive(!isOn);
	}
}
