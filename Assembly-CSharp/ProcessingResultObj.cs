using System;
using System.IO;
using UnityEngine;

public class ProcessingResultObj : RandomItemObj, ISerializable, ISceneObjAgent, ISceneSerializableObjAgent
{
	public ProcessingResultObj()
	{
	}

	public ProcessingResultObj(Vector3 pos, int[] itemIdNum)
	{
		id = 0;
		path = "Prefab/RandomItems/item_drop";
		genPos = pos;
		position = pos;
		rotation = Quaternion.Euler(0f, new System.Random().Next(360), 0f);
		isNew = true;
		items = itemIdNum;
	}

	public ProcessingResultObj(Vector3 pos, Quaternion rot, int[] itemIdNum)
	{
		id = 0;
		path = "Prefab/RandomItems/item_drop";
		genPos = pos;
		position = pos;
		rotation = rot;
		isNew = true;
		items = itemIdNum;
	}

	public void Serialize(BinaryWriter bw)
	{
		BufferHelper.Serialize(bw, genPos);
		BufferHelper.Serialize(bw, position);
		BufferHelper.Serialize(bw, rotation);
		BufferHelper.Serialize(bw, id);
		BufferHelper.Serialize(bw, path);
		BufferHelper.Serialize(bw, isNew);
		int num = items.Length;
		BufferHelper.Serialize(bw, num);
		for (int i = 0; i < num; i++)
		{
			BufferHelper.Serialize(bw, items[i]);
		}
	}

	public void Deserialize(BinaryReader br)
	{
		BufferHelper.ReadVector3(br, out genPos);
		BufferHelper.ReadVector3(br, out position);
		BufferHelper.ReadQuaternion(br, out rotation);
		id = BufferHelper.ReadInt32(br);
		path = BufferHelper.ReadString(br);
		isNew = BufferHelper.ReadBoolean(br);
		int num = BufferHelper.ReadInt32(br);
		items = new int[num];
		for (int i = 0; i < num; i++)
		{
			items[i] = BufferHelper.ReadInt32(br);
		}
		RandomItemMgr.Instance.AddItemToManager(this);
	}

	public new void TryGenObject()
	{
		RandomItemMgr.Instance.AddItemToManager(this);
		SceneMan.AddSceneObj(this);
	}
}
