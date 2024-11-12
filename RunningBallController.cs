using UnityEngine;

public class RunningBallController : MonoBehaviour
{
	public float bpm = 80f;

	public int level;

	private GameController gameController;

	private float standardWidth = 1280f;

	private float widthRatio = 1f;

	private void Start()
	{
		gameController = GameObject.Find("GameController").GetComponent<GameController>();
		widthRatio = (float)Screen.width / standardWidth;
	}

	private void Update()
	{
		widthRatio = (float)Screen.width / standardWidth;
		if (base.transform.localPosition.x < 310f)
		{
			base.transform.Translate(Vector3.right * getSpeed() * Time.deltaTime * widthRatio);
		}
		else
		{
			setBallToOriginalPos();
		}
	}

	private float getSpeed()
	{
		return bpm - 40f;
	}

	public void setBallToOriginalPos()
	{
		base.transform.localPosition = new Vector3(40f, base.transform.localPosition.y, base.transform.localPosition.x);
	}

	public void setBallWhenStartingDance()
	{
		base.transform.localPosition = new Vector3(120f, base.transform.localPosition.y, base.transform.localPosition.x);
	}
}
