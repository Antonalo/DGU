using Photon.Pun;
using UnityEngine;

public class MoveBehaviour : GenericBehaviour
{
	public float walkSpeed = 0.15f;

	public float runSpeed = 1f;

	public float sprintSpeed = 2f;

	public float speedDampTime = 0.1f;

	public string jumpButton = "Jump";

	public float jumpHeight = 1.5f;

	public bool isSitting;

	private float speed;

	private float speedSeeker;

	private int jumpBool;

	private int groundedBool;

	private bool jump;

	private bool isColliding;

	private GameObject sittingObj;

	private Transform sitPoint;

	private void Start()
	{
		jumpBool = Animator.StringToHash("Jump");
		groundedBool = Animator.StringToHash("Grounded");
		behaviourManager.GetAnim.SetBool(groundedBool, value: true);
		behaviourManager.SubscribeBehaviour(this);
		behaviourManager.RegisterDefaultBehaviour(behaviourCode);
		speedSeeker = runSpeed;
	}

	private void Update()
	{
		if (GetComponent<PhotonView>().IsMine && !jump && Input.GetButtonDown(jumpButton) && behaviourManager.IsCurrentBehaviour(behaviourCode) && !behaviourManager.IsOverriding())
		{
			if (isSitting)
			{
				standup();
			}
			else
			{
				jump = true;
			}
		}
	}

	private void standup()
	{
		if (!GetComponent<MainController>().isUserTyping)
		{
			CommonUtils.GetAnimator().SetTrigger("StandUp");
			sittingObj.GetComponent<Collider>().enabled = true;
			base.transform.position = sitPoint.position;
			base.transform.eulerAngles = new Vector3(0f, sitPoint.eulerAngles.y, 0f);
			isSitting = false;
		}
	}

	public override void LocalFixedUpdate()
	{
		if (GetComponent<PhotonView>().IsMine && !isSitting)
		{
			MovementManagement(behaviourManager.GetH, behaviourManager.GetV);
			JumpManagement();
		}
	}

	public void sitDown(GameObject sittingObj, Transform sitPoint)
	{
		if (!isSitting)
		{
			this.sittingObj = sittingObj;
			this.sitPoint = sitPoint;
			this.sittingObj.GetComponent<Collider>().enabled = false;
			isSitting = true;
			base.transform.position = sitPoint.position;
			base.transform.eulerAngles = new Vector3(0f, sitPoint.eulerAngles.y, 0f);
			animator.SetTrigger("Sitting");
			CommonUtils.addNormalMsg("Press SPACE button to stand up");
		}
	}

	private void JumpManagement()
	{
		if (jump && !behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.IsGrounded())
		{
			behaviourManager.LockTempBehaviour(behaviourCode);
			behaviourManager.GetAnim.SetBool(jumpBool, value: true);
			if ((double)behaviourManager.GetAnim.GetFloat(speedFloat) > 0.1)
			{
				GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
				GetComponent<CapsuleCollider>().material.staticFriction = 0f;
				float f = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
				f = Mathf.Sqrt(f);
				behaviourManager.GetRigidBody.AddForce(Vector3.up * f, ForceMode.VelocityChange);
			}
		}
		else if (behaviourManager.GetAnim.GetBool(jumpBool))
		{
			if (!behaviourManager.IsGrounded() && !isColliding)
			{
				behaviourManager.GetRigidBody.AddForce(base.transform.forward * behaviourManager.GetRigidBody.mass * sprintSpeed, ForceMode.Acceleration);
			}
			if (behaviourManager.GetRigidBody.velocity.y < 0f && behaviourManager.IsGrounded())
			{
				behaviourManager.GetAnim.SetBool(groundedBool, value: true);
				GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
				GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;
				jump = false;
				behaviourManager.GetAnim.SetBool(jumpBool, value: false);
				behaviourManager.UnlockTempBehaviour(behaviourCode);
			}
		}
	}

	private void MovementManagement(float horizontal, float vertical)
	{
		if (behaviourManager.IsGrounded())
		{
			behaviourManager.GetRigidBody.useGravity = true;
		}
		Rotating(horizontal, vertical);
		Vector2 vector = new Vector2(horizontal, vertical);
		speed = Vector2.ClampMagnitude(vector, 1f).magnitude;
		speedSeeker += Input.GetAxis("Mouse ScrollWheel");
		speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
		speed *= speedSeeker;
		if (behaviourManager.isSprinting())
		{
			speed = sprintSpeed;
		}
		behaviourManager.GetAnim.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);
	}

	private Vector3 Rotating(float horizontal, float vertical)
	{
		if (behaviourManager.playerCamera == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = behaviourManager.playerCamera.TransformDirection(Vector3.forward);
		vector.y = 0f;
		vector = vector.normalized;
		Vector3 vector2 = new Vector3(vector.z, 0f, 0f - vector.x);
		Vector3 vector3 = vector * vertical + vector2 * horizontal;
		if (behaviourManager.IsMoving() && vector3 != Vector3.zero)
		{
			Quaternion b = Quaternion.LookRotation(vector3);
			Quaternion rot = Quaternion.Slerp(behaviourManager.GetRigidBody.rotation, b, behaviourManager.turnSmoothing);
			behaviourManager.GetRigidBody.MoveRotation(rot);
			behaviourManager.SetLastDirection(vector3);
		}
		if (!((double)Mathf.Abs(horizontal) > 0.9) && !((double)Mathf.Abs(vertical) > 0.9))
		{
			behaviourManager.Repositioning();
		}
		return vector3;
	}

	private void OnCollisionStay(Collision collision)
	{
		isColliding = true;
	}

	private void OnCollisionExit(Collision collision)
	{
		isColliding = false;
	}
}
