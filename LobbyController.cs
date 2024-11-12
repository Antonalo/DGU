using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Lean.Localization;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks
{
	public GameObject canvas;

	public Transform rows;

	public GameObject SongSelectionCanvas;

	[Header("========= CONTROLLER ========= ")]
	public Text SongName;

	public TextMeshProUGUI SongBpm;

	public GameObject ReadyBtn;

	public GameObject StartBtn;

	public Color ReadyColor = new Color(0.34901962f, 53f / 85f, 0.16078432f);

	public Color NotReadyColor = new Color(0.4f, 0.4f, 0.4f);

	private bool isRandomSong;

	[HideInInspector]
	public Song danceSong;

	private void Start()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			Debug.Log("You are game master. Auto ready now");
			CommonUtils.setLocalPlayerStatus(isReady: true);
		}
		StartBtn.SetActive(PhotonNetwork.IsMasterClient);
		ReadyBtn.SetActive(!PhotonNetwork.IsMasterClient);
		StartCoroutine(loadPlayerList());
	}

	private IEnumerator loadPlayerList()
	{
		Debug.Log("Finding local Player: " + Global.user.id);
		yield return new WaitUntil(() => CommonUtils.GetLocalPlayer() != null);
		foreach (Transform row in rows)
		{
			Object.Destroy(row.gameObject);
		}
		foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
		{
			Player value = player.Value;
			StartCoroutine(addPlayerRow(value));
		}
	}

	private IEnumerator addPlayerRow(Player player)
	{
		Debug.Log("Finding Player: " + player.UserId);
		yield return new WaitUntil(() => GameObject.Find(player.UserId) != null && GameObject.Find(player.UserId).GetComponent<MainController>() != null);
		bool flag = (bool)player.CustomProperties["isReady"] || player.IsMasterClient;
		MainController component = GameObject.Find(player.UserId).GetComponent<MainController>();
		GameObject obj = Object.Instantiate((GameObject)Resources.Load("Prefabs/Lobby/Row"));
		obj.name = player.UserId;
		obj.transform.SetParent(rows);
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		obj.GetComponent<Image>().color = (flag ? ReadyColor : NotReadyColor);
		obj.transform.Find("Username").GetComponent<Text>().text = player.NickName;
		obj.transform.Find("Level").GetComponent<Text>().text = "LVL. " + component.user.level;
		bool active = PhotonNetwork.IsMasterClient && !Global.user.id.Equals(player.UserId);
		obj.transform.Find("Kick").gameObject.SetActive(active);
		obj.transform.Find("Master_Client_Icon").gameObject.SetActive(player.IsMasterClient);
	}

	public void OnLeaveBtnClick()
	{
		CommonUtils.GetChatController().Disconnect();
		PhotonNetwork.LeaveRoom();
	}

	public override void OnLeftRoom()
	{
		Debug.LogError("[OnLeftRoom]: IsConnected = " + PhotonNetwork.IsConnected + " | InLobby: " + PhotonNetwork.InLobby + " | InRoom: " + PhotonNetwork.InRoom);
		SceneManager.LoadScene("Lobby");
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.LogError("[OnDisconnected]: IsConnected = " + PhotonNetwork.IsConnected + " | InLobby: " + PhotonNetwork.InLobby + " | InRoom: " + PhotonNetwork.InRoom);
		Debug.Log("Trying to reconnect...");
		PhotonNetwork.Reconnect();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Debug.Log("Player joined : [" + newPlayer.UserId + "] - " + newPlayer.NickName);
		StartCoroutine(addPlayerRow(newPlayer));
	}

	public override void OnPlayerLeftRoom(Player leftPlayer)
	{
		Debug.Log("Left Player: " + leftPlayer.NickName);
		Object.Destroy(rows.Find(leftPlayer.UserId).gameObject);
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.LogError("[OnMasterClientSwitched] is executing...: " + PhotonNetwork.IsMasterClient);
		CommonUtils.addNormalMsg(string.Format(LeanLocalization.GetTranslationText("MasterClientSwitched"), newMasterClient.NickName));
		if (PhotonNetwork.IsMasterClient)
		{
			Start();
		}
	}

	public void toggleSongSelection(bool isOn)
	{
		SongSelectionCanvas.SetActive(isOn);
	}

	public void onSongSelected()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			CommonUtils.showPopupMsg(LeanLocalization.GetTranslationText("OnlyRoomMasterCanSelectSong"));
			return;
		}
		base.photonView.RPC("RpcOnSongSelected", RpcTarget.AllBuffered, JsonHelper.ToJson(CommonUtils.GetSongSelectionController().selectedSong));
		toggleSongSelection(isOn: false);
	}

	[PunRPC]
	private void RpcOnSongSelected(string selectedSongJson)
	{
		Song song = JsonHelper.FromJson<Song>(selectedSongJson);
		SongName.text = song.name;
		SongBpm.SetText(song.bpm + " BPM");
	}

	public void OnPlayClick()
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		int num = 0;
		foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
		{
			Player value = player.Value;
			if (!(bool)value.CustomProperties["isReady"])
			{
				CommonUtils.addNormalMsg("<b>" + value.NickName + "</b> " + LeanLocalization.GetTranslationText("NotReady"));
				return;
			}
			hashtable.Add(value.UserId, num);
			num++;
		}
		if (CommonUtils.GetSongSelectionController().selectedSong.id < 2)
		{
			setSelectedSong(SongFactory.randomSong(), isRandomSong: true);
		}
		else
		{
			setSelectedSong(CommonUtils.GetSongSelectionController().selectedSong, isRandomSong: true);
		}
		PhotonNetwork.CurrentRoom.IsOpen = false;
		CommonUtils.GetDanceController().generateDanceMoves(CommonUtils.GetSongSelectionController().currentModeIndex);
		base.photonView.RPC("RpcPlayClick", RpcTarget.All, hashtable);
	}

	[PunRPC]
	private void RpcPlayClick(ExitGames.Client.Photon.Hashtable dancePositions)
	{
		CommonUtils.GetAnimator().SetInteger("Song", danceSong.id);
		canvas.SetActive(value: false);
		Transform transform = CommonUtils.GetGameManager().spawnPosition(dancePositions[Global.user.id].ToString());
		CommonUtils.GetLocalPlayer().transform.position = transform.position;
		CommonUtils.GetLocalPlayer().transform.rotation = Quaternion.identity;
		Debug.Log("Start dance with song Id : " + danceSong.id);
		CommonUtils.GetGameController().startGame(danceSong);
	}

	public void setSelectedSong(Song selectedSong, bool isRandomSong)
	{
		if (selectedSong == null)
		{
			selectedSong = SongFactory.getSongList()[0];
		}
		Debug.Log("setSelectedSong : " + selectedSong.id);
		this.isRandomSong = isRandomSong;
		base.photonView.RPC("RpcSelectedSong", RpcTarget.All, selectedSong.ToJson());
	}

	[PunRPC]
	private void RpcSelectedSong(string songJson)
	{
		danceSong = JsonUtility.FromJson<Song>(songJson);
		setSongInfo();
	}

	public void setSongInfo()
	{
		Debug.Log("setSongInfo : " + danceSong.fileName);
		Transform obj = GameObject.Find("Screen/DanceObj/Time").transform;
		obj.Find("SongName").GetComponent<TextMeshProUGUI>().SetText(danceSong.name);
		obj.Find("bpm").GetComponent<TextMeshProUGUI>().SetText("BPM " + danceSong.bpm);
		GameObject.Find("Screen/DanceObj/AU/Running-Ball").GetComponent<RunningBallController>().bpm = float.Parse(danceSong.bpm);
		Debug.Log("song.id : " + danceSong.id);
		CommonUtils.GetAnimator().SetInteger("Song", danceSong.id);
		Transform obj2 = GameObject.Find("Screen/Intro").transform;
		obj2.Find("Song").GetComponent<Text>().text = danceSong.name;
		obj2.Find("BPM_Value").GetComponent<TextMeshProUGUI>().SetText("BPM " + danceSong.bpm);
	}

	public virtual void OnReadyClick()
	{
		bool flag = (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];
		CommonUtils.setLocalPlayerStatus(!flag);
		base.photonView.RPC("RpcOnReadyClick", RpcTarget.All, PhotonNetwork.LocalPlayer.UserId, !flag);
	}

	[PunRPC]
	public void RpcOnReadyClick(string playerId, bool isReady)
	{
		Debug.Log("RpcOnReadyClick : " + playerId);
		rows.Find(playerId).GetComponent<Image>().color = (isReady ? ReadyColor : NotReadyColor);
	}
}
