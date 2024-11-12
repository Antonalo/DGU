using UnityEngine;
using UnityEngine.EventSystems;

public class HoverMouse : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public Texture2D cursorTexture;

	public CursorMode cursorMode;

	public Vector2 hotSpot = Vector2.zero;

	public void OnPointerEnter(PointerEventData eventData)
	{
		Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Cursor.SetCursor(null, Vector2.zero, cursorMode);
	}
}
