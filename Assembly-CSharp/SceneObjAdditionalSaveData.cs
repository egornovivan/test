using System.Collections.Generic;
using System.IO;
using PETools;
using UnityEngine;

public class SceneObjAdditionalSaveData : ISerializable
{
	private Dictionary<int, byte[]> _datas = new Dictionary<int, byte[]>();

	public void Serialize(BinaryWriter bw)
	{
		int count = _datas.Count;
		bw.Write(count);
		foreach (KeyValuePair<int, byte[]> data in _datas)
		{
			bw.Write(data.Key);
			PETools.Serialize.WriteBytes(data.Value, bw);
		}
	}

	public void Deserialize(BinaryReader br)
	{
		_datas.Clear();
		int num = br.ReadInt32();
		while (num-- > 0)
		{
			int key = br.ReadInt32();
			_datas[key] = PETools.Serialize.ReadBytes(br);
		}
	}

	public void CollectData(GameObject rootGo)
	{
		if (!(rootGo != null))
		{
			return;
		}
		MonoBehaviour[] componentsInChildren = rootGo.GetComponentsInChildren<MonoBehaviour>();
		MonoBehaviour[] array = componentsInChildren;
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (monoBehaviour is ISaveDataInScene saveDataInScene)
			{
				string text = monoBehaviour.gameObject.name + saveDataInScene.GetType().Name;
				int hashCode = text.GetHashCode();
				_datas[hashCode] = saveDataInScene.ExportData();
			}
		}
	}

	public void DispatchData(GameObject rootGo)
	{
		if (!(rootGo != null))
		{
			return;
		}
		MonoBehaviour[] componentsInChildren = rootGo.GetComponentsInChildren<MonoBehaviour>();
		MonoBehaviour[] array = componentsInChildren;
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (monoBehaviour is ISaveDataInScene saveDataInScene)
			{
				string text = monoBehaviour.gameObject.name + saveDataInScene.GetType().Name;
				int hashCode = text.GetHashCode();
				if (_datas.TryGetValue(hashCode, out var value))
				{
					saveDataInScene.ImportData(value);
				}
			}
		}
	}
}
