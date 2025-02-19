using System.IO;
using CustomCharactor;
using PeEvent;
using PETools;
using UnityEngine;

namespace Pathea;

public class MainPlayer : ArchivableSingleton<MainPlayer>
{
	public class MainPlayerCreatedArg : EventArg
	{
		public int id;
	}

	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	public Event<MainPlayerCreatedArg> mainPlayerCreatedEventor = new Event<MainPlayerCreatedArg>();

	private int mMainPlayerId = -1;

	private PeEntity mEntity;

	public int entityId => mMainPlayerId;

	public PeEntity entity
	{
		get
		{
			if (null == mEntity)
			{
				mEntity = PeSingleton<EntityMgr>.Instance.Get(mMainPlayerId);
			}
			return mEntity;
		}
	}

	protected override bool GetYird()
	{
		return false;
	}

	public void SetEntityId(int id)
	{
		mMainPlayerId = id;
		MainPlayerCreatedArg mainPlayerCreatedArg = new MainPlayerCreatedArg();
		mainPlayerCreatedArg.id = id;
		mainPlayerCreatedEventor.Dispatch(mainPlayerCreatedArg);
	}

	protected override void WriteData(BinaryWriter w)
	{
		w.Write(0);
		PeCreature.EntityList.WriteEntity(w, mMainPlayerId);
	}

	protected override void SetData(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num > 0)
			{
				Debug.LogError("error version:" + num);
			}
			int num2 = PeCreature.EntityList.ReadEntity(r);
			SetEntityId(num2);
		});
	}

	public PeEntity CreatePlayer(int id, Vector3 pos, Quaternion rot, Vector3 scl, CustomCharactor.CustomData data = null)
	{
		PeEntity peEntity = PeSingleton<PeEntityCreator>.Instance.CreatePlayer(id, pos, rot, scl, data);
		if (null == peEntity)
		{
			return null;
		}
		SetEntityId(id);
		return peEntity;
	}
}
