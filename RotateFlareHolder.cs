using UnityEngine;

public class RotateFlareHolder : MonoBehaviour
{
	private void Update()
	{
		base.transform.Rotate(Vector3.down * Time.deltaTime * 60f);
		base.transform.Rotate(Vector3.left * Time.deltaTime * 30f);
		base.transform.Rotate(Vector3.back * Time.deltaTime * 12f);
	}
}
