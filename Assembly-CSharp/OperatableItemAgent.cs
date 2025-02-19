using System;
using System.IO;
using PETools;
using UnityEngine;

public class OperatableItemAgent : ISerializable, ISceneObjAgent, ISceneSerializableObjAgent
{
	private int mId;

	private Vector3 mPos;

	public string mPrefabPath;

	private int mAgentId;

	private OperatableItem mOperatableItem;

	int ISceneObjAgent.Id
	{
		get
		{
			return mAgentId;
		}
		set
		{
			mAgentId = value;
		}
	}

	GameObject ISceneObjAgent.Go
	{
		get
		{
			if (null == mOperatableItem)
			{
				return null;
			}
			return mOperatableItem.gameObject;
		}
	}

	Vector3 ISceneObjAgent.Pos => mPos;

	IBoundInScene ISceneObjAgent.Bound => null;

	bool ISceneObjAgent.NeedToActivate => false;

	bool ISceneObjAgent.TstYOnActivate => false;

	public int ScenarioId { get; set; }

	public OperatableItemAgent()
	{
	}

	public OperatableItemAgent(int id, Vector3 pos, string prefabPath)
	{
		mId = id;
		mPos = pos;
		mPrefabPath = prefabPath;
	}

	void ISceneObjAgent.OnConstruct()
	{
		Create();
	}

	void ISceneObjAgent.OnActivate()
	{
		throw new NotImplementedException();
	}

	void ISceneObjAgent.OnDeactivate()
	{
		throw new NotImplementedException();
	}

	void ISceneObjAgent.OnDestruct()
	{
		Destory();
	}

	void ISerializable.Serialize(BinaryWriter bw)
	{
		bw.Write(mId);
		Serialize.WriteVector3(bw, mPos);
		Serialize.WriteNullableString(bw, mPrefabPath);
	}

	void ISerializable.Deserialize(BinaryReader br)
	{
		mId = br.ReadInt32();
		mPos = Serialize.ReadVector3(br);
		mPrefabPath = Serialize.ReadNullableString(br);
	}

	private void Create()
	{
		GameObject gameObject = Resources.Load(mPrefabPath) as GameObject;
		if (null == gameObject)
		{
			Debug.LogError("Operatable item load failed.");
			return;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, mPos, Quaternion.identity) as GameObject;
		if (!(null == gameObject2))
		{
			mOperatableItem = gameObject2.GetComponent<OperatableItem>();
			if (null == mOperatableItem || !mOperatableItem.Init(mId))
			{
				Debug.LogError("Operatable item load failed.");
				UnityEngine.Object.Destroy(gameObject2);
				mOperatableItem = null;
			}
		}
	}

	private void Destory()
	{
		if (mOperatableItem != null)
		{
			UnityEngine.Object.Destroy(mOperatableItem.gameObject);
			mOperatableItem = null;
		}
	}
}
