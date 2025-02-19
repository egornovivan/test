using System.Collections.Generic;
using Pathea;
using SkillAsset;
using SkillSystem;
using uLink;
using UnityEngine;

public abstract class CommonInterface : UnityEngine.MonoBehaviour, ISkillTarget
{
	private NetworkInterface netlayer;

	private SkAliveEntity _skEntityPE;

	private SkEntity _skEntity;

	private static Dictionary<int, CommonInterface> _comMgr = new Dictionary<int, CommonInterface>();

	internal bool IsMultiMine => !(null == Netlayer) && Netlayer.IsOwner;

	internal bool IsMultiProxy => !(null == Netlayer) && Netlayer.IsProxy;

	public virtual bool IsOwner => !(null == Netlayer) && Netlayer.IsOwner;

	public virtual bool IsController => !(null == Netlayer) && Netlayer.hasOwnerAuth;

	public NetworkInterface Netlayer => netlayer;

	public int TeamId => (!(null == Netlayer)) ? Netlayer.TeamId : 0;

	internal virtual uLink.NetworkView OwnerView => (!(null == Netlayer)) ? Netlayer.OwnerView : null;

	public SkAliveEntity SkEntityPE => _skEntityPE;

	public SkEntity SkEntityBase => _skEntity;

	public static Dictionary<int, CommonInterface> ComMgr => _comMgr;

	public virtual ESkillTargetType GetTargetType()
	{
		return ESkillTargetType.TYPE_SkillRunner;
	}

	public virtual Vector3 GetPosition()
	{
		return Vector3.zero;
	}

	internal virtual void InitNetworkLayer(NetworkInterface network, GameObject obj = null)
	{
		if (obj == null)
		{
			obj = base.gameObject;
		}
		netlayer = network;
		if (null == network)
		{
			return;
		}
		_comMgr[network.Id] = this;
		if (obj != null)
		{
			_skEntity = obj.GetComponent<SkEntity>();
			if (_skEntity != null)
			{
				_skEntity.SetNet(network);
			}
			_skEntityPE = obj.GetComponent<SkAliveEntity>();
		}
	}

	public static CommonInterface GetComByNetID(int id)
	{
		if (_comMgr.ContainsKey(id))
		{
			return _comMgr[id];
		}
		return null;
	}

	internal virtual void RPCServer(EPacketType type, params object[] objs)
	{
		if (null != Netlayer)
		{
			Netlayer.RPCServer(type, objs);
		}
	}

	internal virtual void URPCServer(EPacketType type, params object[] objs)
	{
		if (null != Netlayer)
		{
			Netlayer.URPCServer(type, objs);
		}
	}

	public virtual void NetworkApplyDamage(CommonInterface caster, float damage, int lifeLeft)
	{
	}

	public virtual void NetworkAiDeath(CommonInterface caster)
	{
	}
}
