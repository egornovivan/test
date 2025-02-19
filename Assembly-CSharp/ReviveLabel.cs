using System.IO;
using Pathea;
using PeMap;
using PETools;
using UnityEngine;

public class ReviveLabel : PeMap.ISerializable, ILabel
{
	public class Mgr : ArchivableLabelMgr<Mgr, ReviveLabel>
	{
		protected override string GetArchiveKey()
		{
			return "ArchivableLabelMgr";
		}

		public override void Add(ReviveLabel item)
		{
			ReviveLabel reviveLabel = Find((ReviveLabel e) => true);
			if (reviveLabel != null)
			{
				Remove(reviveLabel);
			}
			base.Add(item);
		}
	}

	public Vector3 pos;

	byte[] PeMap.ISerializable.Serialize()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			Serialize.WriteVector3(w, pos);
		});
	}

	void PeMap.ISerializable.Deserialize(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			pos = Serialize.ReadVector3(r);
		});
		PeSingleton<LabelMgr>.Instance.Add(this);
	}

	public int GetIcon()
	{
		return 43;
	}

	public Vector3 GetPos()
	{
		return pos;
	}

	public string GetText()
	{
		return PELocalization.GetString(8000182);
	}

	public bool FastTravel()
	{
		return false;
	}

	public new ELabelType GetType()
	{
		return ELabelType.Revive;
	}

	public bool NeedArrow()
	{
		return true;
	}

	public float GetRadius()
	{
		return 20f;
	}

	public EShow GetShow()
	{
		return EShow.All;
	}

	public bool CompareTo(ILabel label)
	{
		if (label is ReviveLabel)
		{
			ReviveLabel reviveLabel = (ReviveLabel)label;
			if (pos == reviveLabel.pos)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
