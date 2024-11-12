using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using VikingCrewTools.UI;

public class MainController : MonoBehaviourPunCallbacks
{
	public delegate void OnUserUpdateDelegate(User user);

	public User user = new User();

	public Camera resultCamera;

	private Camera inventoryCamera;

	private Animator anim;

	public bool isUserTyping;

	private float rotationSpeed = 0.2f;

	public GameObject levelUpParticle;

	private bool isInInventory;

	[HideInInspector]
	public Transform backgroundImage;

	[HideInInspector]
	public ThirdPersonOrbitCamBasic thirdPersonCamera;

	[HideInInspector]
	public float maxScale;

	[HideInInspector]
	public float minScale;

	[HideInInspector]
	public bool isUserUpdating;

	private Transform speechBubblePos;

	private Canvas OnScreenUI;

	public event OnUserUpdateDelegate onUserUpdate;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		isUserTyping = false;
		speechBubblePos = base.transform.Find("CharacterInfo/Speech");
		OnScreenUI = GameObject.Find("OnScreenUI").GetComponent<Canvas>();
		inventoryCamera = CommonUtils.GetInventoryController().inventoryCamera;
	}

	private void Start()
	{
		if (GetComponent<PhotonView>().IsMine)
		{
			CommonUtils.GetGameController().OnEnterDanceMode += enterDanceMode;
		}
	}

	public int getNpcPartnerViewId()
	{
		return -1;
	}

	public void triggerUserUpdatedEvent(bool updateClothes)
	{
		StartCoroutine(getUserInfo(updateClothes));
	}

	private IEnumerator getUserInfo(bool updateClothes)
	{
		isUserUpdating = true;
		UnityWebRequest request = CommonUtils.prepareRequest(Global.USER_URL, "GET");
		yield return request.SendWebRequest();
		Response<User> response = JsonUtility.FromJson<Response<User>>(request.downloadHandler.text);
		if (response.status.code != 0)
		{
			Debug.LogError("Fail to get User Information: " + user.id);
			CommonUtils.showPopupMsg(response.status.message);
		}
		else
		{
			setLocalUser(response.data, updateClothes);
		}
		isUserUpdating = false;
	}

	public void setLocalUser(User user, bool updateClothes)
	{
		Debug.Log("setLocalUser: " + base.name + " | " + user.name);
		if (Global.user.id.Equals(user.id))
		{
			base.photonView.RPC("RpcSetUserJson", RpcTarget.AllBuffered, JsonUtility.ToJson(user));
			CommonUtils.GetProfileController().setupProfile(user);
		}
	}

	[PunRPC]
	public void RpcSetUserJson(string userJson)
	{
		user = JsonUtility.FromJson<User>(userJson);
		base.name = user.id;
	}

	public void setupCamera()
	{
		if (!(thirdPersonCamera != null) && GetComponent<PhotonView>().IsMine)
		{
			string text = "3rdPersonCamera";
			thirdPersonCamera = Object.Instantiate(Resources.Load<ThirdPersonOrbitCamBasic>("Prefabs/" + text), new Vector3(0f, 0f, 0f), Quaternion.identity);
			thirdPersonCamera.player = base.transform;
			thirdPersonCamera.name = Global.localPlayerId + "-Camera";
			Canvas component = GameObject.Find("Particle_Canvas").GetComponent<Canvas>();
			component.worldCamera = thirdPersonCamera.GetComponent<Camera>();
			component.planeDistance = 2.5f;
			component.sortingOrder = 99;
			GetComponent<BasicBehaviour>().SetupCamera(thirdPersonCamera.transform);
		}
	}

	public void setResultCameraForOnScreenUI(bool isOn)
	{
		OnScreenUI.worldCamera = resultCamera;
		resultCamera.gameObject.SetActive(isOn);
		CommonUtils.setLayer(base.gameObject, isOn ? Global.characterOnUILayer : Global.characterLayer);
	}

	public void setInventoryCameraForOnScreenUI(bool isOn)
	{
		OnScreenUI.worldCamera = inventoryCamera;
		inventoryCamera.transform.position = base.transform.Find("Inventory_Cam_Pos").position;
		inventoryCamera.transform.rotation = base.transform.Find("Inventory_Cam_Pos").rotation;
		inventoryCamera.gameObject.SetActive(isOn);
		isInInventory = isOn;
		thirdPersonCamera.setIsInventory(isOn);
		OnScreenUI.transform.Find("Inventory_BG").gameObject.SetActive(isOn);
		CommonUtils.setLayer(base.gameObject, isOn ? Global.characterOnUILayer : Global.characterLayer);
	}

	public void showSpeechMsg(string msg, string channelId)
	{
		base.photonView.RPC("RpcShowSpeechMsg", RpcTarget.All, base.name, msg, channelId);
	}

	[PunRPC]
	private void RpcShowSpeechMsg(string userId, string msg, string channelId)
	{
		if (base.name.Equals(userId) && CommonUtils.GetChatController().JoinedChannels.Contains(channelId))
		{
			SpeechBubbleManager.Instance.AddSpeechBubble(speechBubblePos, CommonUtils.wrapText(msg));
		}
	}

	private void Update()
	{
		if (isInInventory)
		{
			RotateAndZoom();
		}
	}

	private IEnumerator playLevelUpParticle(ParticleSystem p, float delayTime)
	{
		p.Play();
		yield return new WaitForSeconds(delayTime);
		p.Play();
		yield return new WaitForSeconds(delayTime);
		p.Play();
	}

	private void RotateAndZoom()
	{
		if (Input.GetMouseButton(0))
		{
			float angle = Input.GetAxis("Mouse X") * rotationSpeed * 20f;
			float y = Input.GetAxis("Mouse Y") * rotationSpeed;
			base.transform.Rotate(Vector3.down, angle);
			inventoryCamera.transform.position -= new Vector3(0f, y, 0f);
		}
		float num = Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * 20f;
		inventoryCamera.orthographicSize -= num;
		Vector3 vector = new Vector3(num, 0f, 0f);
		inventoryCamera.transform.localPosition += vector;
	}

	public void Setup(User user)
	{
		setLocalUser(user, updateClothes: true);
		anim.SetBool("isMale", this.user.isMale());
		setupCamera();
	}

	public void disableActionWhenTyping(bool isTyping)
	{
		isUserTyping = isTyping;
		if (!GetComponent<Animator>().GetBool("Dance") && isTyping && !anim.GetBool("Dance"))
		{
			GetComponent<Animator>().SetTrigger("Idle");
		}
	}

	public void JoinPrivateChannel(string receiverUserId)
	{
		string text = CommonUtils.GetGameController().getCurrentUser().id + "-" + receiverUserId;
		base.photonView.RPC("RPCTargetJoinPrivateChannel", RpcTarget.Others, text, CommonUtils.GetGameController().getCurrentUser().name, receiverUserId);
	}

	[PunRPC]
	public void RPCTargetJoinPrivateChannel(string channelId, string nickname, string receiverUserId)
	{
		if (Global.localPlayerId.Equals(receiverUserId))
		{
			CommonUtils.GetChatController().onJoinedPrivateChannel(channelId, nickname);
		}
	}

	public void enterDanceMode()
	{
		Debug.Log("enterDanceMode");
		CommonUtils.GetLocalPlayer().GetComponent<BasicBehaviour>().enabled = false;
		CommonUtils.GetLocalPlayer().GetComponent<MoveBehaviour>().enabled = false;
		CommonUtils.GetDanceBehaviour().enabled = true;
	}

	public void enterNormalMode()
	{
		Debug.Log("enterNormalMode");
		CommonUtils.GetLocalPlayer().GetComponent<BasicBehaviour>().enabled = true;
		CommonUtils.GetLocalPlayer().GetComponent<MoveBehaviour>().enabled = true;
		CommonUtils.GetDanceBehaviour().enabled = false;
	}

	public void enterIdleMode()
	{
		Debug.Log("enterIdleMode");
		CommonUtils.GetLocalPlayer().GetComponent<BasicBehaviour>().enabled = false;
		CommonUtils.GetLocalPlayer().GetComponent<MoveBehaviour>().enabled = false;
		CommonUtils.GetDanceBehaviour().enabled = false;
	}
}
