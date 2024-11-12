using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileController : MonoBehaviour
{
	private Slider hpSlider;

	private TextMeshProUGUI hpText;

	private Slider manaSlider;

	private TextMeshProUGUI manaText;

	private Text userNameText;

	private TextMeshProUGUI levelText;

	private TextMeshProUGUI vipText;

	private TextMeshProUGUI goldText;

	private TextMeshProUGUI gemText;

	private Image avatar;

	private Transform avatarBorderParent;

	private Transform avatarBorderDefault;

	private string picture;

	private void Start()
	{
		Transform transform = GameObject.Find("Screen/Profile").transform;
		picture = "";
		hpSlider = transform.Find("HP").GetComponent<Slider>();
		hpText = transform.Find("HP/Slider/Number").GetComponent<TextMeshProUGUI>();
		manaSlider = transform.Find("Mana").GetComponent<Slider>();
		manaText = transform.Find("Mana/Slider/Number").GetComponent<TextMeshProUGUI>();
		userNameText = transform.Find("Name").GetComponent<Text>();
		levelText = transform.Find("Level").GetComponent<TextMeshProUGUI>();
		vipText = transform.Find("VIP").GetComponent<TextMeshProUGUI>();
		goldText = transform.Find("Gold").GetComponent<TextMeshProUGUI>();
		gemText = transform.Find("Gem").GetComponent<TextMeshProUGUI>();
		Transform transform2 = transform.Find("AvatarParent");
		avatar = transform2.Find("Avatar/Image").GetComponent<Image>();
		avatarBorderParent = transform2.Find("AvatarBorder");
		avatarBorderDefault = transform2.Find("Avatar_Default");
	}

	public void setupAVATAR(Item item)
	{
		foreach (Transform item2 in avatarBorderParent)
		{
			Object.Destroy(item2.gameObject);
		}
		if (item != null)
		{
			Object.Instantiate(Resources.Load<GameObject>("Items/common/AVATAR/" + item.itemName), avatarBorderParent);
		}
		avatarBorderDefault.gameObject.SetActive(avatarBorderParent.childCount == 0);
	}

	public void setupProfile(User user)
	{
		Debug.Log(">>>>>>>>>>>>>> Setting up Profile now...:");
		if (user != null && Global.user.id.Equals(user.id))
		{
			userNameText.text = user.name;
			levelText.SetText("Level " + user.level + " (" + user.levelName + ")");
			setHPBar(user.currentExp, user.nextExp);
		}
		Debug.Log("Setting up Profile now: DONE");
	}

	public void setHPBar(float currentExp, float nextExp)
	{
		float t = currentExp / nextExp;
		hpSlider.value = Mathf.Lerp(0f, 1f, t);
		hpText.SetText(Mathf.RoundToInt(currentExp) + " / " + nextExp);
	}
}
