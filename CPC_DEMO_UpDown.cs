using UnityEngine;

public class CPC_DEMO_UpDown : MonoBehaviour
{
	private Vector3 startPos;

	public float height;

	public float speed;

	private void Start()
	{
		startPos = base.transform.position;
	}

	private void Update()
	{
		base.transform.position = startPos + Vector3.up * height * Mathf.Sin(Time.time * speed);
	}
}
