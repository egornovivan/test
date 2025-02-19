using System.IO;
using UnityEngine;

namespace Pathea;

public abstract class PeCmpt : MonoBehaviour, IPeCmpt
{
	private PeEntity mEntity;

	public PeEntity Entity
	{
		get
		{
			if (mEntity == null)
			{
				mEntity = ((!(this != null)) ? null : GetComponent<PeEntity>());
			}
			return mEntity;
		}
	}

	void IPeCmpt.Serialize(BinaryWriter w)
	{
		Serialize(w);
	}

	void IPeCmpt.Deserialize(BinaryReader r)
	{
		Deserialize(r);
	}

	string IPeCmpt.GetTypeName()
	{
		return GetType().Name;
	}

	public virtual void Serialize(BinaryWriter w)
	{
	}

	public virtual void Deserialize(BinaryReader r)
	{
	}

	public virtual void Awake()
	{
		if (this is IPeMsg)
		{
			Entity.AddMsgListener(this as IPeMsg);
		}
	}

	public virtual void Start()
	{
		PeSingleton<CmptMgr>.Instance.AddCmpt(this);
	}

	public virtual void OnUpdate()
	{
	}

	public virtual void OnDestroy()
	{
		if (null != Entity && this is IPeMsg)
		{
			Entity.RemoveMsgListener(this as IPeMsg);
		}
	}
}
