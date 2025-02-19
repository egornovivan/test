using Pathea;
using PeMap;
using UnityEngine;

public class NpcTownLabel : TownLabel
{
	public NpcTownLabel(VArtifactTown vat)
	{
		pos = vat.TransPos;
		townLabelType = TownLabelType.NpcTown;
		name = PELocalization.GetString(vat.townNameId);
		allyId = vat.allyId;
		allyColorId = vat.AllyColorId;
		PeSingleton<LabelMgr>.Instance.Add(this);
	}

	public override int GetIcon()
	{
		return 27;
	}

	public override bool FastTravel()
	{
		return allyId == 0;
	}

	public static bool ContainsIcon(Vector3 pos)
	{
		ILabel label = PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is NpcTownLabel npcTownLabel && npcTownLabel.pos == pos) ? true : false);
		if (label != null)
		{
			return true;
		}
		return false;
	}
}
