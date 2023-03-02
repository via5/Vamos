using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vamos;

public static class Meta
{
	public const string Guid = "via5.vamos";
	public const string Name = "Vamos";
	public const string Version = "1.1";
	public const string String = $"{Name} {Version}";
}


public interface IFeature
{
	void Enable();
	void Disable();
}


public abstract class BasicFeature : IFeature
{
	struct Conf
	{
		public ConfigEntry<string> enabled;
	}

	private readonly string name_;
	private readonly Logger log_;
	private Conf conf_;
	private bool enabled_ = false;

	protected BasicFeature(string name)
	{
		name_ = name;
		log_ = new Logger(name);
		conf_ = new Conf();
	}

	public Logger Log
	{
		get { return log_; }
	}

	public void Enable()
	{
		try
		{
			conf_.enabled = Vamos.Instance.Config.Bind(
				name_, "enabled", "true",
				$"Whether {name_} is enabled");

			if (conf_.enabled.Value.ToLower() == "true")
			{
				Log.Info("enabled");

				enabled_ = true;
				DoEnable();
			}
			else
			{
				Log.Info("disabled in configuration");
			}
		}
		catch (Exception e)
		{
			Debug.LogError($"exception in {name_} Enable():");
			Debug.LogError(e.ToString());
		}
	}

	public void Disable()
	{
		try
		{
			if (!enabled_)
				return;

			Log.Info("disabled");
			DoDisable();
		}
		catch (Exception e)
		{
			Debug.LogError($"exception in {name_} Disable():");
			Debug.LogError(e.ToString());
		}
	}

	protected abstract void DoEnable();
	protected abstract void DoDisable();
}



[BepInPlugin(Meta.Guid, Meta.Name, Meta.Version)]
public class Vamos : BaseUnityPlugin
{
	private static Vamos instance_ = null;
	private readonly List<IFeature> features_ = new List<IFeature>();
	private Coroutine cr_ = null;
	private bool quitting_ = false;

	public static Vamos Instance
	{
		get { return instance_; }
	}

	public Coroutine StartCo(IEnumerator e)
	{
		if (quitting_)
			return null;

		return StartCoroutine(e);
	}

	public void StopCo(Coroutine co)
	{
		if (quitting_)
			return;

		StopCoroutine(co);
	}

	void Awake()
    {
		try
		{
			instance_ = this;

			Debug.Log($"loaded {Meta.String}");

			features_.Clear();
			features_.Add(new AutoUIScale());
			features_.Add(new DropFiles());
			features_.Add(new API());
		}
		catch (Exception e)
		{
			Debug.LogError($"exception in Vamos.Awake():");
			Debug.LogError(e.ToString());
		}
	}

	void OnEnable()
	{
		try
		{
			cr_ = StartCo(Run());
		}
		catch (Exception e)
		{
			Debug.LogError($"exception in Vamos.OnEnable():");
			Debug.LogError(e.ToString());
		}
	}

	void OnApplicationQuit()
	{
		quitting_ = true;
	}

	System.Collections.IEnumerator Run()
	{
		yield return new WaitUntil(() =>
		{
			return
				SuperController.singleton != null &&
				SuperController.singleton.assetManagerReady;
		});

		foreach (var f in features_)
			f.Enable();
	}

	void OnDisable()
	{
		try
		{
			Cleanup();
		}
		catch (Exception e)
		{
			Debug.LogError($"exception in Vamos.Run():");
			Debug.LogError(e.ToString());
		}
	}

	void OnDestroy()
	{
		try
		{
			Cleanup();
		}
		catch (Exception e)
		{
			Debug.LogError($"exception in Vamos.OnDestroy():");
			Debug.LogError(e.ToString());
		}
	}

	private void Cleanup()
	{
		if (cr_ != null)
		{
			StopCo(cr_);
			cr_ = null;
		}

		foreach (var f in features_)
			f.Disable();

		features_.Clear();
		instance_ = null;
	}
}
