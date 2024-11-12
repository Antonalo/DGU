using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonEventHandler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Sprite normalSprite;

	public Sprite hoverSprite;

	public Sprite selectedSprite;

	private Text txt;

	private Color textColor;

	private Button btn;

	private Image btnImg;

	private bool interactableDelay;

	private Sprite defaultSprite;

	private void Start()
	{
		txt = GetComponentInChildren<Text>();
		textColor = txt.color;
		btn = base.gameObject.GetComponent<Button>();
		btnImg = btn.GetComponent<Image>();
		defaultSprite = btnImg.sprite;
		interactableDelay = btn.interactable;
	}

	private void Update()
	{
		if (btn == null)
		{
			btn = GetComponent<Button>();
		}
		if (btn.interactable != interactableDelay)
		{
			updateTextColor();
		}
		interactableDelay = btn.interactable;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (btnImg.sprite != selectedSprite)
		{
			btnImg.sprite = hoverSprite;
		}
		updateTextColor();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		btnImg.sprite = normalSprite;
		updateTextColor();
	}

	private void updateTextColor()
	{
		if (btn.interactable)
		{
			txt.color = textColor * btn.colors.normalColor * btn.colors.colorMultiplier;
		}
		else
		{
			txt.color = textColor * btn.colors.disabledColor * btn.colors.colorMultiplier;
		}
	}
}
