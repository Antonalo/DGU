using UnityEngine;

public class FadeAlpha : MonoBehaviour
{
	private float fadePerSecond = 0.2f;

	private CanvasGroup canvasGroup;

	private void Start()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		InvokeRepeating("fade", 2f, 0.1f);
	}

	private void fade()
	{
		canvasGroup.alpha -= fadePerSecond;
		if (canvasGroup.alpha <= 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
