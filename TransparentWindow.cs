using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransparentWindow : MonoBehaviour
{
	private struct MARGINS
	{
		public int cxLeftWidth;

		public int cxRightWidth;

		public int cyTopHeight;

		public int cyBottomHeight;
	}

	public Material material;

	private const int GWL_STYLE = -16;

	private const uint WS_POPUP = 2147483648u;

	private const uint WS_VISIBLE = 268435456u;

	private const int HWND_TOPMOST = -1;

	private bool clickThrough;

	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

	[DllImport("user32.dll")]
	private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	private static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

	[DllImport("user32.dll")]
	private static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

	[DllImport("Dwmapi.dll")]
	private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

	private void Start()
	{
		MARGINS mARGINS = default(MARGINS);
		mARGINS.cxLeftWidth = -1;
		MARGINS margins = mARGINS;
		IntPtr activeWindow = GetActiveWindow();
		SetWindowLong(activeWindow, -16, 2415919104u);
		DwmExtendFrameIntoClientArea(activeWindow, ref margins);
	}

	private void ChangeTransparency()
	{
		MARGINS mARGINS = default(MARGINS);
		mARGINS.cxLeftWidth = -1;
		MARGINS margins = mARGINS;
		IntPtr activeWindow = GetActiveWindow();
		if (!clickThrough)
		{
			Debug.Log("Window is click-able now");
			mARGINS = default(MARGINS);
			mARGINS.cxLeftWidth = 0;
			mARGINS.cxRightWidth = 0;
			mARGINS.cyTopHeight = 0;
			mARGINS.cyBottomHeight = 0;
			margins = mARGINS;
			SetWindowLong(activeWindow, -16, 2415919104u);
		}
		else
		{
			Debug.Log("Window is click-through now");
			int width = Screen.width;
			int height = Screen.height;
			SetWindowLong(activeWindow, -20, 524320u);
			SetLayeredWindowAttributes(activeWindow, 0, byte.MaxValue, 2);
			SetWindowPos(activeWindow, -1, 0, 0, width, height, 96);
		}
		DwmExtendFrameIntoClientArea(activeWindow, ref margins);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			clickThrough = !clickThrough;
			ChangeTransparency();
		}
	}

	private void OnRenderImage(RenderTexture from, RenderTexture to)
	{
		Graphics.Blit(from, to, material);
	}
}
