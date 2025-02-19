using Pathea;
using PeMap;
using UnityEngine;

public class ColonyLabel : TownLabel
{
	public ColonyLabel(Vector3 pos)
	{
		base.pos = pos;
		name = PELocalization.GetString(8000735);
		townLabelType = TownLabelType.Colony;
		PeSingleton<LabelMgr>.Instance.Add(this);
	}

	public override int GetIcon()
	{
		return 16;
	}

	public override bool FastTravel()
	{
		return true;
	}

	public new static bool Remove(Vector3 pos)
	{
		ILabel label = PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is ColonyLabel colonyLabel && colonyLabel.pos == pos) ? true : false);
		if (label != null)
		{
			return PeSingleton<LabelMgr>.Instance.Remove(label);
		}
		return false;
	}

	public static bool ContainsIcon(Vector3 pos)
	{
		ILabel label = PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is ColonyLabel colonyLabel && colonyLabel.pos == pos) ? true : false);
		if (label != null)
		{
			return true;
		}
		return false;
	}
}
