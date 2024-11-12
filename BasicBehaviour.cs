using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using VikingCrewTools.UI;

public class BasicBehaviour : MonoBehaviourPunCallbacks
{
	public Transform playerCamera;

	public float turnSmoothing = 0.06f;

	public float sprintFOV = 120f;

	public string sprintButton = "Sprint";

	private float h;

	private float v;

	private int currentBehaviour;

	private int defaultBehaviour;

	private int behaviourLocked;

	private Vector3 lastDirection;

	private Animator anim;

	private ThirdPersonOrbitCamBasic camScript;

	private bool sprint;

	private bool changedFOV;

	private int hFloat;

	private int vFloat;

	private List<GenericBehaviour> behaviours;

	private List<GenericBehaviour> overridingBehaviours;

	private Rigidbody rBody;

	private int groundedBool;

	private Vector3 colExtents;

	private MoveBehaviour moveBehaviour;

	private float povBeforeSprinting;

	public float GetH => h;

	public float GetV => v;

	public ThirdPersonOrbitCamBasic GetCamScript => camScript;

	public Rigidbody GetRigidBody => rBody;

	public Animator GetAnim => anim;

	public int GetDefaultBehaviour => defaultBehaviour;

	private void Start()
	{
		moveBehaviour = GetComponent<MoveBehaviour>();
		behaviours = new List<GenericBehaviour>();
		overridingBehaviours = new List<GenericBehaviour>();
		anim = GetComponent<Animator>();
		hFloat = Animator.StringToHash("H");
		vFloat = Animator.StringToHash("V");
		rBody = GetComponent<Rigidbody>();
		groundedBool = Animator.StringToHash("Grounded");
		colExtents = GetComponent<Collider>().bounds.extents;
	}

	public void SetupCamera(Transform playerCamera)
	{
		if (GetComponent<PhotonView>().IsMine)
		{
			this.playerCamera = playerCamera;
			camScript = playerCamera.GetComponent<ThirdPersonOrbitCamBasic>();
			GameObject.Find("Speechbubble Manager").GetComponent<SpeechBubbleManager>().Cam = playerCamera.GetComponent<Camera>();
		}
	}

	private bool disallowMoving()
	{
		if (GetComponent<PhotonView>().IsMine && !(playerCamera == null) && !CommonUtils.GetMainController().isUserTyping && !moveBehaviour.isSitting)
		{
			return CommonUtils.GetGameController().IsDancing;
		}
		return true;
	}

	private void Update()
	{
		if (disallowMoving())
		{
			return;
		}
		h = Input.GetAxis("Horizontal");
		v = Input.GetAxis("Vertical");
		anim.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
		anim.SetFloat(vFloat, v, 0.1f, Time.deltaTime);
		sprint = Input.GetButton(sprintButton);
		if (isSprinting())
		{
			changedFOV = true;
			if (povBeforeSprinting == 0f)
			{
				povBeforeSprinting = camScript.transform.GetComponent<Camera>().fieldOfView;
			}
			camScript.SetFOV(sprintFOV);
		}
		else if (changedFOV)
		{
			camScript.setPOVBeforeSprint(povBeforeSprinting);
			camScript.ResetFOV();
			changedFOV = false;
			povBeforeSprinting = 0f;
		}
		anim.SetBool(groundedBool, IsGrounded());
	}

