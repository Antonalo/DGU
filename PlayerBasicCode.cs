using System.Collections.Generic;
using UnityEngine;

public class PlayerBasicCode : MonoBehaviour
{
	public Transform playerCamera;

	public float turnSmoothing = 0.06f;

	public float sprintFOV = 100f;

	private float h;

	private float v;

	private int currentBehaviour;

	private int defaultBehaviour;

	private int behaviourLocked;

	private Vector3 lastDirection;

	private Animator anim;

	private ThirdPerson camScript;

	private bool sprint;

	private bool changedFOV;

	private int hFloat;

	private int vFloat;

	private List<GenericBehaviour1> behaviours;

	private List<GenericBehaviour1> overridingBehaviours;

	private Rigidbody rBody;

	private int groundedBool;

	private Vector3 colExtents;

	public float GetH => h;

	public float GetV => v;

	public ThirdPerson GetCamScript => camScript;

	public Rigidbody GetRigidBody => rBody;

	public Animator GetAnim => anim;

	public int GetDefaultBehaviour => defaultBehaviour;

	private void Awake()
	{
		behaviours = new List<GenericBehaviour1>();
		overridingBehaviours = new List<GenericBehaviour1>();
		anim = GetComponent<Animator>();
		hFloat = Animator.StringToHash("H");
		vFloat = Animator.StringToHash("V");
		camScript = playerCamera.GetComponent<ThirdPerson>();
		rBody = GetComponent<Rigidbody>();
		groundedBool = Animator.StringToHash("Grounded");
		colExtents = GetComponent<Collider>().bounds.extents;
	}

	private void Update()
	{
		h = Input.GetAxis("Horizontal");
		v = Input.GetAxis("Vertical");
		anim.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
		anim.SetFloat(vFloat, v, 0.1f, Time.deltaTime);
		sprint = Input.GetKey(KeyCode.LeftShift);
		if (IsSprinting())
		{
			changedFOV = true;
			camScript.SetFOV(sprintFOV);
		}
		else if (changedFOV)
		{
			camScript.ResetFOV();
			changedFOV = false;
		}
		anim.SetBool(groundedBool, IsGrounded());
	}

	private void FixedUpdate()
	{
		bool flag = false;
		if (behaviourLocked > 0 || overridingBehaviours.Count == 0)
		{
			foreach (GenericBehaviour1 behaviour in behaviours)
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
			foreach (GenericBehaviour1 overridingBehaviour in overridingBehaviours)
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
			foreach (GenericBehaviour1 behaviour in behaviours)
			{
				if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode())
				{
					behaviour.LocalLateUpdate();
				}
			}
			return;
		}
		foreach (GenericBehaviour1 overridingBehaviour in overridingBehaviours)
		{
			overridingBehaviour.LocalLateUpdate();
		}
	}

	public void SubscribeBehaviour(GenericBehaviour1 behaviour)
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

	public bool OverrideWithBehaviour(GenericBehaviour1 behaviour)
	{
		if (!overridingBehaviours.Contains(behaviour))
		{
			if (overridingBehaviours.Count == 0)
			{
				foreach (GenericBehaviour1 behaviour2 in behaviours)
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

	public bool RevokeOverridingBehaviour(GenericBehaviour1 behaviour)
	{
		if (overridingBehaviours.Contains(behaviour))
		{
			overridingBehaviours.Remove(behaviour);
			return true;
		}
		return false;
	}

	public bool IsOverriding(GenericBehaviour1 behaviour = null)
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

	public bool GetTempLockStatus(int behaviourCodeIgnoreSelf = 0)
	{
		if (behaviourLocked != 0)
		{
			return behaviourLocked != behaviourCodeIgnoreSelf;
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

	public virtual bool IsSprinting()
	{
		if (sprint && IsMoving())
		{
			return CanSprint();
		}
		return false;
	}

	public bool CanSprint()
	{
		foreach (GenericBehaviour1 behaviour in behaviours)
		{
			if (!behaviour.AllowSprint())
			{
				return false;
			}
		}
		foreach (GenericBehaviour1 overridingBehaviour in overridingBehaviours)
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
