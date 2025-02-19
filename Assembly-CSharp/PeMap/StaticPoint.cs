using System.IO;
using Pathea;
using PETools;
using UnityEngine;

namespace PeMap;

public class StaticPoint : ISerializable, ILabel
{
	public class Mgr : MyListArchiveSingleton<Mgr, StaticPoint>
	{
		public void Tick(Vector3 observerPos)
		{
			ForEach(delegate(StaticPoint item)
			{
				if (!item.discoverd && Vector3.Distance(observerPos, item.position) < item.distance)
				{
					item.discoverd = true;
					item.AddToLabelMgr();
					if (PeGameMgr.IsMultiStory)
					{
						PlayerNetwork.mainPlayer.RequestAddFountMapLable(item.ID);
					}
				}
			});
		}

		public void UnveilAll()
		{
			ForEach(delegate(StaticPoint item)
			{
				if (!item.discoverd)
				{
					item.discoverd = true;
					item.AddToLabelMgr();
				}
			});
		}

		public int GetMapSoundID(Vector3 position)
		{
			int soundID = 0;
			ForEach(delegate(StaticPoint item)
			{
				if (Vector3.Distance(position, item.position) <= item.distance && item.soundID > 0)
				{
					soundID = item.soundID;
				}
			});
			return soundID;
		}
	}

	public int ID;

	public int soundID;

	public int campId = -1;

	public float distance = 100f;

	public string text = "default";

	public int textId = -1;

	public int icon = 10;

	public bool fastTravel = true;

	public Vector3 position;

	private bool discoverd;

	int ILabel.GetIcon()
	{
		return icon;
	}

	Vector3 ILabel.GetPos()
	{
		return position;
	}

	string ILabel.GetText()
	{
		if (textId != -1)
		{
			return PELocalization.GetString(textId);
		}
		return text;
	}

	bool ILabel.FastTravel()
	{
		return fastTravel;
	}

	ELabelType ILabel.GetType()
	{
		if (fastTravel)
		{
			return ELabelType.FastTravel;
		}
		return ELabelType.Mark;
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
		return EShow.All;
	}

	byte[] ISerializable.Serialize()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			Serialize.WriteVector3(w, position);
			w.Write(distance);
			w.Write(textId);
			w.Write(text);
			w.Write(icon);
			w.Write(fastTravel);
			w.Write(discoverd);
			w.Write(campId);
			w.Write(ID);
		});
	}

	void ISerializable.Deserialize(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			position = Serialize.ReadVector3(r);
			distance = r.ReadSingle();
			textId = r.ReadInt32();
			text = r.ReadString();
			icon = r.ReadInt32();
			fastTravel = r.ReadBoolean();
			discoverd = r.ReadBoolean();
			if (PeSingleton<ArchiveMgr>.Instance.GetCurArvhiveVersion() >= 2)
			{
				campId = r.ReadInt32();
			}
			else
			{
				campId = StoryStaticPoint.GetCamp(textId);
			}
			if (PeSingleton<ArchiveMgr>.Instance.GetCurArvhiveVersion() >= 5)
			{
				ID = r.ReadInt32();
			}
			else
			{
				ID = StoryStaticPoint.GetIDByNameID(textId);
			}
		});
		if (discoverd)
		{
			AddToLabelMgr();
		}
	}

	private void AddToLabelMgr()
	{
		PeSingleton<LabelMgr>.Instance.Add(this);
	}

	public static void StaticPointBeFound(int id)
	{
		StaticPoint staticPoint = PeSingleton<Mgr>.Instance.Find((StaticPoint sp) => (sp.ID == id) ? true : false);
		if (staticPoint != null && !staticPoint.discoverd)
		{
			staticPoint.discoverd = true;
			staticPoint.AddToLabelMgr();
		}
	}

	public bool CompareTo(ILabel label)
	{
		if (label is StaticPoint)
		{
			StaticPoint staticPoint = (StaticPoint)label;
			if (ID == staticPoint.ID)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