	private void FixedUpdate()
	{
		if (disallowMoving())
		{
			return;
		}
		bool flag = false;
		if (behaviourLocked > 0 || overridingBehaviours.Count == 0)
		{
			foreach (GenericBehaviour behaviour in behaviours)
			{
				if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode())
				{
					flag = true;
					behaviour.LocalFixedUpdate();
				}
			}
		}
		else
		{
			foreach (GenericBehaviour overridingBehaviour in overridingBehaviours)
			{
				overridingBehaviour.LocalFixedUpdate();
			}
		}
		if (!flag && overridingBehaviours.Count == 0)
		{
			rBody.useGravity = true;
			Repositioning();
		}
	}

	private void LateUpdate()
	{
		if (behaviourLocked > 0 || overridingBehaviours.Count == 0)
		{
			foreach (GenericBehaviour behaviour in behaviours)
			{
				if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode())
				{
					behaviour.LocalLateUpdate();
				}
			}
			return;
		}
		foreach (GenericBehaviour overridingBehaviour in overridingBehaviours)
		{
			overridingBehaviour.LocalLateUpdate();
		}
	}

	public void SubscribeBehaviour(GenericBehaviour behaviour)
	{
		behaviours.Add(behaviour);
	}

	public void RegisterDefaultBehaviour(int behaviourCode)
	{
		defaultBehaviour = behaviourCode;
		currentBehaviour = behaviourCode;
	}

	public void RegisterBehaviour(int behaviourCode)
	{
		if (currentBehaviour == defaultBehaviour)
		{
			currentBehaviour = behaviourCode;
		}
	}

	public void UnregisterBehaviour(int behaviourCode)
	{
		if (currentBehaviour == behaviourCode)
		{
			currentBehaviour = defaultBehaviour;
		}
	}

	public bool OverrideWithBehaviour(GenericBehaviour behaviour)
	{
		if (!overridingBehaviours.Contains(behaviour))
		{
			if (overridingBehaviours.Count == 0)
			{
				foreach (GenericBehaviour behaviour2 in behaviours)
				{
					if (behaviour2.isActiveAndEnabled && currentBehaviour == behaviour2.GetBehaviourCode())
					{
						behaviour2.OnOverride();
						break;
					}
				}
			}
			overridingBehaviours.Add(behaviour);
			return true;
		}
		return false;
	}

	public bool RevokeOverridingBehaviour(GenericBehaviour behaviour)
	{
		if (overridingBehaviours.Contains(behaviour))
		{
			overridingBehaviours.Remove(behaviour);
			return true;
		}
		return false;
	}

	public bool IsOverriding(GenericBehaviour behaviour = null)
	{
		if (behaviour == null)
		{
			return overridingBehaviours.Count > 0;
		}
		return overridingBehaviours.Contains(behaviour);
	}

	public bool IsCurrentBehaviour(int behaviourCode)
	{
		return currentBehaviour == behaviourCode;
	}

	public bool GetTempLockStatus(int behaviourCode = 0)
	{
		if (behaviourLocked != 0)
		{
			return behaviourLocked != behaviourCode;
		}
		return false;
	}

	public void LockTempBehaviour(int behaviourCode)
	{
		if (behaviourLocked == 0)
		{
			behaviourLocked = behaviourCode;
		}
	}

	public void UnlockTempBehaviour(int behaviourCode)
	{
		if (behaviourLocked == behaviourCode)
		{
			behaviourLocked = 0;
		}
	}

	public virtual bool isSprinting()
	{
		if (sprint && IsMoving())
		{
			return CanSprint();
		}
		return false;
	}

	public bool CanSprint()
	{
		foreach (GenericBehaviour behaviour in behaviours)
		{
			if (!behaviour.AllowSprint())
			{
				return false;
			}
		}
		foreach (GenericBehaviour overridingBehaviour in overridingBehaviours)
		{
			if (!overridingBehaviour.AllowSprint())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsHorizontalMoving()
	{
		return h != 0f;
	}

	public bool IsMoving()
	{
		if (h == 0f)
		{
			return v != 0f;
		}
		return true;
	}

	public Vector3 GetLastDirection()
	{
		return lastDirection;
	}

	public void SetLastDirection(Vector3 direction)
	{
		lastDirection = direction;
	}

	public void Repositioning()
	{
		if (lastDirection != Vector3.zero)
		{
			lastDirection.y = 0f;
			Quaternion b = Quaternion.LookRotation(lastDirection);
			Quaternion rot = Quaternion.Slerp(rBody.rotation, b, turnSmoothing);
			rBody.MoveRotation(rot);
		}
	}

	public bool IsGrounded()
	{
		return Physics.SphereCast(new Ray(base.transform.position + Vector3.up * 2f * colExtents.x, Vector3.down), colExtents.x, colExtents.x + 0.2f);
	}
}
