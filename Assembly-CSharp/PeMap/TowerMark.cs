using System.IO;
using Pathea;
using PETools;
using UnityEngine;

namespace PeMap;

public class TowerMark : ISerializable, ILabel
{
	public class Mgr : MyListArchiveSingleton<Mgr, TowerMark>
	{
	}

	public int ID;

	public int campId = -1;

	public string text = "default";

	public int icon = 8;

	public Vector3 position;

	ELabelType ILabel.GetType()
	{
		return ELabelType.Mark;
	}

	public int GetIcon()
	{
		return icon;
	}

	public Vector3 GetPos()
	{
		return position;
	}

	public string GetText()
	{
		return text;
	}

	public bool FastTravel()
	{
		return false;
	}

	public bool NeedArrow()
	{
		return false;
	}

	public float GetRadius()
	{
		return -1f;
	}

	public EShow GetShow()
	{
		return EShow.All;
	}

	public bool CompareTo(ILabel label)
	{
		return label is TowerMark towerMark && towerMark.ID == ID;
	}

	public byte[] Serialize()
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(ID);
			PETools.Serialize.WriteVector3(w, position);
			w.Write(campId);
			w.Write(icon);
			w.Write(text);
		});
	}

	public void Deserialize(byte[] data)
	{
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			ID = r.ReadInt32();
			position = PETools.Serialize.ReadVector3(r);
			campId = r.ReadInt32();
			icon = r.ReadInt32();
			text = r.ReadString();
		});
		PeSingleton<LabelMgr>.Instance.Add(this);
	}
}
