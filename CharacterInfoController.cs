using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CharacterInfoController : MonoBehaviour
{
	public Image vipBadge;

	public List<Sprite> vipBadgeSprites;

	public Image ringBadge;

	public List<Sprite> ringBadgeSprites;

	public Transform achievementParent;

	private Transform player;

	private User user;

	private Camera cam;

	private void Awake()
	{
	}

	private void Start()
	{
		StartCoroutine(WaitForCameraSpawned());
	}

	public void setNameAndLevel(string bossName, int level)
	{
		base.transform.Find("Name").GetComponent<TextMesh>().text = bossName;
		setLevel(level);
	}

	private IEnumerator WaitForCameraSpawned()
	{
		yield return new WaitUntil(() => CommonUtils.GetMainController() != null);
		yield return new WaitUntil(() => CommonUtils.GetMainController().thirdPersonCamera != null);
		cam = CommonUtils.GetMainController().thirdPersonCamera.GetComponent<Camera>();
		refreshCharacterInfo();
		loadAchievementBadges();
	}

	public void loadAchievementBadges()
	{
		if (achievementParent == null)
		{
			return;
		}
		foreach (Transform item in achievementParent)
		{
			Object.Destroy(item.gameObject);
		}
		StartCoroutine(getAchievementBadges());
	}

	private IEnumerator getAchievementBadges()
	{
		UnityWebRequest request = CommonUtils.prepareRequest(Global.ACHIEVEMENT_URL + "/badge/" + base.transform.root.name, "GET");
		yield return request.SendWebRequest();
		Response<List<string>> response = JsonUtility.FromJson<Response<List<string>>>(request.downloadHandler.text);
		_ = response.data;
		if (response.status.code != 0)
		{
			Debug.LogError("Fail to Get Achievement Badges");
			CommonUtils.showPopupMsg(response.status.message);
			yield break;
		}
		Debug.Log("Success to Get Achievement Badges");
		foreach (string datum in response.data)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Achievement/Badge") as GameObject);
			gameObject.transform.SetParent(achievementParent, worldPositionStays: false);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Achievement/" + datum);
			gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObject.GetComponent<Image>().sprite.bounds.size.x * 100f, gameObject.GetComponent<Image>().sprite.bounds.size.y * 100f);
		}
	}

	public void refreshCharacterInfo()
	{
		StartCoroutine(initCharacterInfo());
	}

	private IEnumerator initCharacterInfo()
	{
		player = base.transform.parent;
		MainController mainController = player.GetComponent<MainController>();
		Debug.Log("[CharacterInfoController] Waiting for getting user [" + player.name + "] info...");
		yield return new WaitUntil(() => !(null == mainController) && mainController.user != null && mainController.user.name != null && !("" == mainController.user.name));
		Debug.Log("[CharacterInfoController] Getting user[" + player.name + "] info successfully");
		user = mainController.user;
		showVIPBadge();
		showRingBadge();
		base.transform.Find("Name").GetComponent<TextMesh>().text = user.name;
		setLevel(user.level);
		if (user.clubId == null || user.clubId == "")
		{
			base.transform.Find("ClubName").gameObject.SetActive(value: false);
			yield break;
		}
		base.transform.Find("ClubName").GetComponent<TextMesh>().text = "『" + user.clubName + "』";
		base.transform.Find("ClubName").gameObject.SetActive(value: true);
	}

	public void setLevel(int level)
	{
		base.transform.Find("Level").GetComponent<TextMeshPro>().SetText("Lv." + level);
	}

	private void showVIPBadge()
	{
		if (user.vip > 0)
		{
			int index = ((user.vip > vipBadgeSprites.Count) ? vipBadgeSprites.Count : user.vip) - 1;
			vipBadge.sprite = vipBadgeSprites[index];
			vipBadge.gameObject.SetActive(value: true);
		}
		else
		{
			vipBadge.gameObject.SetActive(value: false);
		}
	}

	private void showRingBadge()
	{
		if (!(ringBadge == null))
		{
			if (user.coupleLevel > 0)
			{
				ringBadge.sprite = getRingImage(user.coupleLevel);
				ringBadge.gameObject.SetActive(value: true);
			}
			else
			{
				ringBadge.gameObject.SetActive(value: false);
			}
		}
	}

	public Sprite getRingImage(int coupleLevel)
	{
		int index = (((coupleLevel > ringBadgeSprites.Count * 5) ? (ringBadgeSprites.Count * 5) : coupleLevel) - 1) / 5;
		return ringBadgeSprites[index];
	}

	private void Update()
	{
		if ((bool)cam)
		{
			base.transform.rotation = cam.transform.rotation;
		}
	}
}
