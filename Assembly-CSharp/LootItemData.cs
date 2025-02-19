using System.IO;
using ItemAsset;
using Pathea;
using UnityEngine;

public class LootItemData
{
	public int id;

	public Vector3 position;

	public double dropTime;

	public float checkItemExistTime;

	private ItemObject m_ItemObj;

	public ItemObject itemObj
	{
		get
		{
			if (m_ItemObj == null)
			{
				m_ItemObj = PeSingleton<ItemMgr>.Instance.Get(id);
			}
			return m_ItemObj;
		}
	}

	public byte[] Export()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter w = new BinaryWriter(memoryStream);
		Export(w);
		return memoryStream.ToArray();
	}

	public void Import(byte[] data)
	{
		MemoryStream memoryStream = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		Import(binaryReader);
		binaryReader.Close();
		memoryStream.Close();
	}

	public void Export(BinaryWriter w)
	{
		w.Write(id);
		w.Write(position.x);
		w.Write(position.y);
		w.Write(position.z);
		w.Write(dropTime);
	}

	public void Import(BinaryReader _in)
	{
		id = _in.ReadInt32();
		position = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
		dropTime = _in.ReadDouble();
	}
}
