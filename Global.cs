using System.Collections.Generic;
using UnityEngine;

public static class Global
{
	public static User user;

	public static string localPlayerId;

	public static List<Item> inUsedItems;

	public static bool isPerfectEffectOn = true;

	public static bool isServer = false;

	public static string token = "";

	public static int draggableLayer;

	public static int characterOnUILayer;

	public static int characterLayer;

	public static int weddingStyle = 0;

	public static bool isSceneloaded = false;

	public static int LUKA_ROOM_ID = 101;

	public static int DEFAULT_SCREEN_WIDTH = 1280;

	public static int DEFAULT_SCREEN_HEIGHT = 720;

	public static bool MASTER_CLIENT_RECONNECT = false;

	public static string effectWingsPath = "ORG-hips/ORG-spine/WINGS";

	public static string wingsPath = "CHARACTER_MODE/WINGS";

	public static string effectPath = "EFFECT";

	public static string BATTLE_CAMERA_DEFAULT_NAME = "Battle_3rdPersonCamera";

	public static string WEDDING_SCENE = "WeddingScene";

	public static string MASTER_SCENE = "MasterScreen";

	public static string PERSISTENT_OBJECTS_SCENE = "Persistent_Objects";

	public static string LOADING_SCENE = "LoadingScreen";

	public static string AUTHORIZATION_HEADER = "X-Authorization";

	public static string WEBSITE_URL = "https://dancingangel.tk";

	public static string BASE_URL = "https://play.dancingangel.live/web-api";

	public static string SERVER_IP = "127.0.0.1";

	public static string SET_STATUS_URL;

	public static string LOGIN_URL;

	public static string REGISTER_URL;

	public static string STATISTIC_URL;

	public static string PK_URL;

	public static string CHARACTER_URL;

	public static string ITEM_URL;

	public static string USER_URL;

	public static string BOSS_URL;

	public static string CLUB_URL;

	public static string FRIEND_URL;

	public static string MAILBOX_URL;

	public static string NEWS_URL;

	public static string QUEST_URL;

	public static string PAYMENT_URL;

	public static string COUPLE_URL;

	public static string ACHIEVEMENT_URL;

	public static string EMAIL_URL;

	public static List<string> AutoJoinChannels = new List<string> { "ALL", "SYSTEM", "CROSS_GAMEMASTER" };

	public static readonly KeyCode LEFT = KeyCode.LeftArrow;

	public static readonly KeyCode UP = KeyCode.UpArrow;

	public static readonly KeyCode DOWN = KeyCode.DownArrow;

	public static readonly KeyCode RIGHT = KeyCode.RightArrow;

	public static readonly KeyCode MINUS = KeyCode.Minus;

	public static readonly KeyCode EQUALS = KeyCode.Equals;

	public static readonly KeyCode SPACE = KeyCode.Space;

	public static readonly float DEL_SCORE = 30f;

	public static readonly float PERFECT_SCORE = 500f;

	public static readonly float GREAT_SCORE = 400f;

	public static readonly float GOOD_SCORE = 300f;

	public static string BREAK_TIME = "BreakTime";

	public static string NEW_ROUND = "NewRound";

	public static KeyCode randomKeyCode()
	{
		return Random.Range(0, 4) switch
		{
			0 => UP, 
			1 => DOWN, 
			2 => LEFT, 
			_ => RIGHT, 
		};
	}

	public static void initConfig(AppConfig appConfig)
	{
		draggableLayer = LayerMask.NameToLayer("Draggable");
		characterOnUILayer = LayerMask.NameToLayer("Character_On_UI");
		characterLayer = LayerMask.NameToLayer("Character");
		WEBSITE_URL = appConfig.websiteUrl;
		SERVER_IP = appConfig.serverIP;
		BASE_URL = appConfig.baseUrl;
		SET_STATUS_URL = BASE_URL + "/status";
		LOGIN_URL = BASE_URL + "/login";
		REGISTER_URL = BASE_URL + "/signup";
		CHARACTER_URL = BASE_URL + "/character";
		ITEM_URL = BASE_URL + "/items";
		BOSS_URL = BASE_URL + "/boss";
		STATISTIC_URL = BASE_URL + "/statistic";
		PK_URL = BASE_URL + "/pk";
		CLUB_URL = BASE_URL + "/club";
		FRIEND_URL = BASE_URL + "/friend";
		MAILBOX_URL = BASE_URL + "/mailbox";
		NEWS_URL = BASE_URL + "/news";
		QUEST_URL = BASE_URL + "/quest";
		PAYMENT_URL = BASE_URL + "/payment";
		COUPLE_URL = BASE_URL + "/couple";
		USER_URL = BASE_URL + "/user/";
		ACHIEVEMENT_URL = BASE_URL + "/achievement";
		EMAIL_URL = BASE_URL + "/email";
	}

	public static void initTestConfig()
	{
		initConfig(new AppConfig
		{
			baseUrl = "http://34.67.125.188"
		});
	}
}
