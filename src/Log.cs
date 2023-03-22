using System;

namespace Vamos;

public class Logger
{
	public enum Levels
	{
		Verbose,
		Info,
		Error
	}

	private readonly string name_;

	public Logger(string name)
	{
		name_ = name;
	}

	public void Verbose(string s)
	{
		DoLog(Levels.Verbose, s);
	}

	public void Info(string s)
	{
		DoLog(Levels.Info, s);
	}

	public void Error(string s)
	{
		DoLog(Levels.Error, s);
	}

	private void DoLog(Levels lv, string s)
	{
		var t = DateTime.Now.ToString("hh:mm:ss.fff");

		switch (lv)
		{
			case Levels.Verbose:
			{
				//SuperController.LogMessage($"{t} [Vamos.{name_}] {s}");
				break;
			}

			case Levels.Info:
			{
				SuperController.LogMessage($"{t} [Vamos.{name_}] {s}");
				break;
			}

			case Levels.Error:
			{
				SuperController.LogMessage($"{t} [Vamos.{name_}] {s}");
				break;
			}
		}
	}
}
