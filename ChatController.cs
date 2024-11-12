using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatController : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IChatClientListener
{
	[Header("Settings")]
	public bool FocusOnEnterClick = true;

	public GameController gameController;

	public GameObject msgContainer;

	public int MaxMessages = 20;

	private Queue<GameObject> _currentMessages;

	private bool _allowFocusOnEnter = true;

	private bool _wasFocused;

	[Header("Colors")]
	public Color AllColor = Color.white;

	public Color ClubColor = Color.yellow;

	public Color SystemColor = Color.red;

	public Color PrivateColor = Color.green;

	public Color RoomColor = Color.gray;

	[Header("Components")]
	public GameObject MessagePrefab;

	public InputField InputField;

	private LayoutGroup MessagesList;

	private string currentChannel;

	private bool isCollapsedChatMsg;

	private string privateMsgReceiver;

	private string localPlayerId;

	private MainController mainController;

	private bool isPointerHover;

	private bool isEnterDanceMode;

	private bool joinedDefaultChannels;

	private bool listenedEvent;

	public List<string> JoinedChannels = new List<string>();

	[HideInInspector]
	public ChatClient chatClient;

	private GameObject ChannelsGO;

	private GameObject EraserGO;

	private GameObject BottomGO;

	private void Awake()
	{
		ChannelsGO = base.transform.Find("Channels").gameObject;
		EraserGO = base.transform.Find("Eraser").gameObject;
		BottomGO = base.transform.Find("Bottom").gameObject;
		chatClientConnect();
		isCollapsedChatMsg = true;
		currentChannel = Global.AutoJoinChannels[0];
		_currentMessages = new Queue<GameObject>();
		StartCoroutine(addListenerForInputField());
	}

	private void chatClientConnect()
	{
		chatClient = new ChatClient(this);
		chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, new AuthenticationValues(Global.user.id + "|" + Global.user.name));
		joinedDefaultChannels = false;
	}

	private IEnumerator addListenerForInputField()
	{
		yield return new WaitUntil(() => GameObject.Find(Global.localPlayerId) != null);
		InputField.onEndEdit.AddListener(delegate
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				OnSendClick();
			}
			if (mainController == null)
			{
				mainController = CommonUtils.GetMainController();
			}
			mainController.disableActionWhenTyping(isTyping: false);
		});
	}

	public void Disconnect()
	{
		chatClient.Disconnect();
	}

	public void OnConnected()
	{
		Debug.Log("Connected to Chat Server");
		StartCoroutine(subscribeDefaultChannels());
	}

	private IEnumerator subscribeDefaultChannels()
	{
		Debug.Log("ChatController -> Waiting for player spawning...");
		yield return new WaitUntil(() => GameObject.Find(Global.localPlayerId) != null);
		Debug.Log("ChatController -> Player is spawned. Go ahead now.");
		mainController = CommonUtils.GetMainController();
		joinDefaultChannels(Global.user);
	}

	public void joinDefaultChannels(User currentUser)
	{
		if (joinedDefaultChannels)
		{
			return;
		}
		if (currentUser.clubId != null && !currentUser.clubId.Equals(""))
		{
			Global.AutoJoinChannels.Add("CLUB");
		}
		localPlayerId = currentUser.id;
		foreach (string autoJoinChannel in Global.AutoJoinChannels)
		{
			if (autoJoinChannel.Equals("CLUB"))
			{
				joinChannel(currentUser.clubId);
			}
			else
			{
				joinChannel(autoJoinChannel);
			}
		}
		joinedDefaultChannels = true;
		JoinedChannels = Global.AutoJoinChannels;
	}

	public void OnGetMessages(string channelName, string[] senders, object[] messages)
	{
		GameObject gameObject = addMsgRowToChannel(channelName, senders, messages);
		if (!channelName.Equals("ALL"))
		{
			GameObject obj = addMsgRowToChannel("ALL", senders, messages);
			obj.GetComponent<Text>().color = gameObject.GetComponent<Text>().color;
			obj.transform.Find("Sender").GetComponent<Text>().color = gameObject.transform.Find("Sender").GetComponent<Text>().color;
		}
	}

	private GameObject addMsgRowToChannel(string channelName, string[] senders, object[] messages)
	{
		string userId = senders[senders.Length - 1].Split('|')[0];
		string nickname = senders[senders.Length - 1].Split('|')[1];
		GameObject textObject = GetTextObject(channelName);
		Text component = textObject.GetComponent<Text>();
		Text component2 = textObject.transform.Find("Sender").GetComponent<Text>();
		if (!userId.Equals("-1"))
		{
			textObject.transform.Find("Sender").GetComponent<Button>().onClick.RemoveAllListeners();
			textObject.transform.Find("Sender").GetComponent<Button>().onClick.AddListener(delegate
			{
				if (!Global.localPlayerId.Equals(userId))
				{
					openSubOption(userId, nickname);
				}
			});
		}
		string text = messages[messages.Length - 1].ToString();
		if (text.StartsWith("@"))
		{
			int num = text.IndexOf(":");
			text = text.Substring(num + 1, text.Length - (num + 1));
		}
		Debug.Log($"[{nickname}]: {text}");
		if (senders[senders.Length - 1].Equals(localPlayerId))
		{
			component2.gameObject.SetActive(value: false);
		}
		else
		{
			component2.text = $"[{nickname}]";
		}
		component.text = $"[{nickname}]: {text}";
		return textObject;
	}

	public void OnPrivateMessage(string sender, object message, string channelName)
	{
		Debug.Log("OnPrivateMessage");
	}

	private void Update()
	{
		if (chatClient != null)
		{
			chatClient.Service();
		}
		if (!listenedEvent)
		{
			if (GameObject.Find("GameController") != null)
			{
				gameController = GameObject.Find("GameController").GetComponent<GameController>();
				gameController.OnEnterDanceMode += enterDanceMode;
				gameController.OnEndDance += OnEndDance;
				listenedEvent = true;
			}
			return;
		}
		if (FocusOnEnterClick && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && _allowFocusOnEnter)
		{
			showUI(value: true);
			InputField.ActivateInputField();
		}
		if (InputField.isFocused)
		{
			if (mainController == null)
			{
				mainController = CommonUtils.GetMainController();
			}
			mainController.disableActionWhenTyping(isTyping: true);
		}
		if (isEnterDanceMode)
		{
			if (isPointerHover)
			{
				GetComponent<CanvasGroup>().alpha += Time.deltaTime * 1.5f;
			}
			else
			{
				GetComponent<CanvasGroup>().alpha -= Time.deltaTime * 1.5f;
			}
		}
		else
		{
			GetComponent<CanvasGroup>().alpha = 1f;
		}
	}

	public void showUI(bool value)
	{
		GetComponent<Image>().enabled = value;
		if ((bool)ChannelsGO)
		{
			ChannelsGO.SetActive(value);
		}
		if ((bool)EraserGO)
		{
			EraserGO.SetActive(value);
		}
		Image[] componentsInChildren = BottomGO.GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = value;
		}
		Text[] componentsInChildren2 = BottomGO.GetComponentsInChildren<Text>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = value;
		}
	}

	public void joinChannel(string channel)
	{
		StartCoroutine(IJoinChannel(channel));
	}

	private IEnumerator IJoinChannel(string channel)
	{
		yield return new WaitUntil(() => GameObject.Find(Global.localPlayerId) != null);
		chatClient.Subscribe(new string[1] { channel }, 0);
	}

	public void OnSubscribed(string[] channels, bool[] results)
	{
		string text = channels[channels.Length - 1];
		Debug.LogFormat("[OnSubscribed] You have joined a channel <b>{0}</b>. Previous Channel: <b>{1}</b>", text, currentChannel);
		if (text.Equals("CROSS_GAMEMASTER"))
		{
			return;
		}
		int num = -1;
		try
		{
			num = int.Parse(text);
		}
		catch (FormatException)
		{
			Debug.Log("Channel isn't ROOM or CLUB");
		}
		if (num != -1)
		{
			text = ((num < 1000) ? "ROOM" : "CLUB");
		}
		else if (text.Contains("-"))
		{
			text = "PRIVATE";
		}
		GameObject obj = UnityEngine.Object.Instantiate(msgContainer);
		obj.name = channels[channels.Length - 1] + "-Msg";
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		obj.transform.SetAsFirstSibling();
		obj.SetActive(channels[channels.Length - 1].Equals(currentChannel));
		GameObject channelBtn = null;
		channelBtn = base.transform.Find("Channels/" + text).gameObject;
		channelBtn.SetActive(value: true);
		JoinedChannels.Add(num.ToString());
		channelBtn.GetComponent<Button>().onClick.RemoveAllListeners();
		channelBtn.GetComponent<Button>().onClick.AddListener(delegate
		{
			changeChannel(channels[channels.Length - 1], channelBtn);
			if (GameObject.Find(Global.user.id) != null)
			{
				CommonUtils.GetMainController().disableActionWhenTyping(isTyping: false);
			}
			else
			{
				Debug.LogError("Character hasn't been spawned yet. DisableActionWhenTyping wasn't executed.");
			}
		});
		if (!text.Equals("SYSTEM"))
		{
			channelBtn.GetComponent<Button>().onClick.Invoke();
		}
	}

	public void changeToALLChannel()
	{
		base.transform.Find("Channels/ALL").GetComponent<Button>().onClick.Invoke();
	}

	public void changeChannel(string channelName, GameObject gameObject)
	{
		StartCoroutine(IchangeChannel(channelName, gameObject));
	}

	private IEnumerator IchangeChannel(string channelName, GameObject gameObject)
	{
		Debug.LogFormat("Change Channel from <b>{0}</b> to <b>{1}</b> : ", currentChannel, channelName);
		yield return new WaitUntil(() => base.transform.Find(currentChannel + "-Msg") != null);
		base.transform.Find(currentChannel + "-Msg").gameObject.SetActive(value: false);
		currentChannel = channelName;
		base.transform.Find(currentChannel + "-Msg").gameObject.SetActive(value: true);
		Transform transform = base.transform.Find("Bottom");
		UnityEngine.Object.Destroy(transform.Find("CurrentChannel").gameObject);
		GameObject obj = UnityEngine.Object.Instantiate(gameObject);
		obj.transform.SetParent(transform, worldPositionStays: false);
		obj.transform.SetAsFirstSibling();
		obj.name = "CurrentChannel";
		if (currentChannel.Contains("-") || currentChannel.Equals("PRIVATE"))
		{
			InputField.text = "@" + privateMsgReceiver + ": ";
		}
		else
		{
			InputField.text = "";
		}
		resizeMsgList(!isCollapsedChatMsg);
	}

	public void onJoinedPrivateChannel(string channel, string privateMsgReceiver)
	{
		this.privateMsgReceiver = privateMsgReceiver;
		joinChannel(channel);
	}

	public void joinPrivateChannel(string userId, string name)
	{
		string channel = gameController.getCurrentUser().id + "-" + userId;
		privateMsgReceiver = name;
		joinChannel(channel);
		mainController.JoinPrivateChannel(userId);
	}

	protected GameObject GetTextObject(string channelName)
	{
		GameObject gameObject;
		if (_currentMessages.Count >= MaxMessages)
		{
			gameObject = _currentMessages.Dequeue();
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(MessagePrefab);
			_currentMessages.Enqueue(gameObject);
		}
		Text component = gameObject.GetComponent<Text>();
		Text component2 = gameObject.transform.Find("Sender").GetComponent<Text>();
		MessagesList = base.transform.Find(channelName + "-Msg/List").GetComponent<LayoutGroup>();
		gameObject.transform.SetParent(MessagesList.transform);
		gameObject.transform.SetAsLastSibling();
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		_currentMessages.Enqueue(gameObject);
		string s = channelName;
		int num = -1;
		try
		{
			num = int.Parse(s);
		}
		catch (FormatException)
		{
			Debug.Log("Channel isn't ROOM or CLUB");
		}
		if (num != -1)
		{
			channelName = ((num < 1000) ? "ROOM" : "CLUB");
		}
		gameObject.SetActive(value: true);
		if (channelName.Equals("ALL"))
		{
			component.color = AllColor;
			component2.color = AllColor;
		}
		else if (channelName.Equals("SYSTEM"))
		{
			component.color = SystemColor;
			component2.color = SystemColor;
		}
		else if (channelName.Contains("-"))
		{
			component.color = PrivateColor;
			component2.color = PrivateColor;
		}
		else if (channelName.Equals("ROOM"))
		{
			component.color = RoomColor;
			component2.color = RoomColor;
		}
		else
		{
			component.color = ClubColor;
			component2.color = ClubColor;
		}
		return gameObject;
	}

	public virtual void OnSendClick()
	{
		string text = InputField.text;
		if (!string.IsNullOrEmpty(text))
		{
			bool num = text.StartsWith("@");
			Debug.Log($"Sending message: '{text}' to channel {currentChannel}");
			sendMessageToChannel(currentChannel, text);
			if (num)
			{
				InputField.text = "@" + privateMsgReceiver + ": ";
			}
			else
			{
				InputField.text = "";
			}
			InputField.DeactivateInputField();
			mainController.showSpeechMsg(text, currentChannel);
			if (_allowFocusOnEnter)
			{
				StartCoroutine(DontAllowFocusOnEnter());
			}
			mainController.disableActionWhenTyping(isTyping: false);
		}
	}

	public void sendMessageToChannel(string channel, string text)
	{
		chatClient.PublishMessage(channel, text);
	}

	protected IEnumerator DontAllowFocusOnEnter()
	{
		_allowFocusOnEnter = false;
		yield return new WaitForSeconds(0.2f);
		_allowFocusOnEnter = true;
	}

	private void OnUserLeftChannel(string channel, string user)
	{
		Debug.Log($"[{channel}] User '{user}' has left");
	}

	private void OnUserJoinedChannel(string channel, string user)
	{
		Debug.Log($"[{channel}] User '{user}' has joined");
	}

	private void openSubOption(string userId, string name)
	{
		Debug.Log("openSubOption : " + userId);
		Transform subOpts = base.transform.Find("SubOptions");
		subOpts.gameObject.SetActive(value: true);
		Button component = subOpts.Find("Options/Profile").GetComponent<Button>();
		component.onClick.RemoveAllListeners();
		component.onClick.AddListener(delegate
		{
			subOpts.gameObject.SetActive(value: false);
		});
		Button component2 = subOpts.Find("Options/AddFriend").GetComponent<Button>();
		component2.onClick.RemoveAllListeners();
		component2.onClick.AddListener(delegate
		{
			subOpts.gameObject.SetActive(value: false);
		});
		Button component3 = subOpts.Find("Options/Chat").GetComponent<Button>();
		component3.onClick.RemoveAllListeners();
		component3.onClick.AddListener(delegate
		{
			joinPrivateChannel(userId, name);
			subOpts.gameObject.SetActive(value: false);
		});
	}

	public void clearMsg()
	{
		MessagesList = base.transform.Find(currentChannel + "-Msg/List").GetComponent<LayoutGroup>();
		foreach (Transform item in MessagesList.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	public void resizeChatMsg()
	{
		Debug.Log("Resize Chat Msg Btn clicked");
		float height = GetComponent<RectTransform>().rect.height;
		GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width, isCollapsedChatMsg ? (height * 2f) : (height / 2f));
		resizeMsgList(isCollapsedChatMsg);
		EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>().Rotate(0f, 0f, isCollapsedChatMsg ? 180f : 0f);
		isCollapsedChatMsg = !isCollapsedChatMsg;
	}

	private void resizeMsgList(bool isCollapsed)
	{
		_ = base.transform.Find(currentChannel + "-Msg").GetComponent<RectTransform>().rect.height;
		base.transform.Find(currentChannel + "-Msg").GetComponent<RectTransform>().sizeDelta = new Vector2(base.transform.Find(currentChannel + "-Msg").GetComponent<RectTransform>().rect.width, isCollapsed ? 438f : 160f);
		Vector2 offsetMax = base.transform.Find(currentChannel + "-Msg").GetComponent<RectTransform>().offsetMax;
		base.transform.Find(currentChannel + "-Msg").GetComponent<RectTransform>().offsetMax = new Vector2(0f, offsetMax.y);
	}

	public void LeaveChannel(string channelId)
	{
		chatClient.Unsubscribe(new string[1] { channelId });
	}

	public void DebugReturn(DebugLevel level, string message)
	{
		if (level == DebugLevel.ERROR)
		{
			Debug.LogError("DebugReturn - message : " + message);
		}
	}

	public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
	{
		Debug.Log("OnStatusUpdate");
	}

	public void OnUserSubscribed(string channel, string user)
	{
		Debug.Log("OnUserSubscribed");
	}

	public void OnUserUnsubscribed(string channel, string user)
	{
		Debug.Log("OnUserUnsubscribed");
	}

	public void OnDisconnected()
	{
		Debug.LogError("ChatController - OnDisconnected: " + chatClient.State);
	}

	public void OnChatStateChange(ChatState state)
	{
		Debug.Log("OnChatStateChange : " + state);
	}

	public void OnUnsubscribed(string[] channels)
	{
		Debug.Log("[OnUnsubscribed] channel : " + channels[0]);
		string text = channels[0];
		JoinedChannels.Remove(text);
		int num = -1;
		try
		{
			num = int.Parse(text);
		}
		catch (FormatException)
		{
			Debug.Log("Channel isn't ROOM or CLUB");
		}
		if (num != -1)
		{
			text = ((num < 1000) ? "ROOM" : "CLUB");
		}
		if (base.transform.Find("Channels/" + text) != null)
		{
			base.transform.Find("Channels/" + text).gameObject.SetActive(value: false);
		}
		base.transform.Find("Channels/ALL").GetComponent<Button>().onClick.Invoke();
	}

	private void enterDanceMode()
	{
		isEnterDanceMode = true;
	}

	private void OnEndDance()
	{
		isEnterDanceMode = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		isPointerHover = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isPointerHover = false;
	}
}
