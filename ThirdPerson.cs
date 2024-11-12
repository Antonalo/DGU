using UnityEngine;

public class ThirdPerson : MonoBehaviour
{
	public Transform player;

	public Vector3 pivotOffset = new Vector3(0f, 1.7f, 0f);

	public Vector3 camOffset = new Vector3(0f, 0f, -3f);

	public float smooth = 10f;

	public float horizontalAimingSpeed = 6f;

	public float verticalAimingSpeed = 6f;

	public float maxVerticalAngle = 30f;

	public float minVerticalAngle = -60f;

	public string XAxis = "Analog X";

	public string YAxis = "Analog Y";

	private float angleH;

	private float angleV;

	private Transform cam;

	private Vector3 smoothPivotOffset;

	private Vector3 smoothCamOffset;

	private Vector3 targetPivotOffset;

	private Vector3 targetCamOffset;

	private float defaultFOV;

	private float targetFOV;

	private float targetMaxVerticalAngle;

	private bool isCustomOffset;

	public float GetH => angleH;

	private void Awake()
	{
		cam = base.transform;
		cam.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
		cam.rotation = Quaternion.identity;
		smoothPivotOffset = pivotOffset;
		smoothCamOffset = camOffset;
		defaultFOV = cam.GetComponent<Camera>().fieldOfView;
		angleH = player.eulerAngles.y;
		ResetTargetOffsets();
		ResetFOV();
		ResetMaxVerticalAngle();
		if (camOffset.y > 0f)
		{
			Debug.LogWarning("Vertical Cam Offset (Y) will be ignored during collisions!\nIt is recommended to set all vertical offset in Pivot Offset.");
		}
	}

	private void Update()
	{
		angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * horizontalAimingSpeed;
		angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f) * verticalAimingSpeed;
		angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);
		Quaternion quaternion = Quaternion.Euler(0f, angleH, 0f);
		Quaternion quaternion2 = Quaternion.Euler(0f - angleV, angleH, 0f);
		cam.rotation = quaternion2;
		cam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(cam.GetComponent<Camera>().fieldOfView, targetFOV, Time.deltaTime);
		Vector3 vector = player.position + quaternion * targetPivotOffset;
		Vector3 zero = targetCamOffset;
		while (zero.magnitude >= 0.2f && !DoubleViewingPosCheck(vector + quaternion2 * zero))
		{
			zero -= zero.normalized * 0.2f;
		}
		if (zero.magnitude < 0.2f)
		{
			zero = Vector3.zero;
		}
		bool flag = isCustomOffset && zero.sqrMagnitude < targetCamOffset.sqrMagnitude;
		smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, flag ? pivotOffset : targetPivotOffset, smooth * Time.deltaTime);
		smoothCamOffset = Vector3.Lerp(smoothCamOffset, flag ? Vector3.zero : zero, smooth * Time.deltaTime);
		cam.position = player.position + quaternion * smoothPivotOffset + quaternion2 * smoothCamOffset;
	}

	public void SetTargetOffsets(Vector3 newPivotOffset, Vector3 newCamOffset)
	{
		targetPivotOffset = newPivotOffset;
		targetCamOffset = newCamOffset;
		isCustomOffset = true;
	}

	public void ResetTargetOffsets()
	{
		targetPivotOffset = pivotOffset;
		targetCamOffset = camOffset;
		isCustomOffset = false;
	}

	public void ResetYCamOffset()
	{
		targetCamOffset.y = camOffset.y;
	}

	public void SetYCamOffset(float y)
	{
		targetCamOffset.y = y;
	}

	public void SetXCamOffset(float x)
	{
		targetCamOffset.x = x;
	}

	public void SetFOV(float customFOV)
	{
		targetFOV = customFOV;
	}

	public void ResetFOV()
	{
		targetFOV = defaultFOV;
	}

	public void SetMaxVerticalAngle(float angle)
	{
		targetMaxVerticalAngle = angle;
	}

	public void ResetMaxVerticalAngle()
	{
		targetMaxVerticalAngle = maxVerticalAngle;
	}

	private bool DoubleViewingPosCheck(Vector3 checkPos)
	{
		if (ViewingPosCheck(checkPos))
		{
			return ReverseViewingPosCheck(checkPos);
		}
		return false;
	}

	private bool ViewingPosCheck(Vector3 checkPos)
	{
		Vector3 direction = player.position + pivotOffset - checkPos;
		if (Physics.SphereCast(checkPos, 0.2f, direction, out var hitInfo, direction.magnitude) && hitInfo.transform != player && !hitInfo.transform.GetComponent<Collider>().isTrigger)
		{
			return false;
		}
		return true;
	}

	private bool ReverseViewingPosCheck(Vector3 checkPos)
	{
		Vector3 vector = player.position + pivotOffset;
		Vector3 direction = checkPos - vector;
		if (Physics.SphereCast(vector, 0.2f, direction, out var hitInfo, direction.magnitude) && hitInfo.transform != player && hitInfo.transform != base.transform && !hitInfo.transform.GetComponent<Collider>().isTrigger)
		{
			return false;
		}
		return true;
	}

	public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
	{
		return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
	}
}
