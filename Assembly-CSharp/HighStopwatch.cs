using System;
using System.Runtime.InteropServices;

public class HighStopwatch
{
	private const string lib = "kernel32.dll";

	private bool isPerfCounterSupported;

	private long frequency;

	public long Frequency => frequency;

	public long Value
	{
		get
		{
			long count = 0L;
			if (isPerfCounterSupported)
			{
				QueryPerformanceCounter(ref count);
				return count;
			}
			return Environment.TickCount;
		}
	}

	public HighStopwatch()
	{
		if (QueryPerformanceFrequency(ref frequency) != 0 && frequency != 1000)
		{
			isPerfCounterSupported = true;
		}
		else
		{
			frequency = 1000L;
		}
	}

	[DllImport("kernel32.dll")]
	private static extern int QueryPerformanceCounter(ref long count);

	[DllImport("kernel32.dll")]
	private static extern int QueryPerformanceFrequency(ref long frequency);
}
