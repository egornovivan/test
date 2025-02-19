using Pathea;
using PeMap;
using UnityEngine;

public class DestroyedLabel : TownLabel
{
	private VArtifactType townType;

	private NativeType nativeType;

	public DestroyedLabel(VArtifactTown vat)
	{
		pos = vat.TransPos;
		name = PELocalization.GetString(vat.townNameId);
		townType = vat.type;
		nativeType = vat.nativeType;
		allyId = vat.allyId;
		allyColorId = vat.AllyColorId;
		PeSingleton<LabelMgr>.Instance.Add(this);
	}

	public override int GetIcon()
	{
		if (townType == VArtifactType.NpcTown)
		{
			return 46;
		}
		if (nativeType == NativeType.Puja)
		{
			return 48;
		}
		return 47;
	}

	public override bool FastTravel()
	{
		return true;
	}

	public static bool ContainsIcon(Vector3 pos)
	{
		ILabel label = PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is DestroyedLabel destroyedLabel && destroyedLabel.pos == pos) ? true : false);
		if (label != null)
		{
			return true;
		}
		return false;
	}
}
