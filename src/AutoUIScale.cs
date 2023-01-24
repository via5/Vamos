using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Vamos;

public class AutoUIScale : BasicFeature
{
	private Coroutine cr_ = null;
	private IntPtr w_ = new IntPtr(0);
	private Rect r_ = new Rect();
	private Rect oldr_ = new Rect();

	public AutoUIScale()
		: base("AutoUIScale")
	{
	}

	protected override void DoEnable()
	{
		w_ = FindWindow("UnityWndClass", "VaM");
		if (w_.ToInt64() == 0)
		{
			Log.Error($"can't find vam's window");
			return;
		}

		cr_ = SuperController.singleton.StartCoroutine(CheckWindow());
	}

	protected override void DoDisable()
	{
		if (cr_ != null)
		{
			SuperController.singleton.StopCoroutine(cr_);
			cr_ = null;
		}
	}

	private IEnumerator CheckWindow()
	{
		for (; ; )
		{
			GetWindowRect(w_, ref r_);

			if (r_.Left != oldr_.Left ||
				r_.Top != oldr_.Top ||
				r_.Right != oldr_.Right ||
				r_.Bottom != oldr_.Bottom)
			{
				oldr_ = r_;
				if (!CheckScaling())
				{
					Log.Error("failed, disabling");
					Disable();
					yield break;
				}
			}

			yield return new WaitForSeconds(1);
		}
	}

	private bool CheckScaling()
	{
		var m = MonitorFromWindow(w_, MONITOR_DEFAULTTO.NEAREST);
		if (m.ToInt64() == 0)
		{
			Log.Error($"can't find vam's monitor");
			return false;
		}

		uint x, y;
		GetDpiForMonitor(m, DpiType.Effective, out x, out y);

		float scaling = x / 96.0f;
		SuperController.singleton.monitorUIScale = scaling;

		return true;
	}

	public enum DpiType
	{
		Effective = 0,
		Angular = 1,
		Raw = 2,
	}


	internal struct Rect
	{
		public int Left { get; set; }
		public int Top { get; set; }
		public int Right { get; set; }
		public int Bottom { get; set; }
	}

	[Flags]
	internal enum MONITOR_DEFAULTTO
	{
		NULL = 0x00000000,
		PRIMARY = 0x00000001,
		NEAREST = 0x00000002,
	}

	[DllImport("User32.dll", SetLastError = true)]
	internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, MONITOR_DEFAULTTO dwFlags);

	[DllImport("Shcore.dll")]
	internal static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

	[DllImport("user32.dll", EntryPoint = "FindWindow")]
	internal static extern IntPtr FindWindow(String className, String windowName);

	[DllImport("user32.dll")]
	internal static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);
}
