using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;
using PETools;
using UnityEngine;

namespace PeCustom;

public class CreatureMgr : ArchivableSingleton<CreatureMgr>
{
	public struct DataStrc
	{
		public int id;

		public string path;

		public byte[] datas;
	}

	public class EntityList : IEnumerable, IEnumerable<DataStrc>
	{
		private Dictionary<int, DataStrc> mDatas;

		public EntityList(int capacity)
		{
			mDatas = new Dictionary<int, DataStrc>(capacity);
		}

		IEnumerator<DataStrc> IEnumerable<DataStrc>.GetEnumerator()
		{
			return mDatas.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mDatas.GetEnumerator();
		}

		public void Add(int id)
		{
			DataStrc value = default(DataStrc);
			value.id = id;
			value.path = string.Empty;
			mDatas.Add(id, value);
		}

		public bool Remove(int id)
		{
			return mDatas.Remove(id);
		}

		public bool Contain(int id)
		{
			return mDatas.ContainsKey(id);
		}

		public bool SetDataToEntity(PeEntity e)
		{
			if (mDatas.ContainsKey(e.Id))
			{
				e.Import(mDatas[e.Id].datas);
				return true;
			}
			return false;
		}

		public bool SetEntityToData(PeEntity e)
		{
			if (mDatas.ContainsKey(e.Id))
			{
				DataStrc value = mDatas[e.Id];
				value.datas = e.Export();
				mDatas[e.Id] = value;
				return true;
			}
			return false;
		}

		public void Clear()
		{
			foreach (KeyValuePair<int, DataStrc> mData in mDatas)
			{
				PeSingleton<EntityMgr>.Instance.Remove(mData.Value.id);
			}
			mDatas.Clear();
		}

		public void CreateAllEntity()
		{
		}

		public byte[] Export()
		{
			try
			{
				using MemoryStream memoryStream = new MemoryStream();
				using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(0);
				binaryWriter.Write(mDatas.Count);
				List<DataStrc> list = new List<DataStrc>(10);
				foreach (KeyValuePair<int, DataStrc> mData in mDatas)
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(mData.Key);
					if (peEntity == null)
					{
						if (mData.Value.datas != null)
						{
							binaryWriter.Write(mData.Key);
							binaryWriter.Write(mData.Value.path);
							Serialize.WriteBytes(mData.Value.datas, binaryWriter);
						}
						else
						{
							binaryWriter.Write(-1);
							Debug.LogError("cant find peentity with id:" + mData.Key);
						}
						continue;
					}
					DataStrc item = default(DataStrc);
					item.path = peEntity.prefabPath;
					item.id = peEntity.Id;
					item.datas = peEntity.Export();
					if (item.datas != null)
					{
						list.Add(item);
						binaryWriter.Write(mData.Key);
						binaryWriter.Write(peEntity.prefabPath);
						Serialize.WriteBytes(item.datas, binaryWriter);
					}
					else
					{
						Debug.LogError("cant find peentity with id:" + mData.Key);
					}
				}
				for (int i = 0; i < list.Count; i++)
				{
					mDatas[list[i].id] = list[i];
				}
				return memoryStream.ToArray();
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
			return null;
		}

		public void Import(byte[] buffer)
		{
			if (buffer == null || buffer.Length == 0)
			{
				return;
			}
			using MemoryStream input = new MemoryStream(buffer, writable: false);
			using BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			if (num > 0)
			{
				Debug.LogError("error version:" + num);
			}
			int num2 = binaryReader.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				int num3 = binaryReader.ReadInt32();
				if (num3 != -1)
				{
					DataStrc value = default(DataStrc);
					value.id = num3;
					value.path = binaryReader.ReadString();
					value.datas = Serialize.ReadBytes(binaryReader);
					mDatas.Add(value.id, value);
				}
			}
		}

