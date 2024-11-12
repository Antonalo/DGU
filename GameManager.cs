using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
	private Transform spawnPositions;

	private void Start()
	{
		if (!PhotonNetwork.IsConnected)
		{
			Debug.LogError("Photon is disconnected. Go back Login Screen.");
			SceneManager.LoadScene("Login");
		}
		else
		{
			spawnPositions = base.transform.Find("SpawnPositions");
			spawnCharacter();
		}
	}

	private void spawnCharacter()
	{
		Debug.Log("spawnCharacter : " + JsonHelper.ToJson(Global.user));
		CommonUtils.GetMasterClientCamera().GetComponent<AudioListener>().enabled = false;
		Vector3 position = spawnPositions.Find(Random.Range(0, spawnPositions.childCount).ToString()).position;
		GameObject player = PhotonNetwork.Instantiate("Prefabs/Character/Character", position, Quaternion.identity, 0);
		CommonUtils.GetMediaPlayerController().init();
		CommonUtils.GetMediaPlayerController().onNextBtnClicked();
		StartCoroutine(fetchInUsedItems(player));
	}

	private IEnumerator fetchInUsedItems(GameObject player)
	{
		UnityWebRequest request = CommonUtils.prepareRequest(Global.ITEM_URL + "/inused", "GET");
		yield return request.SendWebRequest();
		string text = request.downloadHandler.text;
		Debug.Log("[fetchInUsedItems] response: " + text);
		JSONNode jSONNode = JSON.Parse(text);
		RespStatus respStatus = JsonUtility.FromJson<RespStatus>(jSONNode["status"].ToString());
		if (respStatus.code != 0)
		{
			Debug.LogError("Fail to fetchInUsedItems");
			CommonUtils.showPopupMsg(respStatus.message);
			yield break;
		}
		string json = "{\"Items\":" + jSONNode["data"].ToString() + "}";
		Global.inUsedItems = JsonHelper.FromJson<List<Item>>(json);
		foreach (Item item in JsonHelper.FromJson<List<Item>>(json))
		{
			if (item.isItemType(Item.CharacterPart.CHARACTER))
			{
				player.GetComponent<CharacterSwapper>().swap(item, isShop: false);
			}
		}
		player.GetComponent<MainController>().Setup(Global.user);
	}

	public Transform spawnPosition(string position)
	{
		return spawnPositions.Find(position);
	}

	private void RaiseDummyEventToRefreshToken()
	{
		Debug.Log("Raise Dummy Event To Refresh Token: " + PhotonNetwork.RaiseEvent(0, null, null, SendOptions.SendUnreliable));
	}
}
