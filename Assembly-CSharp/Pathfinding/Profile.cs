using System;
using System.Diagnostics;
using UnityEngine;

namespace Pathfinding;

public class Profile
{
	private const bool PROFILE_MEM = false;

	public string name;

	private Stopwatch w;

	private int counter;

	private long mem;

	private long smem;

	private int control = 1073741824;

	private bool dontCountFirst;

	public Profile(string name)
	{
		this.name = name;
		w = new Stopwatch();
	}

	public int ControlValue()
	{
		return control;
	}

	[Conditional("PROFILE")]
	public void Start()
	{
		if (!dontCountFirst || counter != 1)
		{
			w.Start();
		}
	}

	[Conditional("PROFILE")]
	public void Stop()
	{
		counter++;
		if (!dontCountFirst || counter != 1)
		{
			w.Stop();
		}
	}

	[Conditional("PROFILE")]
	public void Log()
	{
		UnityEngine.Debug.Log(ToString());
	}

	[Conditional("PROFILE")]
	public void ConsoleLog()
	{
		Console.WriteLine(ToString());
	}

	[Conditional("PROFILE")]
	public void Stop(int control)
	{
		counter++;
		if (!dontCountFirst || counter != 1)
		{
			w.Stop();
			if (this.control == 1073741824)
			{
				this.control = control;
			}
			else if (this.control != control)
			{
				throw new Exception("Control numbers do not match " + this.control + " != " + control);
			}
		}
	}

	[Conditional("PROFILE")]
	public void Control(Profile other)
	{
		if (ControlValue() != other.ControlValue())
		{
			throw new Exception("Control numbers do not match (" + name + " " + other.name + ") " + ControlValue() + " != " + other.ControlValue());
		}
	}

	public override string ToString()
	{
		return name + " #" + counter + " " + w.Elapsed.TotalMilliseconds.ToString("0.0 ms") + " avg: " + (w.Elapsed.TotalMilliseconds / (double)counter).ToString("0.00 ms");
	}
}