		protected static PeEntity Create(int id, string path, Vector3 pos, Quaternion rot, Vector3 scl)
		{
			return PeSingleton<EntityMgr>.Instance.Create(id, path, pos, rot, scl);
		}
	}

	private const int VERSION_0000 = 0;

	private const int VERSION_0001 = 1;

	private const int CURRENT_VERSION = 0;

	private const string ArchiveKeyCreature = "ArchiveKeyCustomCreature";

	private Dictionary<int, EntityList> mEntsMap = new Dictionary<int, EntityList>();

	private int mMainPlayerId = -1;

	public PeEntity mainPlayer => PeSingleton<MainPlayer>.Instance.entity;

	public int mainPlayerId => PeSingleton<MainPlayer>.Instance.entityId;

	protected override bool GetYird()
	{
		return false;
	}

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyCustomCreature";
	}

	protected override void SetData(byte[] data)
	{
		if (data != null)
		{
			Import(data);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		Export(bw);
	}

	public void Export(BinaryWriter w)
	{
		w.Write(0);
		w.Write(mEntsMap.Count);
		foreach (KeyValuePair<int, EntityList> item in mEntsMap)
		{
			w.Write(item.Key);
			Serialize.WriteBytes(item.Value.Export(), w);
		}
	}

	public void Import(byte[] buffer)
	{
		Serialize.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num > 0)
			{
				Debug.LogError("error version:" + num);
			}
			else
			{
				int num2 = r.ReadInt32();
				for (int i = 0; i < num2; i++)
				{
					int key = r.ReadInt32();
					byte[] buffer2 = Serialize.ReadBytes(r);
					EntityList entityList = new EntityList(200);
					entityList.Import(buffer2);
					mEntsMap.Add(key, entityList);
				}
			}
		});
	}

	public PeEntity CreateDoodad(int world_index, int id, int protoId)
	{
		if (world_index == -1)
		{
			throw new Exception("world index:" + world_index + " is invalid world index, can not used here.");
		}
		if (PeSingleton<WorldInfoMgr>.Instance.IsNonRecordAutoId(id))
		{
			throw new Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
		}
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateDoodad(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
		if (peEntity == null)
		{
			return null;
		}
		if (mEntsMap.ContainsKey(world_index) && mEntsMap[world_index].Contain(id))
		{
			mEntsMap[world_index].SetDataToEntity(peEntity);
		}
		else
		{
			AddEntity(world_index, id);
		}
		return peEntity;
	}

	public PeEntity CreateMonster(int world_index, int id, int protoId)
	{
		if (world_index == -1)
		{
			throw new Exception("world index:" + world_index + " is invalid world index, can not used here.");
		}
		if (PeSingleton<WorldInfoMgr>.Instance.IsNonRecordAutoId(id))
		{
			throw new Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
		}
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateMonster(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
		if (peEntity == null)
		{
			return null;
		}
		if (mEntsMap.ContainsKey(world_index) && mEntsMap[world_index].Contain(id))
		{
			mEntsMap[world_index].SetDataToEntity(peEntity);
		}
		else
		{
			AddEntity(world_index, id);
		}
		return peEntity;
	}

	public PeEntity CreateNpc(int world_index, int id, int protoId)
	{
		if (PeSingleton<WorldInfoMgr>.Instance.IsNonRecordAutoId(id))
		{
			throw new Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
		}
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateNpc(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
		if (peEntity == null)
		{
			return null;
		}
		if (mEntsMap.ContainsKey(world_index) && mEntsMap[world_index].Contain(id))
		{
			mEntsMap[world_index].SetDataToEntity(peEntity);
		}
		else
		{
			AddEntity(world_index, id);
		}
		return peEntity;
	}

	public PeEntity CreateRandomNpc(int world_index, int templateId, int id)
	{
		if (PeSingleton<WorldInfoMgr>.Instance.IsNonRecordAutoId(id))
		{
			throw new Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
		}
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateRandomNpc(templateId, id, Vector3.zero, Quaternion.identity, Vector3.one);
		if (null == peEntity)
		{
			return null;
		}
		AddEntity(world_index, id);
		return peEntity;
	}

	private void AddEntity(int world_index, int entity_id)
	{
		if (mEntsMap.ContainsKey(world_index))
		{
			mEntsMap[world_index].Add(entity_id);
			return;
		}
		EntityList entityList = new EntityList(200);
		entityList.Add(entity_id);
		mEntsMap.Add(world_index, entityList);
	}

	public bool Destory(int id)
	{
		using (Dictionary<int, EntityList>.Enumerator enumerator = mEntsMap.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(id))
			{
			}
		}
		return PeSingleton<EntityMgr>.Instance.Destroy(id);
	}

	public bool DestroyAndDontRemove(int id)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(id);
		if (peEntity == null)
		{
			return false;
		}
		using (Dictionary<int, EntityList>.Enumerator enumerator = mEntsMap.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.Value.SetEntityToData(peEntity))
			{
			}
		}
		return PeSingleton<EntityMgr>.Instance.Destroy(id);
	}

	public void OnPeCreatureDestroyEntity(int id)
	{
		using Dictionary<int, EntityList>.Enumerator enumerator = mEntsMap.GetEnumerator();
		while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(id))
		{
		}
	}
}
