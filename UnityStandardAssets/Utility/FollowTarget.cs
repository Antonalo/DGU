using UnityEngine;

namespace UnityStandardAssets.Utility
{
	public class FollowTarget : MonoBehaviour
	{
		public Transform target;

		public Vector3 offset = new Vector3(0f, 7.5f, 0f);

		public float delay;

		private void Start()
		{
			if (delay != 0f)
			{
				InvokeRepeating("updatePosition", 0f, delay);
			}
		}

		private void LateUpdate()
		{
			if (delay == 0f)
			{
				updatePosition();
			}
		}

		private void updatePosition()
		{
			if (!(target == null))
			{
				base.transform.position = target.position + offset;
			}
		}
	}
}
