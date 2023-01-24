using B83.Win32;
using MVR.FileManagement;
using System;
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
		try
		{
			if (files.Count == 0)
				return;

			if (files.Count > 1)
			{
				Log.Error($"dropping multiple files is not supported");
				return;
			}

			var f = new System.IO.FileInfo(files[0]).Name;
			Log.Info($"dropped {f}");

			Drop(f);
		}
		catch (Exception e)
		{
			Log.Error(e.ToString());
		}
	}

	private void Drop(string filename)
	{
		var f = $"AddonPackages\\{filename}";

		var p = FileManager.GetPackage(f);
		if (p == null)
		{
			Log.Error($"no package for {f}");
			return;
		}

		List<FileEntry> files = new List<FileEntry>();
		p.FindFiles("Saves/scene/", "*.json", files);

		if (files.Count == 0)
		{
			Log.Error($"no scene files in {f}");
			return;
		}
		else if (files.Count > 1)
		{
			Log.Error($"multiple scene files in {f}");
			return;
		}

		Log.Info($"loading {f}");
		SuperController.singleton.Load(files[0].Uid);
	}
}
