using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
	[HideInInspector]
	public float songLength;

	[HideInInspector]
	public float currentLength;

	[HideInInspector]
	public float normalizedTime;

	public Transform screen;

	private TextMeshProUGUI songProgress;

	private Slider timeSlider;

	private Transform timeProgress;

	private TextMeshProUGUI songLengthText;

	private void Start()
	{
		songProgress = screen.Find("DanceObj/Time/time").GetComponent<TextMeshProUGUI>();
		timeSlider = screen.Find("DanceObj/Time/TimeSlider").GetComponent<Slider>();
		timeProgress = screen.Find("DanceObj/Time").transform;
		songLengthText = screen.Find("DanceObj/Time/songLength").GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		if (!isTimeout())
		{
			startTimeSlider();
		}
		else
		{
			currentLength = 0f;
		}
	}

	public void startGame(float length)
	{
		songLength = length;
		currentLength = 0f;
		songLengthText.SetText(CommonUtils.formatTime(songLength));
	}

	public bool isSongEnded()
	{
		return songLength - currentLength < 10f;
	}

	public bool isTimeout()
	{
		return currentLength >= songLength;
	}

	private void startTimeSlider()
	{
		currentLength += Time.deltaTime;
		normalizedTime = currentLength / songLength;
		timeSlider.value = Mathf.Lerp(0f, 1f, normalizedTime);
		songProgress.SetText(CommonUtils.formatTime(currentLength));
	}
}
