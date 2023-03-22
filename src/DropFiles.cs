using B83.Win32;
using MVR.FileManagement;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Vamos;

public class DropFiles : BasicFeature
{
	public DropFiles()
		: base("DropFiles")
	{
	}

	protected override void DoEnable()
	{
		UnityDragAndDropHook.InstallHook();
		UnityDragAndDropHook.OnDroppedFiles += OnDrop;
	}

	protected override void DoDisable()
	{
		UnityDragAndDropHook.UninstallHook();
		UnityDragAndDropHook.OnDroppedFiles += OnDrop;
	}

	void OnDrop(List<string> files, POINT pos)
	{
		Vamos.Instance.StartCo(CoDrop(files));
	}

	private IEnumerator CoDrop(List<string> files)
	{
		string f = null;

		try
		{
			if (files.Count == 0)
			{
				Log.Error($"no files dropped");
				yield break;
			}

			if (files.Count > 1)
			{
				Log.Error($"dropping multiple files is not supported");
				yield break;
			}

			f = new System.IO.FileInfo(files[0]).Name;
			Log.Info($"dropped {f}");
		}
		catch (Exception e)
		{
			Log.Error(e.ToString());
			f = null;
		}

		if (f == null)
			yield break;

		yield return Open(f);
	}

	private IEnumerator Open(string filename)
	{
		var f = $"AddonPackages\\{filename}";

		var p = FileManager.GetPackage(f);
		if (p == null)
		{
			Log.Error($"no package for {f}");
			yield break;
		}

		List<FileEntry> files = new List<FileEntry>();
		p.FindFiles("Saves/scene/", "*.json", files);

		if (files.Count == 0)
		{
			Log.Error($"no scene files in {f}");
			yield break;
		}
		else if (files.Count > 1)
		{
			if (SuperController.singleton != null)
			{
				Log.Info($"multiple scene files, opening load dialog for {f}");
				SuperController.singleton.LoadSceneDialog();
				SuperController.singleton.fileBrowserUI.GotoDirectory(f);
			}

			yield break;
		}

		Log.Info($"loading {f}");
		yield return new UnityEngine.WaitForSeconds(0.5f);

		if (SuperController.singleton != null)
			SuperController.singleton.Load(files[0].Uid);
	}
}
