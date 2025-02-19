using System.IO;
using Pathea;
using PETools;
using UnityEngine;

namespace PeMap;

public class UserLabel : ISerializable, ILabel
{
	public class Mgr : ArchivableLabelMgr<Mgr, UserLabel>
	{
		protected override string GetArchiveKey()
		{
			return "ArchiveKeyMapUserLabel";
		}
	}

	public Vector3 pos;

	public string text;

	public int icon;

	public int playerID;

	public byte index;

	int ILabel.GetIcon()
	{
		return icon;
	}

	Vector3 ILabel.GetPos()
	{
		return pos;
	}

	string ILabel.GetText()
	{
		return text;
	}

	bool ILabel.FastTravel()
	{
		return false;
	}

	ELabelType ILabel.GetType()
	{
		return ELabelType.User;
	}

	bool ILabel.NeedArrow()
	{
		return false;
	}

	float ILabel.GetRadius()
	{
		return -1f;
	}

	EShow ILabel.GetShow()
	{
		return EShow.BigMap;
	}

	byte[] ISerializable.Serialize()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			Serialize.WriteVector3(w, pos);
			w.Write(text);
			w.Write(icon);
			w.Write(index);
		});
	}

	void ISerializable.Deserialize(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			pos = Serialize.ReadVector3(r);
			text = r.ReadString();
			icon = r.ReadInt32();
			if (PeSingleton<ArchiveMgr>.Instance.GetCurArvhiveVersion() >= 5)
			{
				index = r.ReadByte();
			}
		});
	}

	public bool CompareTo(ILabel label)
	{
		if (label is UserLabel)
		{
			UserLabel userLabel = (UserLabel)label;
			if (pos == userLabel.pos)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
