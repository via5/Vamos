using BepInEx;
using System.Collections.Generic;
using UnityEngine;

namespace Vamos;

public static class Meta
{
	public const string Guid = "vamos";
	public const string Name = "Vamos";
	public const string Version = "1.0";
	public const string String = $"{Name} {Version}";
}


public interface IFeature
{
	void Enable();
	void Disable();
}


public abstract class BasicFeature : IFeature
{
	private readonly string name_;
	private readonly Logger log_;

	protected BasicFeature(string name)
	{
		name_ = name;
		log_ = new Logger(name);
	}

	public Logger Log
	{
		get { return log_; }
	}

	public void Enable()
	{
		Log.Info("enabled");
		DoEnable();
	}

	public void Disable()
	{
		Log.Info("disabled");
		DoDisable();
	}

	protected abstract void DoEnable();
	protected abstract void DoDisable();
}


[BepInPlugin(Meta.Guid, Meta.Name, Meta.Version)]
public class Vamos : BaseUnityPlugin
{
	private readonly List<IFeature> features_ = new List<IFeature>();
	private Coroutine cr_ = null;

    void Awake()
    {
		Debug.Log($"loaded {Meta.String}");

		features_.Clear();
		features_.Add(new AutoUIScale());
		features_.Add(new DropFiles());
	}

	void OnEnable()
	{
		cr_ = StartCoroutine(Run());
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
		Cleanup();
	}

	void OnDestroy()
	{
		Cleanup();
	}

	private void Cleanup()
	{
		if (cr_ != null)
		{
			StopCoroutine(cr_);
			cr_ = null;
		}

		foreach (var f in features_)
			f.Disable();

		features_.Clear();
	}
}
