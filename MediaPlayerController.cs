using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MediaPlayerController : MonoBehaviour
{
	public Image playBtnObj;

	public Sprite playBtnSprite;

	public Sprite pauseBtnSprite;

	public AudioSource songPlayer;

	public GameObject songListObj;

	public Transform rowParent;

	public Slider timeSlider;

	public TextMeshProUGUI songNameText;

	public TextMeshProUGUI currentTimeText;

	public TextMeshProUGUI songLengthText;

	[HideInInspector]
	public bool isPlaying = true;

	private int songIndex;

	private List<Song> songList;

	private float currentLength;

	private GameObject selectedRow;

	private AudioClip currentSong;

	private void Awake()
	{
	}

	public void init()
	{
		songList = new List<Song>(SongFactory.getSongList());
		songList.RemoveAt(0);
		songIndex = Random.Range(0, songList.Count);
	}

	private void Update()
	{
		startTimeSlider();
	}

	public void onPlayBtnClicked()
	{
		if (isPlaying)
		{
			songPlayer.Pause();
			playBtnObj.sprite = playBtnSprite;
		}
		else
		{
			songPlayer.UnPause();
			playBtnObj.sprite = pauseBtnSprite;
		}
		isPlaying = !isPlaying;
	}

	public void onNextBtnClicked()
	{
		StopSong();
		if (songIndex != songList.Count - 1)
		{
			songIndex++;
		}
		else
		{
			songIndex = 0;
		}
		PlaySong(songList[songIndex]);
	}

	public void onPreviousBtnClicked()
	{
		StopSong();
		if (songIndex > 0)
		{
			songIndex--;
		}
		else
		{
			songIndex = songList.Count - 1;
		}
		PlaySong(songList[songIndex]);
	}

	public void onListBtnClicked()
	{
		if (songListObj.activeSelf)
		{
			onListSongCloseBtnClicked();
			return;
		}
		foreach (Transform item in rowParent)
		{
			Object.Destroy(item.gameObject);
		}
		int num = 1;
		foreach (Song song in songList)
		{
			GameObject itemGO = Object.Instantiate(Resources.Load("Prefabs/MediaPlayer/Row") as GameObject);
			itemGO.transform.SetParent(rowParent);
			itemGO.transform.localScale = new Vector3(1f, 1f, 1f);
			string text = num + ". " + song.name + " - " + song.artist;
			itemGO.transform.Find("Text").GetComponent<Text>().text = text;
			itemGO.name = num.ToString();
			ButtonEventHandler btnEvtHandler = itemGO.GetComponent<ButtonEventHandler>();
			if (songIndex == num - 1)
			{
				itemGO.GetComponent<Image>().sprite = btnEvtHandler.selectedSprite;
				selectedRow = itemGO;
			}
			else
			{
				itemGO.GetComponent<Image>().sprite = btnEvtHandler.normalSprite;
			}
			Button component = itemGO.GetComponent<Button>();
			component.onClick.RemoveAllListeners();
			component.onClick.AddListener(delegate
			{
				PlaySong(song);
				selectedRow.GetComponent<Image>().sprite = btnEvtHandler.normalSprite;
				songIndex = int.Parse(itemGO.name) - 1;
				selectedRow = itemGO;
				selectedRow.GetComponent<Image>().sprite = btnEvtHandler.selectedSprite;
			});
			num++;
		}
		songListObj.SetActive(value: true);
		GameObject.Find(Global.localPlayerId + "-Camera").GetComponent<ThirdPersonOrbitCamBasic>().enabled = false;
	}

	public void onListSongCloseBtnClicked()
	{
		songListObj.SetActive(value: false);
		GameObject.Find(Global.localPlayerId + "-Camera").GetComponent<ThirdPersonOrbitCamBasic>().enabled = true;
	}

	public void PlaySong()
	{
		isPlaying = true;
		songPlayer.clip = currentSong;
		songPlayer.Play();
		playBtnObj.sprite = pauseBtnSprite;
	}

	public void StopSong()
	{
		songPlayer.Stop();
		isPlaying = false;
		currentLength = 0f;
		currentTimeText.SetText(CommonUtils.formatTime(currentLength));
		timeSlider.value = Mathf.Lerp(0f, 1f, 0f);
		playBtnObj.sprite = playBtnSprite;
	}

	private void PlaySong(Song song)
	{
		isPlaying = true;
		currentLength = 0f;
		Debug.Log("Playing song: " + song.fileName);
		currentSong = Resources.Load<AudioClip>(song.fileName);
		Debug.Log("Playing song: " + currentSong.name);
		songPlayer.clip = currentSong;
		songPlayer.Play();
		songNameText.SetText(song.name + " - " + song.artist);
		playBtnObj.sprite = pauseBtnSprite;
	}

	private void startTimeSlider()
	{
		if (isPlaying && !(songPlayer.clip == null))
		{
			currentLength += Time.deltaTime;
			float t = currentLength / songPlayer.clip.length;
			timeSlider.value = Mathf.Lerp(0f, 1f, t);
			currentTimeText.SetText(CommonUtils.formatTime(currentLength));
			songLengthText.SetText(CommonUtils.formatTime(songPlayer.clip.length));
			if (currentLength > songPlayer.clip.length)
			{
				onNextBtnClicked();
			}
		}
	}

	public void show(bool isShow)
	{
		if (isShow)
		{
			PlaySong();
		}
		else
		{
			StopSong();
		}
		base.gameObject.SetActive(isShow);
	}
}
