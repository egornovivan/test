using System.IO;
using UnityEngine;

namespace Pathea;

public class StateInjuredCmpt : PeCmpt
{
	private float mLevel;

	public float Level => mLevel;

	public void SetLevel(float level)
	{
		if (level < float.Epsilon)
		{
			level = 0f;
		}
		mLevel = level;
		DoInjured();
	}

	private void DoInjured()
	{
		if (!Mathf.Approximately(mLevel, 1f))
		{
		}
	}

	public override void Start()
	{
		base.Start();
		DoInjured();
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(mLevel);
	}

	public override void Deserialize(BinaryReader r)
	{
		mLevel = r.ReadSingle();
	}
}
