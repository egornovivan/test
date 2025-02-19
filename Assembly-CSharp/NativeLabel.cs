using Pathea;
using PeMap;
using UnityEngine;

public class NativeLabel : TownLabel
{
	private NativeType type;

	public NativeLabel(VArtifactTown vat)
	{
		pos = vat.TransPos;
		type = vat.nativeType;
		townLabelType = TownLabelType.NativeCamp;
		if (type == NativeType.Puja)
		{
			name = PELocalization.GetString(8000737);
		}
		else
		{
			name = PELocalization.GetString(8000736);
		}
		allyId = vat.allyId;
		allyColorId = vat.AllyColorId;
		PeSingleton<LabelMgr>.Instance.Add(this);
	}

	public override int GetIcon()
	{
		if (type == NativeType.Puja)
		{
			return 19;
		}
		return 22;
	}

	public override string GetText()
	{
		return name;
	}

	public override bool FastTravel()
	{
		return false;
	}

	public static bool ContainsIcon(Vector3 pos)
	{
		ILabel label = PeSingleton<LabelMgr>.Instance.Find((ILabel item) => (item is NativeLabel nativeLabel && nativeLabel.pos == pos) ? true : false);
		if (label != null)
		{
			return true;
		}
		return false;
	}
}
