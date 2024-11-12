using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
	[Header("LOGIN")]
	public GameObject LoginPopup;

	public TMP_InputField username;

	public TMP_InputField password;

	[Header("REGISTRATION")]
	public GameObject RegisterPopup;

	public TMP_InputField email;

	public TMP_InputField nickname;

	public TMP_InputField newPassword;

	public TMP_InputField retypePassword;

	[Header("OTHERS")]
	public GameObject PopupMsg;

	public GameObject LoadingPopup;

	public GameObject versionPopup;

	public TextMeshProUGUI versionText;

	[HideInInspector]
	public AppConfig appConfig;

	private Dictionary<string, string> urls;

	private int currentPage;

	private int pageSize = 3;

	private int totalPages;

	public void Start()
	{
		StartCoroutine(loadConfigJson());
	}

	public IEnumerator loadConfigJson()
	{
		LoadingPopup.SetActive(value: true);
		string url = $"https://play.dancingangel.live/appconfig.json";
		UnityWebRequest request = CommonUtils.prepareRequest(url, "GET");
		yield return request.SendWebRequest();
		string text = request.downloadHandler.text;
		appConfig = JsonUtility.FromJson<AppConfig>(text);
		Global.initConfig(appConfig);
		if (appConfig.isServerUnderMaintenance)
		{
			openPopupMsg("Server ba\u0309o tri\u0300, vui lo\u0300ng thư\u0309 la\u0323i sau");
		}
		else
		{
			addListenerToInputText();
		}
		LoadingPopup.SetActive(value: false);
	}

	private void addListenerToInputText()
	{
		username.onEndEdit.AddListener(delegate
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				doLogin();
			}
		});
		password.onEndEdit.AddListener(delegate
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				doLogin();
			}
		});
		EventSystem.current.SetSelectedGameObject(username.gameObject);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (username.isFocused)
			{
				EventSystem.current.SetSelectedGameObject(password.gameObject);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(username.gameObject);
			}
		}
	}

	public void doLogin(string username, string pass)
	{
		string bodyJsonString = new User
		{
			username = username,
			password = pass
		}.toString();
		StartCoroutine(sendLoginRequest(bodyJsonString));
	}

	public void doLogin()
	{
		string text = "";
		if (username.text.Equals(""))
		{
			text = LeanLocalization.GetTranslationText("EmailRequired");
			openPopupMsg(text);
		}
		else if (password.text.Equals(""))
		{
			text = LeanLocalization.GetTranslationText("PasswordRequired");
			openPopupMsg(text);
		}
		else
		{
			LoadingPopup.SetActive(value: true);
			doLogin(username.text, password.text);
		}
	}

	private IEnumerator sendLoginRequest(string bodyJsonString)
	{
		UnityWebRequest request = new UnityWebRequest(Global.LOGIN_URL, "POST");
		byte[] bytes = new UTF8Encoding().GetBytes(bodyJsonString);
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		LoadingPopup.SetActive(value: false);
		string text = request.downloadHandler.text;
		Debug.Log("responseData:" + text);
		Response<string> response = JsonUtility.FromJson<Response<string>>(text);
		if (response.status.code == -10)
		{
			string translationText = LeanLocalization.GetTranslationText("IncorrectEmailOrPasword");
			openPopupMsg(translationText);
			yield return null;
		}
		else if (response.status.code != 0)
		{
			Debug.LogError("Login failed : " + response.status.message);
			openPopupMsg(response.status.message);
			yield return null;
		}
		else
		{
			Global.token = response.data;
			StartCoroutine(getUserInfo());
		}
	}

	private IEnumerator getUserInfo()
	{
		UnityWebRequest request = new UnityWebRequest(Global.USER_URL, "GET");
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		request.SetRequestHeader(Global.AUTHORIZATION_HEADER, Global.token);
		request.timeout = 4;
		yield return request.SendWebRequest();
		try
		{
			string text = request.downloadHandler.text;
			Response<User> response = JsonUtility.FromJson<Response<User>>(text);
			Debug.Log("response: " + text);
			if (response.status.code != 0)
			{
				Debug.LogError("Get User Information Failed: " + response.status.code);
				openPopupMsg("Get User Information Failed (Error Code: " + response.status.code + ")");
				yield break;
			}
			Global.user = response.data;
			Debug.Log("Global.user: " + Global.user.role);
			Global.localPlayerId = response.data.id;
			SceneManager.LoadScene("Lobby");
		}
		catch (Exception ex)
		{
			Debug.LogError("Server Down : " + ex.Message);
			openPopupMsg("Server is under maintaince. Please try again later");
		}
	}

	public void onCreateAccountclicked()
	{
		string text = "";
		if (!Regex.IsMatch(email.text, "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.IgnoreCase))
		{
			text = LeanLocalization.GetTranslationText("IncorrectEmail");
			openPopupMsg(text);
			return;
		}
		if (nickname.text.Length < 4)
		{
			text = LeanLocalization.GetTranslationText("IncorrectNickname");
			openPopupMsg(text);
			return;
		}
		if (newPassword.text.Length < 6)
		{
			text = LeanLocalization.GetTranslationText("IncorrectPassword");
			openPopupMsg(text);
			return;
		}
		if (!newPassword.text.Equals(retypePassword.text))
		{
			text = LeanLocalization.GetTranslationText("IncorrectRetypePassword");
			openPopupMsg(text);
			return;
		}
		LoadingPopup.SetActive(value: true);
		User user = new User();
		user.username = email.text;
		user.name = nickname.text;
		user.email = email.text;
		user.password = newPassword.text;
		user.gender = "male";
		StartCoroutine(sendCreateAccountRequest(user.toString()));
	}

	private IEnumerator sendCreateAccountRequest(string bodyJsonString)
	{
		UnityWebRequest request = new UnityWebRequest(Global.REGISTER_URL, "POST");
		byte[] bytes = new UTF8Encoding().GetBytes(bodyJsonString);
		request.uploadHandler = new UploadHandlerRaw(bytes);
		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");
		yield return request.SendWebRequest();
		Response<string> response = JsonUtility.FromJson<Response<string>>(request.downloadHandler.text);
		LoadingPopup.SetActive(value: false);
		if (response.status.code == -13)
		{
			Debug.LogError("Username is existed");
			openPopupMsg(LeanLocalization.GetTranslationText("ExistedEmail"));
			yield return null;
			yield break;
		}
		if (response.status.code != 0)
		{
			Debug.LogError("Registration failed : " + response.status.message);
			openPopupMsg(response.status.message);
			yield return null;
			yield break;
		}
		Debug.Log("Registration success");
		openPopupMsg(LeanLocalization.GetTranslationText("AccountCreated"));
		nickname.text = "";
		email.text = "";
		newPassword.text = "";
		retypePassword.text = "";
		toggleRegistrationPopup(isOn: false);
	}

	public void togglePopupMsg(bool isOn)
	{
		PopupMsg.SetActive(isOn);
	}

	public void closeVersionPopup()
	{
		versionPopup.SetActive(value: false);
		Application.Quit();
	}

	public void openPopupMsg(string msg)
	{
		togglePopupMsg(isOn: true);
		PopupMsg.transform.Find("Panel/Msg").GetComponent<TextMeshProUGUI>().text = msg;
	}

	public void toggleRegistrationPopup(bool isOn)
	{
		RegisterPopup.SetActive(isOn);
		LoginPopup.SetActive(!isOn);
	}
}
