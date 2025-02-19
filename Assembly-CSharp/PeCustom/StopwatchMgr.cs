using System.Collections.Generic;
using System.IO;
using ScenarioRTL;

namespace PeCustom;

public class StopwatchMgr
{
	public delegate void DIntNotify(int id);

	public Dictionary<int, Stopwatch> stopwatches = new Dictionary<int, Stopwatch>();

	public event DIntNotify OnTimeout;

	public void SetStopwatch(int id, string name, EFunc func_time, double amount_time, EFunc func_speed, float amount_speed)
	{
		if (!stopwatches.ContainsKey(id))
		{
			stopwatches[id] = new Stopwatch();
		}
		Stopwatch stopwatch = stopwatches[id];
		stopwatch.name = name;
		stopwatch.timer.Second = Utility.Function(stopwatch.timer.Second, amount_time, func_time);
		stopwatch.timer.ElapseSpeed = Utility.Function(stopwatch.timer.ElapseSpeed, amount_speed, func_speed);
	}

	public void UnsetStopwatch(int id)
	{
		stopwatches.Remove(id);
	}

	public bool CompareStopwatch(int id, ECompare comp, double amount)
	{
		double lhs = 0.0;
		if (stopwatches.ContainsKey(id))
		{
			lhs = stopwatches[id].timer.Second;
		}
		return Utility.Compare(lhs, amount, comp);
	}

	public void Update(float deltaTime)
	{
		List<int> list = null;
		foreach (KeyValuePair<int, Stopwatch> stopwatch in stopwatches)
		{
			stopwatch.Value.timer.Update(deltaTime);
			if (stopwatch.Value.timer.Second <= 0.0)
			{
				stopwatch.Value.timer.Second = 0.0;
				if (this.OnTimeout != null)
				{
					this.OnTimeout(stopwatch.Key);
				}
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(stopwatch.Key);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				stopwatches.Remove(list[i]);
			}
		}
	}

	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		int num = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int id = r.ReadInt32();
			string name = r.ReadString();
			double amount_time = r.ReadDouble();
			float amount_speed = r.ReadSingle();
			SetStopwatch(id, name, EFunc.SetTo, amount_time, EFunc.SetTo, amount_speed);
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(0);
		w.Write(stopwatches.Count);
		foreach (KeyValuePair<int, Stopwatch> stopwatch in stopwatches)
		{
			w.Write(stopwatch.Key);
			w.Write(stopwatch.Value.name);
			w.Write(stopwatch.Value.timer.Second);
			w.Write(stopwatch.Value.timer.ElapseSpeed);
		}
	}
}
