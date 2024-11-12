using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
	public Camera inventoryCamera;

	public GameObject canvas;

	public GameObject loadingPanel;

	public TextMeshProUGUI title;

	public Text pageNumner;

	public GameObject nextBtn;

	public GameObject PrevBtn;

	public Sprite disableNxtPg;

	public Sprite enableNxtPg;

	public Sprite disablePrevPg;

	public Sprite enablePrevPg;

	public Transform gridLayout;

	public Sprite defaultItemSprite;

	public Sprite selectedItemSprite;

	private List<Item> items;

	private Image currentItem;

	private bool isShop;

	private int currentPage;

	private int pageSize = 10;

	private int totalPages;

	private ItemRequest itemRequest;

	private List<int> storeTypes;

	private void Start()
	{
		items = new List<Item>();
		itemRequest = new ItemRequest();
		storeTypes = new List<int>();
		storeTypes.Add(0);
		itemRequest.storeTypes = storeTypes;
	}

	public void toggle(bool isOn, bool isShopOpened = false)
	{
		canvas.SetActive(isOn);
		if (isOn)
		{
			isShop = isShopOpened;
			currentPage = 0;
			totalPages = 0;
			loadItem();
			title.SetText(isShop ? LeanLocalization.GetTranslationText("Shop") : LeanLocalization.GetTranslationText("Bag"));
			return;
		}
		if (isShop)
		{
			foreach (Item inUsedItem in Global.inUsedItems)
			{
				CommonUtils.GetLocalPlayer().GetComponent<CharacterSwapper>().swap(inUsedItem, isShop);
			}
			return;
		}
		StartCoroutine(syncInUsedItems());
	}

	private IEnumerator syncInUsedItems()
	{
		List<int> list = new List<int>();
		foreach (Item inUsedItem in Global.inUsedItems)
		{
			list.Add(inUsedItem.itemId);
		}
		string url = Global.ITEM_URL + "/inused";
		UnityWebRequest request = CommonUtils.prepareRequest(url, "POST", JsonHelper.ToJson(list));
		yield return request.SendWebRequest();
		RespStatus respStatus = JsonUtility.FromJson<RespStatus>(JSON.Parse(request.downloadHandler.text)["status"].ToString());
		if (respStatus.code != 0)
		{
			Debug.Log("Fail to get Item List");
			CommonUtils.showPopupMsg(respStatus.message);
		}
		else
		{
			Debug.Log("[syncInUsedItems] success");
		}
	}

	public void loadItem()
	{
		loadingPanel.SetActive(value: true);
		items.Clear();
		foreach (Transform item in gridLayout)
		{
			Object.Destroy(item.gameObject);
		}
		StartCoroutine(getItemList(itemRequest, storeTypes));
	}

	private IEnumerator getItemList(ItemRequest itemRequest, List<int> storeTypes)
	{
		string url = Global.ITEM_URL + (isShop ? "/shop" : "/bag") + "?page=" + currentPage + "&size=" + pageSize + "&sort=createdDate,desc";
		UnityWebRequest request = CommonUtils.prepareRequest(url, "POST", JsonUtility.ToJson(itemRequest));
		yield return request.SendWebRequest();
		string text = request.downloadHandler.text;
		Debug.Log("[" + base.name + "] getItemList response: " + text);
		JSONNode jSONNode = JSON.Parse(text);
		RespStatus respStatus = JsonUtility.FromJson<RespStatus>(jSONNode["status"].ToString());
		if (respStatus.code != 0)
		{
			Debug.LogError("Fail to get Item List");
			CommonUtils.showPopupMsg(respStatus.message);
			yield break;
		}
		Page<Item> page = JsonUtility.FromJson<Page<Item>>(jSONNode["data"].ToString());
		items = page.content;
		totalPages = page.totalPages;
		pageNumner.text = currentPage + 1 + " / " + totalPages;
		populateItems();
	}

	private void populateItems()
	{
		foreach (Item item in items)
		{
			GameObject itemGO = Object.Instantiate(Resources.Load("Prefabs/Inventory/Item") as GameObject, gridLayout);
			itemGO.transform.localScale = new Vector3(1f, 1f, 1f);
			itemGO.name = item.itemId.ToString();
			bool flag = Global.inUsedItems.Exists((Item i) => i.itemId == item.itemId);
			itemGO.GetComponent<Image>().sprite = (flag ? selectedItemSprite : defaultItemSprite);
			itemGO.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Items/" + item.itemName);
			itemGO.transform.Find("Name").GetComponent<Text>().text = item.itemName;
			itemGO.transform.Find("Price").GetComponent<Text>().text = ((item.levelRequired > Global.user.level) ? ("Level " + item.levelRequired) : item.price.ToString());
			itemGO.transform.Find("Price").GetComponent<Text>().color = ((item.levelRequired > Global.user.level) ? Color.red : Color.white);
			itemGO.transform.Find("Buy_Btn").gameObject.SetActive(isShop);
			itemGO.transform.Find("Buy_Btn").GetComponent<Button>().onClick.AddListener(delegate
			{
				confirmBuyItems(item);
			});
			itemGO.GetComponent<Button>().onClick.RemoveAllListeners();
			itemGO.GetComponent<Button>().onClick.AddListener(delegate
			{
				if (item.levelRequired > Global.user.level)
				{
					CommonUtils.addNormalMsg(LeanLocalization.GetTranslationText("LevelNotSufficient"));
				}
				else
				{
					if ((bool)currentItem)
					{
						currentItem.sprite = defaultItemSprite;
					}
					itemGO.GetComponent<Image>().sprite = selectedItemSprite;
					if (item.isItemType(Item.CharacterPart.CHARACTER))
					{
						CommonUtils.GetLocalPlayer().GetComponent<CharacterSwapper>().swap(item, isShop);
					}
					CommonUtils.GetAnimator().SetBool("isMale", item.isMale);
					currentItem = itemGO.GetComponent<Image>();
				}
			});
		}
		loadingPanel.SetActive(value: false);
	}

	public void toggleHighlightItem(int itemId, bool isHighlight)
	{
		if (canvas.activeSelf)
		{
			gridLayout.Find(itemId.ToString()).GetComponent<Image>().sprite = (isHighlight ? selectedItemSprite : defaultItemSprite);
		}
	}

	public void confirmBuyItems(Item item)
	{
		List<int> list = new List<int>();
		list.Add(item.itemId);
		Debug.Log("Purchasing Item Request: " + JsonHelper.ToJson(list));
		string content = string.Format(LeanLocalization.GetTranslationText("ConfirmMsgToBuyItem"), item.itemName);
		CommonUtils.GetGameController().showConfirmPopup(isDisplay: true, ISendPurchaseItemRequest(JsonHelper.ToJson(list)), "", content);
	}

	private IEnumerator ISendPurchaseItemRequest(string jsonReq)
	{
		UnityWebRequest request = CommonUtils.prepareRequest(Global.ITEM_URL + "/purchase", "POST", jsonReq);
		yield return request.SendWebRequest();
		Response<List<int>> response = JsonUtility.FromJson<Response<List<int>>>(request.downloadHandler.text);
		if (response.status.code != 0)
		{
			Debug.LogError("Fail to Purchase Item : " + response.status.code);
			CommonUtils.showPopupMsg(response.status.message);
			yield break;
		}
		Debug.Log("Response: " + response.data);
		EventUtils.triggerUserUpdatedEvent(updateClothes: false);
		CommonUtils.addNormalMsg(LeanLocalization.GetTranslationText("BoughtItemSucess"));
		loadItem();
		CommonUtils.GetGameController().showConfirmPopup(isDisplay: false);
	}

	public void onPageNavigationClicked(bool isNext)
	{
		if (isNext)
		{
			if (currentPage >= totalPages - 1)
			{
				nextBtn.GetComponent<Button>().interactable = false;
				nextBtn.GetComponent<Image>().sprite = disableNxtPg;
				return;
			}
			currentPage++;
			if (currentPage == totalPages - 1)
			{
				nextBtn.GetComponent<Button>().interactable = false;
				nextBtn.GetComponent<Image>().sprite = disableNxtPg;
			}
			PrevBtn.GetComponent<Button>().interactable = true;
			PrevBtn.GetComponent<Image>().sprite = enablePrevPg;
		}
		else
		{
			if (currentPage == 0)
			{
				PrevBtn.GetComponent<Button>().interactable = false;
				PrevBtn.GetComponent<Image>().sprite = disablePrevPg;
				return;
			}
			currentPage--;
			if (currentPage == 0)
			{
				PrevBtn.GetComponent<Button>().interactable = false;
				PrevBtn.GetComponent<Image>().sprite = disablePrevPg;
			}
			nextBtn.GetComponent<Button>().interactable = true;
			nextBtn.GetComponent<Image>().sprite = enableNxtPg;
		}
		loadItem();
	}
}
