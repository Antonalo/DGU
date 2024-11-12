using UnityEngine;

public abstract class GenericBehaviour1 : MonoBehaviour
{
	protected int speedFloat;

	protected PlayerBasicCode behaviourManager;

	protected int behaviourCode;

	protected bool canSprint;

	private void Awake()
	{
		behaviourManager = GetComponent<PlayerBasicCode>();
		speedFloat = Animator.StringToHash("Speed");
		canSprint = true;
		behaviourCode = GetType().GetHashCode();
	}

	public virtual void LocalFixedUpdate()
	{
	}

	public virtual void LocalLateUpdate()
	{
	}

	public virtual void OnOverride()
	{
	}

	public int GetBehaviourCode()
	{
		return behaviourCode;
	}

	public bool AllowSprint()
	{
		return canSprint;
	}
}
