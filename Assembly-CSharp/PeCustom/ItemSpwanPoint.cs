using System.IO;

namespace PeCustom;

public class ItemSpwanPoint : SpawnPoint
{
	public int ItemObjId;

	public bool CanPickup;

	public bool isNew = true;

	public DragArticleAgent agent;

	public ItemSpwanPoint()
	{
	}

	public ItemSpwanPoint(WEItem obj)
		: base(obj)
	{
		ItemObjId = -1;
		CanPickup = obj.CanPickup;
	}

	public ItemSpwanPoint(EffectSpwanPoint sp)
		: base(sp)
	{
	}

	public override void Serialize(BinaryWriter bw)
	{
		base.Serialize(bw);
		bw.Write(ItemObjId);
		bw.Write(CanPickup);
		bw.Write(isNew);
	}

	public override void Deserialize(int version, BinaryReader br)
	{
		base.Deserialize(version, br);
		switch (version)
		{
		case 1:
			ItemObjId = br.ReadInt32();
			CanPickup = br.ReadBoolean();
			break;
		case 2:
		case 3:
		case 4:
			ItemObjId = br.ReadInt32();
			CanPickup = br.ReadBoolean();
			isNew = br.ReadBoolean();
			break;
		}
	}
}
