using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CommonUtils
{
	private static GameController gameController;

	private static Light mainLight;

	private static GameObject localPlayerGO;

	private static LobbyController lobbyController;

	private static Animator animator;

	private static DanceBehaviour danceBehaviour;

	private static CharacterInfoController characterInfoController;

	private static GameManager gameManager;

	private static DanceController danceController;

	private static ChatController chatController;

	private static Projector gridProjector;

	private static GameObject masterScreen;

	private static MainController mainController;

	public static string formatTime(float time)
	{
		string text = Mathf.Floor(time / 60f).ToString("00");
		string text2 = (time % 60f).ToString("00");
		return text + ":" + text2;
	}

	public static void RunOnChildrenRecursive(this GameObject go, Action<GameObject> action)
	{
		if (!(go == null))
		{
			action(go);
			Transform[] componentsInChildren = go.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				action(transform.gameObject);
			}
		}
	}

	public static RoomOptions getDefaultRoomOptions()
	{
		return new RoomOptions
		{
			PublishUserId = true,
			IsVisible = true,
			IsOpen = true,
			PlayerTtl = 0,
			EmptyRoomTtl = 0,
			MaxPlayers = 10
		};
	}

	public static void setLocalPlayerStatus(bool isReady)
	{
		ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable { { "isReady", isReady } };
		PhotonNetwork.LocalPlayer.SetCustomProperties(propertiesToSet);
	}

	public static bool isLocalPlayerReady()
	{
		return (bool)PhotonNetwork.LocalPlayer.CustomProperties["isReady"];
	}

	public static IEnumerator LoadURLToImg(Image img, string picture)
	{
		picture = Global.BASE_URL + picture;
		UnityWebRequest request = UnityWebRequestTexture.GetTexture(picture);
		yield return request.SendWebRequest();
		if (request.isNetworkError || request.isHttpError)
		{
			Debug.LogError(request.error + "(Image Not Found : " + picture + ")");
			yield break;
		}
		Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
		img.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0f, 0f));
	}

	public static UnityWebRequest prepareRequest(string url, string method, string requestBody = null, bool isDebug = true)
	{
		string text = Guid.NewGuid().ToString();
		if (isDebug)
		{
			Debug.LogFormat("[{0}] Sending request to URL : {1}", text, url);
		}
		UnityWebRequest unityWebRequest = new UnityWebRequest(url, method);
		if (requestBody != null)
		{
			if (isDebug)
			{
				Debug.LogFormat("[{0}] Request Body: {1}", text, requestBody);
			}
			byte[] bytes = new UTF8Encoding().GetBytes(requestBody);
			unityWebRequest.uploadHandler = new UploadHandlerRaw(bytes);
		}
		unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
		unityWebRequest.SetRequestHeader("Content-Type", "application/json");
		unityWebRequest.SetRequestHeader("X-Request-ID", text);
		unityWebRequest.SetRequestHeader("X-Authorization", Global.token);
		return unityWebRequest;
	}

	public static bool isNormalRoom(GameInfoType type)
	{
		return type == GameInfoType.Room;
	}

	public static bool isBossRoom(GameInfoType type)
	{
		return type == GameInfoType.Boss_Room;
	}

	public static bool isPKRoom(GameInfoType type)
	{
		return type == GameInfoType.PK_Room;
	}

	public static void showPopupMsg(string msg)
	{
		if (GameObject.Find("GameController") != null)
		{
			GameObject.Find("GameController").GetComponent<GameController>().openPopupMsg(msg);
		}
		else
		{
			Debug.LogError("GameController has been destroyed or can't find");
		}
	}

	public static ProfileController GetProfileController()
	{
		return GameObject.Find("Screen/Profile").GetComponent<ProfileController>();
	}

	public static void addNormalMsg(string msg)
	{
		if (GameObject.Find("GameController") != null)
		{
			GameObject.Find("GameController").GetComponent<GameController>().addNormalMsg(msg);
		}
		else
		{
			Debug.LogError("GameController has been destroyed or can't find");
		}
	}

	public static void setLoadingPanelActive(bool isShow)
	{
		if (GameObject.Find("GameController") != null)
		{
			GameObject.Find("GameController").GetComponent<GameController>().setLoadingPanelActive(isShow);
		}
		else
		{
			Debug.LogError("GameController has been destroyed or can't find");
		}
	}

	public static TypedLobby getDefaultLobby()
	{
		return new TypedLobby("Lobby1", LobbyType.Default);
	}

	public static string getCoupleTitle(int coupleLevel)
	{
		if (coupleLevel < 6)
		{
			return "Newlyweds";
		}
		if (coupleLevel < 11)
		{
			return "Happy Couple";
		}
		if (coupleLevel < 16)
		{
			return "Salty Couple";
		}
		if (coupleLevel < 21)
		{
			return "InLove Couple";
		}
		if (coupleLevel < 26)
		{
			return "Bronze Couple";
		}
		if (coupleLevel < 31)
		{
			return "Silver Couple";
		}
		if (coupleLevel < 36)
		{
			return "Golden Couple";
		}
		if (coupleLevel < 41)
		{
			return "Platinum Couple";
		}
		if (coupleLevel < 46)
		{
			return "Saphire Couple";
		}
		if (coupleLevel < 51)
		{
			return "Diamond Couple";
		}
		if (coupleLevel < 56)
		{
			return "Eternal Couple";
		}
		if (coupleLevel < 61)
		{
			return "Famous Couple";
		}
		if (coupleLevel < 66)
		{
			return "MoonStar Couple";
		}
		if (coupleLevel < 71)
		{
			return "Iris";
		}
		if (coupleLevel < 76)
		{
			return "Artemmis";
		}
		if (coupleLevel < 81)
		{
			return "Helios";
		}
		if (coupleLevel < 86)
		{
			return "Cupid";
		}
		if (coupleLevel < 91)
		{
			return "Athena";
		}
		if (coupleLevel < 96)
		{
			return "Poseidon";
		}
		return "Hera";
	}

	public static int calculateGold(ScoreBoardItem score)
	{
		return Mathf.RoundToInt(score.score / 5000f + (float)(5 * score.perfectX));
	}

	public static int calculateExp(ScoreBoardItem score)
	{
		return Mathf.RoundToInt(score.score / 1000f + (float)(10 * score.perfectX));
	}

	public static string readFile(string filePath)
	{
		Debug.Log("Reading File: Starting... : " + filePath);
		TextAsset textAsset = (TextAsset)Resources.Load(filePath, typeof(TextAsset));
		Debug.Log("Reading File: Done !");
		if (!(textAsset == null))
		{
			return textAsset.text;
		}
		return null;
	}

	public static void WriteFile(string filePath, string content)
	{
		Debug.Log("Writting File: Starting...");
		StreamWriter streamWriter = new StreamWriter(filePath, append: false);
		streamWriter.WriteLine(content);
		streamWriter.Close();
		Debug.Log("Writting File: Done !");
	}

	public static string wrapText(string sentence)
	{
		int num = 5;
		List<string> list = new List<string>(sentence.Split(' '));
		int num2 = num;
		for (int i = 0; i < list.Count / num; i++)
		{
			list.Insert(num2, "\n");
			num2 += num;
		}
		return string.Join(" ", list.ToArray());
	}

	public static void changeScene()
	{
	}

	public static List<T> FindObjectsOfTypeAll<T>()
	{
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded)
			{
				GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
				foreach (GameObject gameObject in rootGameObjects)
				{
					list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive: true));
				}
			}
		}
		return list;
	}

	public static InventoryController GetInventoryController()
	{
		GameObject gameObject = GameObject.Find("Inventory_Screen");
		if (!(gameObject == null))
		{
			return gameObject.GetComponent<InventoryController>();
		}
		return null;
	}

	public static GameController GetGameController()
	{
		if (gameController == null && GameObject.Find("GameController") != null)
		{
			gameController = GameObject.Find("GameController").GetComponent<GameController>();
		}
		return gameController;
	}

	public static Light GetMainLight()
	{
		if (mainLight == null)
		{
			mainLight = GameObject.Find("MainLight").GetComponent<Light>();
		}
		return mainLight;
	}

	public static SongSelectionController GetSongSelectionController()
	{
		return GameObject.Find("SongSelection").GetComponent<SongSelectionController>();
	}

	public static MediaPlayerController GetMediaPlayerController()
	{
		return GameObject.Find("Screen/MediaPlayer").GetComponent<MediaPlayerController>();
	}

	public static AudioSource GetSongPlayer()
	{
		return GameObject.Find("SongPlayer").GetComponent<AudioSource>();
	}

	public static GameObject GetLocalPlayer()
	{
		if (localPlayerGO == null)
		{
			localPlayerGO = GameObject.Find(Global.user.id);
		}
		return localPlayerGO;
	}

	public static LobbyController GetLobbyController()
	{
		if (lobbyController == null)
		{
			lobbyController = GameObject.Find("Lobby").GetComponent<LobbyController>();
		}
		return lobbyController;
	}

	public static Animator GetAnimator()
	{
		if (animator == null)
		{
			animator = GetLocalPlayer().GetComponent<Animator>();
		}
		return animator;
	}

	public static DanceBehaviour GetDanceBehaviour()
	{
		if (danceBehaviour == null)
		{
			danceBehaviour = GetLocalPlayer().GetComponent<DanceBehaviour>();
		}
		return danceBehaviour;
	}

	public static CharacterInfoController GetCharacterInfoController()
	{
		if (characterInfoController == null)
		{
			characterInfoController = GameObject.Find(Global.user.id + "/CharacterInfo").GetComponent<CharacterInfoController>();
		}
		return characterInfoController;
	}

	public static void setLayer(GameObject go, int layer)
	{
		go.layer = layer;
		go.RunOnChildrenRecursive(delegate(GameObject child)
		{
			child.layer = layer;
		});
	}

	public static GameManager GetGameManager()
	{
		if (gameManager == null && GameObject.Find("GameManager") != null)
		{
			return GameObject.Find("GameManager").GetComponent<GameManager>();
		}
		return gameManager;
	}

	public static DanceController GetDanceController()
	{
		if (danceController == null && (bool)GameObject.Find("DanceController"))
		{
			danceController = GameObject.Find("DanceController").GetComponent<DanceController>();
		}
		return danceController;
	}

	public static ChatController GetChatController()
	{
		if (chatController == null)
		{
			chatController = GameObject.Find("Screen/Chat").GetComponent<ChatController>();
		}
		return chatController;
	}

	public static Projector GetGridProjector()
	{
		if (gridProjector == null)
		{
			gridProjector = GameObject.Find("GridProjector").GetComponent<Projector>();
		}
		return gridProjector;
	}

	public static GameObject GetMasterScreen()
	{
		if (masterScreen == null)
		{
			masterScreen = GameObject.Find("MasterScreen");
		}
		return masterScreen;
	}

	public static GameObject GetMasterClientCamera()
	{
		return GameObject.Find("MasterClientCamera").gameObject;
	}

	public static GameObject GetBoss(int bossId)
	{
		GameObject gameObject = GameObject.Find("Boss_" + bossId + "(Clone)");
		if (!(gameObject == null))
		{
			return gameObject;
		}
		return null;
	}

	public static MainController GetMainController()
	{
		if (mainController == null && GetLocalPlayer() != null)
		{
			mainController = GetLocalPlayer().GetComponent<MainController>();
		}
		return mainController;
	}
}
