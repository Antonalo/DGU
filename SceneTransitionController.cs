using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Lean.Localization;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionController : MonoBehaviourPunCallbacks
{
	[Serializable]
	public struct SceneImage
	{
		public string sceneName;

		public string title;

		public List<string> playerIds;
	}

	public Transform danceRoomContent;

	public GameObject PopupMsg;

	public List<SceneImage> sceneImages;

	public GameObject Loading_Panel;

	private GameObject selectedRoomGO;

	private RoomInfo selectedRoom;

	private int retry;

	private void Start()
	{
		Debug.Log("[SceneTransitionController] Awake() executing: " + PhotonNetwork.IsConnected + " - " + PhotonNetwork.NetworkClientState);
		if (Global.user == null)
		{
			Debug.LogError("haven't logined. Back to Login scene");
			SceneManager.LoadScene("Login");
		}
		else if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated)
		{
			Loading_Panel.SetActive(value: true);
			PhotonNetwork.AuthValues = new AuthenticationValues();
			PhotonNetwork.AuthValues.UserId = Global.user.id;
			PhotonNetwork.AutomaticallySyncScene = true;
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("OnConnectedToMaster() was called by PUN.");
		Hashtable propertiesToSet = new Hashtable { { "isReady", false } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet);
		PhotonNetwork.JoinLobby(CommonUtils.getDefaultLobby());
	}

	public override void OnJoinedLobby()
	{
		Debug.Log("Joined Lobby with UserId: " + PhotonNetwork.LocalPlayer.UserId);
		Debug.Log("Region: " + PhotonNetwork.CloudRegion);
		PhotonNetwork.LocalPlayer.NickName = Global.user.name;
		Loading_Panel.SetActive(value: false);
	}

	public void OnCreateRoomClicked()
	{
		Loading_Panel.SetActive(value: true);
		RoomOptions defaultRoomOptions = CommonUtils.getDefaultRoomOptions();
		PhotonNetwork.CreateRoom(randomRoomName(), defaultRoomOptions);
	}

	private string randomRoomName()
	{
		return LeanLocalization.GetTranslationText("Room") + " " + UnityEngine.Random.Range(1, 100);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		retry++;
		Debug.LogError("Created Room Failed : " + returnCode + " - " + message + ". Retrying to create a new room (retryCount = " + retry + ") ...");
		if (returnCode == 32766 && retry < 3)
		{
			OnCreateRoomClicked();
		}
		else
		{
			SceneManager.LoadScene("Login");
		}
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("Created room [" + PhotonNetwork.CurrentRoom.Name + "] successfully");
		PhotonNetwork.LoadLevel("Main");
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		Debug.Log("OnRoomListUpdate");
		foreach (Transform item in danceRoomContent)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (RoomInfo room in roomList)
		{
			if (room.PlayerCount != 0)
			{
				GameObject roomGO = UnityEngine.Object.Instantiate((GameObject)Resources.Load("Prefabs/SceneTransition/RoomItem"));
				roomGO.name = room.Name;
				roomGO.transform.SetParent(danceRoomContent);
				roomGO.transform.localScale = new Vector3(1f, 1f, 1f);
				roomGO.transform.localPosition = Vector3.zero;
				roomGO.GetComponent<Image>().enabled = false;
				roomGO.transform.Find("RoomName").GetComponent<TextMeshProUGUI>().SetText(room.Name);
				roomGO.transform.Find("RoomCapacity").GetComponent<TextMeshProUGUI>().SetText(room.PlayerCount + "/" + room.MaxPlayers);
				TextMeshProUGUI component = roomGO.transform.Find("Status").GetComponent<TextMeshProUGUI>();
				component.SetText(room.IsOpen ? LeanLocalization.GetTranslationText("Waiting") : LeanLocalization.GetTranslationText("Playing"));
				component.color = (room.IsOpen ? Color.yellow : Color.red);
				roomGO.GetComponent<Button>().onClick.RemoveAllListeners();
				roomGO.GetComponent<Button>().onClick.AddListener(delegate
				{
					onRoomSelected(roomGO, room);
				});
			}
		}
		Loading_Panel.SetActive(value: false);
	}

	private void onRoomSelected(GameObject roomGO, RoomInfo room)
	{
		Debug.Log(room.Name + " is selected.");
		selectedRoom = room;
		if (selectedRoomGO != null)
		{
			selectedRoomGO.GetComponent<Image>().enabled = false;
		}
		selectedRoomGO = roomGO;
		selectedRoomGO.GetComponent<Image>().enabled = true;
	}

	public void closeMasterScreen()
	{
		CommonUtils.GetMasterScreen().SetActive(value: false);
	}

	public void togglePopupMsg(bool isOpen)
	{
		PopupMsg.SetActive(isOpen);
	}

	private void populateRoomList()
	{
	}

	public void onEnterRoomClicked()
	{
		if (selectedRoom == null || selectedRoom.Name.Equals(""))
		{
			togglePopupMsg(isOpen: true);
			PopupMsg.transform.Find("Panel/Msg").GetComponent<TextMeshProUGUI>().text = LeanLocalization.GetTranslationText("SelectRoomRequired");
		}
		else if (!selectedRoom.IsOpen)
		{
			togglePopupMsg(isOpen: true);
			PopupMsg.transform.Find("Panel/Msg").GetComponent<TextMeshProUGUI>().text = LeanLocalization.GetTranslationText("CannotEnterPlayingRoom");
		}
		else
		{
			Loading_Panel.SetActive(value: true);
			PhotonNetwork.JoinRoom(selectedRoom.Name);
		}
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.LogError("Join Room Failed : " + returnCode + " - " + message);
		SceneManager.LoadScene("Login");
	}

	public override void OnJoinedRoom()
	{
		Debug.Log(">>>> OnJoinedRoom called");
	}
}
