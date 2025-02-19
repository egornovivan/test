using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PETools;
using UnityEngine;

namespace Pathea;

public class PeCreature : ArchivableSingleton<PeCreature>
{
	public class EntityList : IEnumerable, IEnumerable<int>
	{
		private List<int> mList;

		public EntityList(int capacity)
		{
			mList = new List<int>(capacity);
		}

		IEnumerator<int> IEnumerable<int>.GetEnumerator()
		{
			return mList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mList.GetEnumerator();
		}

		public bool Contains(int id)
		{
			return mList.Contains(id);
		}

		public void Add(int id)
		{
			mList.Add(id);
		}

		public int GetIdByIndex(int index)
		{
			if (index < 0 || index >= mList.Count)
			{
				return -1;
			}
			return mList[index];
		}

		public bool Remove(int id)
		{
			return mList.Remove(id);
		}

		public void Clear()
		{
			foreach (int m in mList)
			{
				PeSingleton<EntityMgr>.Instance.Remove(m);
			}
			mList.Clear();
		}

		public void Export(BinaryWriter bw)
		{
			bw.Write(0);
			bw.Write(mList.Count);
			for (int i = 0; i < mList.Count; i++)
			{
				WriteEntity(bw, mList[i]);
			}
		}

		public static void WriteEntity(BinaryWriter w, int id)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(id);
			if (null != peEntity)
			{
				w.Write(id);
				w.Write(peEntity.prefabPath);
				Serialize.WriteData(peEntity.Export, w);
			}
			else
			{
				w.Write(id);
				w.Write(string.Empty);
				Serialize.WriteData(null, w);
				Debug.LogError("cant find peentity with id:" + id);
			}
		}

		public static int ReadEntity(BinaryReader r)
		{
			int num = r.ReadInt32();
			string text = r.ReadString();
			byte[] buffer = Serialize.ReadBytes(r);
			if (!string.IsNullOrEmpty(text))
			{
				PeEntity peEntity = Create(num, text, Vector3.zero, Quaternion.identity, Vector3.one);
				peEntity.Import(buffer);
			}
			return num;
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
				int id = ReadEntity(binaryReader);
				Add(id);
			}
		}

		protected static PeEntity Create(int id, string path, Vector3 pos, Quaternion rot, Vector3 scl)
		{
			return PeSingleton<EntityMgr>.Instance.Create(id, path, pos, rot, scl);
		}
	}

	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	private const string ArchiveKeyCreature = "ArchiveKeyCreature";

	private EntityList mList = new EntityList(100);

	private int mMainPlayerId = -1;

	public Action<int> destoryEntityEvent;

	public PeEntity mainPlayer => PeSingleton<MainPlayer>.Instance.entity;

	public int mainPlayerId => PeSingleton<MainPlayer>.Instance.entityId;

	protected override bool GetYird()
	{
		return false;
	}

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyCreature";
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

	public void Export(BinaryWriter bw)
	{
		bw.Write(0);
		Serialize.WriteData(mList.Export, bw);
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
			byte[] buffer2 = Serialize.ReadBytes(r);
			mList.Import(buffer2);
		});
	}

	public PeEntity CreateDoodad(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		if (PeSingleton<WorldInfoMgr>.Instance.IsNonRecordAutoId(id))
		{
			throw new Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
		}
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateDoodad(id, protoId, pos, rot, scl);
		if (peEntity == null)
		{
			return null;
		}
		mList.Add(id);
		return peEntity;
	}

	public PeEntity CreateMonster(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl, float exScale = 1f)
	{
		if (PeSingleton<WorldInfoMgr>.Instance.IsNonRecordAutoId(id))
		{
			throw new Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
		}
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateMonster(id, protoId, pos, rot, scl, exScale);
		if (peEntity == null)
		{
			return null;
		}
		mList.Add(id);
		return peEntity;
	}

	public PeEntity CreateNpc(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		if (PeSingleton<WorldInfoMgr>.Instance.IsNonRecordAutoId(id))
		{
			throw new Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
		}
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateNpc(id, protoId, pos, rot, scl);
		if (peEntity == null)
		{
			return null;
		}
		PeEntity peEntity2 = PeSingleton<PeEntityCreator>.Instance.CreateNpcRobot(id, protoId, pos, rot, scl);
		if (peEntity2 != null)
		{
			mList.Add(peEntity2.Id);
		}
		mList.Add(id);
		return peEntity;
	}

	public PeEntity CreateRandomNpc(int templateId, int id, Vector3 pos, Quaternion rot, Vector3 scl)
	{
		if (PeSingleton<WorldInfoMgr>.Instance.IsNonRecordAutoId(id))
		{
			throw new Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
		}
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreateRandomNpc(templateId, id, pos, rot, scl);
		if (null == peEntity)
		{
			return null;
		}
		mList.Add(id);
		return peEntity;
	}

	public bool Destory(int id)
	{
		mList.Remove(id);
		if (destoryEntityEvent != null)
		{
			destoryEntityEvent(id);
		}
		return PeSingleton<EntityMgr>.Instance.Destroy(id);
	}

	public void Add(int id)
	{
		if (!mList.Contains(id))
		{
			mList.Add(id);
		}
	}

	public bool Remove(int id)
	{
		return mList.Remove(id);
	}
}
