using Photon.Pun;
using UnityEngine;

public abstract class GenericBehaviour : MonoBehaviourPunCallbacks
{
	protected int speedFloat;

	protected BasicBehaviour behaviourManager;

	protected int behaviourCode;

	protected bool canSprint;

	protected Animator animator;

	private void Awake()
	{
		behaviourManager = GetComponent<BasicBehaviour>();
		animator = GetComponent<Animator>();
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
