using Pathea;
using PeMap;
using UnityEngine;

public class UnknownLabel : TownLabel
{
	public UnknownLabel(Vector3 pos)
	{
		base.pos = pos;
		name = PELocalization.GetString(8000695);
		townLabelType = TownLabelType.Unknown;
		PeSingleton<LabelMgr>.Instance.Add(this);
	}

	public override int GetIcon()
	{
		return 26;
	}

	public override bool FastTravel()
	{
		return false;
	}

	public new static void Remove(Vector3 pos)
	{
		ILabel label = PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is UnknownLabel unknownLabel && unknownLabel.pos == pos) ? true : false);
		if (label != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(label);
		}
	}

	public static bool HasMark(Vector3 pos)
	{
		return null != PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is UnknownLabel unknownLabel && unknownLabel.pos == pos) ? true : false);
	}

	public static void AddMark(Vector3 pos)
	{
		new UnknownLabel(pos);
	}
}
