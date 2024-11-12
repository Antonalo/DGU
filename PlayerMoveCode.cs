using UnityEngine;

public class PlayerMoveCode : GenericBehaviour1
{
	public float walkSpeed = 0.15f;

	public float runSpeed = 1f;

	public float sprintSpeed = 2f;

	public float speedDampTime = 0.1f;

	public float jumpHeight = 1.5f;

	public float jumpIntertialForce = 10f;

	private float speed;

	private float speedSeeker;

	private int jumpBool;

	private int groundedBool;

	private bool jump;

	private bool isColliding;

	public bool isLock;

	private void Start()
	{
		jumpBool = Animator.StringToHash("Jump");
		groundedBool = Animator.StringToHash("Grounded");
		behaviourManager.GetAnim.SetBool(groundedBool, value: true);
		behaviourManager.SubscribeBehaviour(this);
		behaviourManager.RegisterDefaultBehaviour(behaviourCode);
		speedSeeker = runSpeed;
		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		if (!jump && Input.GetKeyDown(KeyCode.Space) && behaviourManager.IsCurrentBehaviour(behaviourCode) && !behaviourManager.IsOverriding())
		{
			jump = true;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			isLock = !isLock;
			if (isLock)
			{
				Time.timeScale = 0f;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				Time.timeScale = 1f;
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}
	}

	public override void LocalFixedUpdate()
	{
		MovementManagement(behaviourManager.GetH, behaviourManager.GetV);
		JumpManagement();
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
				RemoveVerticalVelocity();
				float f = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
				f = Mathf.Sqrt(f);
				behaviourManager.GetRigidBody.AddForce(Vector3.up * f, ForceMode.VelocityChange);
			}
		}
		else if (behaviourManager.GetAnim.GetBool(jumpBool))
		{
			if (!behaviourManager.IsGrounded() && !isColliding && behaviourManager.GetTempLockStatus())
			{
				behaviourManager.GetRigidBody.AddForce(base.transform.forward * jumpIntertialForce * Physics.gravity.magnitude * sprintSpeed, ForceMode.Acceleration);
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
		else if (!behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.GetRigidBody.velocity.y > 0f)
		{
			RemoveVerticalVelocity();
		}
		Rotating(horizontal, vertical);
		Vector2 vector = new Vector2(horizontal, vertical);
		speed = Vector2.ClampMagnitude(vector, 1f).magnitude;
		speedSeeker += Input.GetAxis("Mouse ScrollWheel");
		speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
		speed *= speedSeeker;
		if (behaviourManager.IsSprinting())
		{
			speed = sprintSpeed;
		}
		behaviourManager.GetAnim.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);
	}

	private void RemoveVerticalVelocity()
	{
		Vector3 velocity = behaviourManager.GetRigidBody.velocity;
		velocity.y = 0f;
		behaviourManager.GetRigidBody.velocity = velocity;
	}

	private Vector3 Rotating(float horizontal, float vertical)
	{
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
		if (behaviourManager.IsCurrentBehaviour(GetBehaviourCode()) && collision.GetContact(0).normal.y <= 0.1f)
		{
			GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
			GetComponent<CapsuleCollider>().material.staticFriction = 0f;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		isColliding = false;
		GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
		GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;
	}
}
