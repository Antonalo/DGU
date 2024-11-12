using Photon.Pun;
using UnityEngine;

public class CharacterSwapper : MonoBehaviourPunCallbacks
{
	private bool rebindAnimator;

	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public void swap(Item item, bool isShop)
	{
		CommonUtils.GetInventoryController().loadingPanel.SetActive(value: true);
		if (!isShop)
		{
			int currentItemId = int.Parse(base.transform.Find(item.itemType.ToUpper()).GetChild(0).name);
			Global.inUsedItems.RemoveAll((Item i) => i.itemId == currentItemId);
			Global.inUsedItems.Add(item);
			CommonUtils.GetInventoryController().toggleHighlightItem(currentItemId, isHighlight: false);
			base.photonView.RPC("RPCSwap", RpcTarget.OthersBuffered, item.itemId, item.itemName);
		}
		RPCSwap(item.itemId, item.itemName);
		CommonUtils.GetInventoryController().loadingPanel.SetActive(value: false);
	}

	[PunRPC]
	public void RPCSwap(int characterId, string characterPrefab)
	{
		Transform transform = base.transform.Find("CHARACTER");
		foreach (Transform item in transform)
		{
			Object.Destroy(item.gameObject);
		}
		Transform transform2 = Object.Instantiate(Resources.Load("Prefabs/Character/" + characterPrefab) as GameObject, transform).transform;
		if (CommonUtils.GetInventoryController().canvas.activeSelf)
		{
			CommonUtils.setLayer(transform2.gameObject, Global.characterOnUILayer);
		}
		animator.avatar = transform2.GetComponent<Animator>().avatar;
		animator.enabled = false;
		transform2.localPosition = Vector3.zero;
		transform2.localRotation = Quaternion.identity;
		Object.Destroy(transform2.GetComponent<Animator>());
		transform2.name = characterId.ToString();
		rebindAnimator = true;
	}

	private void LateUpdate()
	{
		if (rebindAnimator)
		{
			animator.Rebind();
			animator.enabled = true;
			rebindAnimator = false;
		}
	}
}
