using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SongSelectionController : MonoBehaviour
{
	public Song selectedSong;

	public Sprite normalRow;

	public Sprite highlightedRow;

	public Transform songList;

	[Header("Windows")]
	public GameObject SongSelection;

	public GameObject ModeSelection;

	[Header("Tab")]
	public Sprite normalTabSpr;

	public Sprite selectedTabSpr;

	public GameObject defaultSelectedTab;

	[Header("ModeSelection")]
	public List<GameObject> modeObjs;

	public TextMeshProUGUI selectedModeText;

	private GameObject selectedItem;

	private int selectedTabIndex;

	private GameObject selectedTab;

	public int currentModeIndex;

	private bool isOnEnable;

	private string danceMode = "";

	protected virtual void Awake()
	{
		loadSongList();
		selectedSong = SongFactory.getSongList()[0];
		selectedTab = defaultSelectedTab;
	}

	private void Update()
	{
	}

	public void onTabSelect(int tabIndex)
	{
		setSelectedTab(tabIndex, isOnEnable: false);
	}

	private void setSelectedTab(int tabIndex, bool isOnEnable)
	{
		SongSelection.SetActive(tabIndex == 0);
		ModeSelection.SetActive(tabIndex == 1);
		selectedTabIndex = tabIndex;
		selectedTab.GetComponent<Image>().sprite = normalTabSpr;
		selectedTab = (isOnEnable ? defaultSelectedTab : EventSystem.current.currentSelectedGameObject);
		selectedTab.GetComponent<Image>().sprite = selectedTabSpr;
	}

	public void onModeSelected(int modeIndex)
	{
		modeObjs[currentModeIndex].transform.Find("Mode_Cursor").GetComponent<Image>().enabled = false;
		currentModeIndex = modeIndex;
		modeObjs[currentModeIndex].transform.Find("Mode_Cursor").GetComponent<Image>().enabled = true;
		switch (modeIndex)
		{
		case 0:
			danceMode = "AU - Easy";
			break;
		case 1:
			danceMode = "AU - Normal";
			break;
		case 2:
			danceMode = "AU - Hard";
			break;
		case 3:
			danceMode = "RHYTHM";
			break;
		default:
			danceMode = "TOUCH";
			CommonUtils.showPopupMsg("The song list is updated for TOUCH mode. Therefore, please select song again.");
			break;
		}
		selectedModeText.text = danceMode;
		foreach (Transform song in songList)
		{
			Object.Destroy(song.gameObject);
		}
		loadSongList();
	}

	public void showCanvas(bool show)
	{
		base.transform.Find("Canvas").gameObject.SetActive(show);
		if (show)
		{
			setSelectedTab(0, isOnEnable: true);
		}
		GameObject.Find(Global.localPlayerId + "-Camera").GetComponent<ThirdPersonOrbitCamBasic>().enabled = !show;
	}

	private void loadSongList()
	{
		foreach (Song song in SongFactory.getSongList())
		{
			if (!danceMode.Equals("TOUCH") || SongFactory.bubbleSongIds.Contains(song.id))
			{
				GameObject itemGO = Object.Instantiate(Resources.Load("Prefabs/SongSelection/song_item") as GameObject);
				itemGO.transform.SetParent(songList);
				itemGO.transform.localScale = new Vector3(1f, 1f, 1f);
				itemGO.transform.Find("Name").GetComponent<Text>().text = song.name;
				itemGO.transform.Find("Artist").GetComponent<Text>().text = song.artist;
				itemGO.transform.Find("BPM").GetComponent<Text>().text = song.bpm;
				itemGO.transform.Find("Level").GetComponent<Text>().text = song.level;
				itemGO.transform.Find("Duration").GetComponent<Text>().text = song.duration;
				Button component = itemGO.GetComponent<Button>();
				component.onClick.RemoveAllListeners();
				component.onClick.AddListener(delegate
				{
					selectSong(itemGO, song);
				});
				if (danceMode.Equals("TOUCH"))
				{
					selectSong(itemGO, song);
				}
			}
		}
	}

	private void selectSong(GameObject itemGO, Song song)
	{
		Debug.Log("Selected song: " + song.name);
		if (selectedItem != null)
		{
			selectedItem.GetComponent<Image>().sprite = normalRow;
		}
		selectedItem = itemGO;
		selectedItem.GetComponent<Image>().sprite = highlightedRow;
		selectedSong = song;
	}
}
