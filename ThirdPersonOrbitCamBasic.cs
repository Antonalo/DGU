using UnityEngine;

public class ThirdPersonOrbitCamBasic : MonoBehaviour
{
	public Transform player;

	public Vector3 pivotOffset = new Vector3(0f, 1f, 0f);

	public Vector3 camOffset = new Vector3(0f, 1f, -6f);

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

	private Vector3 relCameraPos;

	private float relCameraPosMag;

	private Vector3 smoothPivotOffset;

	private Vector3 smoothCamOffset;

	private Vector3 targetPivotOffset;

	private Vector3 targetCamOffset;

	private float defaultFOV;

	private float targetFOV;

	private float targetMaxVerticalAngle;

	private float recoilAngle;

	private float minZoom = 20f;

	private float maxZoom = 80f;

	private bool isDancing;

	private bool isInInventory;

	private bool isSprinting;

	[HideInInspector]
	public Vector3 playerPos;

	private Quaternion camYRotation;

	private Quaternion aimRotation;

	private float povBeforeSprint = -1f;

	public CPC_CameraPath cameraPath;

	public float GetH => angleH;

	private void Start()
	{
		if (!(player == null))
		{
			setPlayer();
		}
	}

	public void setPlayer()
	{
		cam = base.transform;
		cam.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
		cam.rotation = Quaternion.identity;
		relCameraPos = base.transform.position - player.position;
		relCameraPosMag = relCameraPos.magnitude - 0.5f;
		smoothPivotOffset = pivotOffset;
		smoothCamOffset = camOffset;
		defaultFOV = cam.GetComponent<Camera>().fieldOfView;
		angleH = player.eulerAngles.y;
		if (cameraPath != null)
		{
			cameraPath.lookAtTarget = true;
			cameraPath.target = player;
		}
		ResetTargetOffsets();
		ResetFOV();
		ResetMaxVerticalAngle();
	}

	public void setPOVBeforeSprint(float povBeforeSprint)
	{
		this.povBeforeSprint = povBeforeSprint;
	}

	private void Update()
	{
		if (player == null || isInInventory)
		{
			return;
		}
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * horizontalAimingSpeed;
			angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f) * verticalAimingSpeed;
		}
		angleV = Mathf.LerpAngle(angleV, angleV + recoilAngle, 10f * Time.deltaTime);
		camYRotation = Quaternion.Euler(0f, angleH, 0f);
		aimRotation = Quaternion.Euler(0f - angleV, angleH, 0f);
		cam.rotation = aimRotation;
		defaultFOV = cam.GetComponent<Camera>().fieldOfView;
		if (isSprinting)
		{
			cam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(defaultFOV, targetFOV, Time.deltaTime);
		}
		else if (povBeforeSprint != -1f && (!(povBeforeSprint <= defaultFOV + 1f) || !(povBeforeSprint >= defaultFOV - 1f)))
		{
			cam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(defaultFOV, povBeforeSprint, Time.deltaTime);
		}
		else
		{
			defaultFOV += (0f - Input.GetAxis("Mouse ScrollWheel")) * 10f;
			defaultFOV = Mathf.Clamp(defaultFOV, minZoom, maxZoom);
			povBeforeSprint = -1f;
			cam.GetComponent<Camera>().fieldOfView = defaultFOV;
		}
		Vector3 vector = player.position + camYRotation * targetPivotOffset;
		Vector3 vector2 = targetCamOffset;
		for (float num = targetCamOffset.z; num <= 0f; num += 0.5f)
		{
			vector2.z = num;
			if (DoubleViewingPosCheck(vector + aimRotation * vector2, Mathf.Abs(num)) || num == 0f)
			{
				break;
			}
		}
		smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, smooth * Time.deltaTime);
		smoothCamOffset = Vector3.Lerp(smoothCamOffset, vector2, smooth * Time.deltaTime);
		if (!isDancing)
		{
			cam.position = player.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;
		}
		else
		{
			cam.position = playerPos + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;
		}
	}

	public void setIsInventory(bool isInInventory)
	{
		this.isInInventory = isInInventory;
	}

	public void setIsDancing(bool isDancing)
	{
		this.isDancing = isDancing;
		if (isDancing)
		{
			cameraPath.points[0].position = new Vector3(player.position.x, player.position.y + 7f, player.position.z - 10f);
			cameraPath.points[1].position = new Vector3(player.position.x, player.position.y + 2.2f, player.position.z + 6f);
			cameraPath.points[0].rotation.eulerAngles = new Vector3(player.rotation.eulerAngles.x + 8f, 0.2f, player.rotation.eulerAngles.z + 12f);
			cameraPath.points[1].rotation.eulerAngles = new Vector3(player.rotation.eulerAngles.x + 9f, 180f, player.rotation.eulerAngles.z + 12f);
			cameraPath.PlayPath(4.5f);
			angleH = player.rotation.eulerAngles.y - 180f;
		}
	}

	public void BounceVertical(float degrees)
	{
		recoilAngle = degrees;
	}

	public void SetTargetOffsets(Vector3 newPivotOffset, Vector3 newCamOffset)
	{
		targetPivotOffset = newPivotOffset;
		targetCamOffset = newCamOffset;
	}

	public void ResetTargetOffsets()
	{
		targetPivotOffset = pivotOffset;
		targetCamOffset = camOffset;
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
		isSprinting = true;
	}

	public void ResetFOV()
	{
		targetFOV = defaultFOV;
		isSprinting = false;
	}

	public void SetMaxVerticalAngle(float angle)
	{
		targetMaxVerticalAngle = angle;
	}

	public void ResetMaxVerticalAngle()
	{
		targetMaxVerticalAngle = maxVerticalAngle;
	}

	private bool DoubleViewingPosCheck(Vector3 checkPos, float offset)
	{
		float deltaPlayerHeight = player.lossyScale.y * 0.5f;
		if (ViewingPosCheck(checkPos, deltaPlayerHeight))
		{
			return ReverseViewingPosCheck(checkPos, deltaPlayerHeight, offset);
		}
		return false;
	}

	private bool ViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight)
	{
		if (Physics.Raycast(checkPos, player.position + Vector3.up * deltaPlayerHeight - checkPos, out var hitInfo, relCameraPosMag) && hitInfo.transform != player && !hitInfo.transform.GetComponent<Collider>().isTrigger)
		{
			return false;
		}
		return true;
	}

	private bool ReverseViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight, float maxDistance)
	{
		if (Physics.Raycast(player.position + Vector3.up * deltaPlayerHeight, checkPos - player.position, out var hitInfo, maxDistance) && hitInfo.transform != player && hitInfo.transform != base.transform && !hitInfo.transform.GetComponent<Collider>().isTrigger)
		{
			return false;
		}
		return true;
	}

	public float getCurrentPivotMagnitude(Vector3 finalPivotOffset)
	{
		return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
	}
}
