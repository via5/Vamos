using HarmonyLib;
using System;
using System.Collections.Generic;
using uFileBrowser;
using UnityEngine;

namespace Vamos
{
	public class VamosAPIControl : MonoBehaviour
	{
		public void VamosPing(object[] args)
		{
			try
			{
				API.Instance?.Ping(args[0] as string);
			}
			catch (Exception e)
			{
				SuperController.LogError($"VamosAPIControl: exception during VamosPing");
				SuperController.LogError(e.ToString());
			}
		}

		public void VamosEnableAPI(object[] args)
		{
			try
			{
				API.Instance?.EnableAPI(args[0] as string);
			}
			catch (Exception e)
			{
				SuperController.LogError($"VamosAPIControl: exception during VamosEnableAPI");
				SuperController.LogError(e.ToString());
			}
		}

		public void VamosDisableAPI(object[] args)
		{
			try
			{
				API.Instance?.DisableAPI(args[0] as string);
			}
			catch (Exception e)
			{
				SuperController.LogError($"VamosAPIControl: exception during VamosDisableAPI");
				SuperController.LogError(e.ToString());
			}
		}

		public void VamosInhibitNext(object[] args)
		{
			try
			{
				API.Instance?.InhibitNext(args[0] as string);
			}
			catch (Exception e)
			{
				SuperController.LogError($"VamosAPIControl: exception during VamosInhibitNext");
				SuperController.LogError(e.ToString());
			}
		}
	}


	public class API : BasicFeature
	{
		private static API instance_ = null;

		private Harmony h_ = null;
		private VamosAPIControl control_ = null;

		private readonly Dictionary<string, int> apis_ =
			new Dictionary<string, int>();

		private readonly HashSet<string> inhibit_ = new HashSet<string>();


		public API()
			: base($"api.{Environment.TickCount}")
		{
			instance_ = this;
		}

		public static API Instance
		{
			get { return instance_; }
		}

		public void EnableAPI(string name)
		{
			int i;

			if (apis_.ContainsKey(name))
			{
				i = apis_[name];
				++i;
				apis_[name] = i;
			}
			else
			{
				i = 1;
				apis_.Add(name, i);
			}

			Log.Verbose($"enabled api {name} (refcount {i})");
		}

		public void DisableAPI(string name)
		{
			if (apis_.ContainsKey(name))
			{
				int i = apis_[name];
				if (i > 0)
					--i;

				apis_[name] = i;
				Log.Verbose($"disabled api {name} (refcount {i})");
			}
			else
			{
				Log.Verbose($"disabled api {name} (was already disabled)");
			}
		}

		public void InhibitNext(string name)
		{
			Log.Verbose($"got inhibit {name}, sending ready");

			inhibit_.Add(name);

			SuperController.singleton.gameObject.SendMessage(
				"VamosInhibitReady", new object[] { name },
				SendMessageOptions.DontRequireReceiver);
		}

		public bool IsEnabled(string name)
		{
			int i = 0;
			apis_.TryGetValue(name, out i);
			return (i > 0);
		}

		public bool CanRun(string name)
		{
			if (inhibit_.Contains(name))
			{
				Log.Verbose($"{name} inhibited");
				inhibit_.Remove(name);
				return false;
			}

			return IsEnabled(name);
		}

		public void Ping(string name)
		{
			Log.Verbose($"got ping {name}, sending pong");

			SuperController.singleton.gameObject.SendMessage(
				"VamosPong", new object[] { name },
				SendMessageOptions.DontRequireReceiver);
		}

		protected override void DoEnable()
		{
			if (h_ != null)
			{
				h_.UnpatchSelf();
				h_ = null;
			}

			h_ = new Harmony("com.via5.vamos");
			h_.PatchAll();

			RemoveComponent();
			AddComponent();
		}

		protected override void DoDisable()
		{
			h_?.UnpatchSelf();
			h_ = null;

			RemoveComponent();
		}

		private void AddComponent()
		{
			var sc = SuperController.singleton.gameObject;
			control_ = sc.AddComponent<VamosAPIControl>();
		}

		private void RemoveComponent()
		{
			var sc = SuperController.singleton.gameObject;

			foreach (Component c in sc.GetComponents<Component>())
			{
				if (c.ToString().Contains("VamosAPIControl"))
					UnityEngine.Object.Destroy(c);
			}

			control_ = null;
		}
	}
}


namespace Vamos.APIs
{
	[HarmonyPatch(typeof(FileBrowser), "Show", new Type[] { typeof(FileBrowserCallback), typeof(bool) })]
	class FileBrowserShowCallback
	{
		private const string Name = "Vamos_uFileBrowser_FileBrowser_Show__FileBrowser_FileBrowserCallback_bool";

		//static FieldRef<FileBrowser, FileBrowserCallback> callbackRef =
		//	AccessTools.FieldRefAccess<FileBrowser, FileBrowserCallback>("callback");

		[HarmonyPrefix]
		static bool Prefix(FileBrowser __instance, ref FileBrowserCallback callback, ref bool changeDirectory)
		{
			if (!API.Instance.CanRun(Name))
				return true;

			API.Instance.Log.Verbose($"patched {Name}");

			SuperController.singleton.gameObject.SendMessage(
				Name, new object[] { __instance, callback, changeDirectory },
				SendMessageOptions.DontRequireReceiver);

			return false;
		}
	}

	[HarmonyPatch(typeof(FileBrowser), "Show", new Type[] { typeof(FileBrowserFullCallback), typeof(bool) })]
	class FileBrowserShowFullCallback
	{
		private const string Name = "Vamos_uFileBrowser_FileBrowser_Show__FileBrowser_FileBrowserFullCallback_bool";

		[HarmonyPrefix]
		static bool Prefix(FileBrowser __instance, ref FileBrowserFullCallback fullCallback, ref bool changeDirectory)
		{
			if (!API.Instance.CanRun(Name))
				return true;

			API.Instance.Log.Verbose($"patched {Name}");

			SuperController.singleton.gameObject.SendMessage(
				Name, new object[] { __instance, fullCallback, changeDirectory },
				SendMessageOptions.DontRequireReceiver);

			return false;
		}
	}

	[HarmonyPatch(typeof(FileBrowser), "GotoDirectory", new Type[] { typeof(string), typeof(string), typeof(bool), typeof(bool) })]
	class FileBrowserGotoDirectoryCallback
	{
		private const string Name = "Vamos_uFileBrowser_FileBrowser_GotoDirectory__FileBrowser_FileBrowserCallback_string_string_bool_bool";

		[HarmonyPrefix]
		static bool Prefix(FileBrowser __instance, ref string path, ref string pkgFilter, ref bool flatten, ref bool includeRegularDirs)
		{
			if (!API.Instance.CanRun(Name))
				return true;

			API.Instance.Log.Verbose($"patched {Name}");

			SuperController.singleton.gameObject.SendMessage(
				Name, new object[] { __instance, path, pkgFilter, flatten, includeRegularDirs},
				SendMessageOptions.DontRequireReceiver);

			return false;
		}
	}

}
