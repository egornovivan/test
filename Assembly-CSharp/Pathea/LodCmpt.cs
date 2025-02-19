using System;
using UnityEngine;

namespace Pathea;

public class LodCmpt : PeCmpt, ISceneObjAgent
{
	public Action<PeEntity> onConstruct;

	public Action<PeEntity> onDestruct;

	public Action<PeEntity> onActivate;

	public Action<PeEntity> onDeactivate;

	public Action<PeEntity> onDestroyView;

	public Action<PeEntity> onDestroyEntity;

	int ISceneObjAgent.Id { get; set; }

	int ISceneObjAgent.ScenarioId { get; set; }

	Vector3 ISceneObjAgent.Pos => (!Equals(null)) ? base.Entity.peTrans.position : Vector3.zero;

	public virtual GameObject Go => (!Equals(null)) ? base.Entity.gameObject : null;

	public virtual IBoundInScene Bound => null;

	public virtual bool NeedToActivate => true;

	public virtual bool TstYOnActivate => true;

	protected void BaseStart()
	{
		base.Start();
	}

	public override void Start()
	{
		base.Start();
		SceneMan.AddSceneObj(this);
	}

	public override void OnDestroy()
	{
		SceneMan.RemoveSceneObj(this);
		if (onDestroyEntity != null)
		{
			onDestroyEntity(base.Entity);
		}
	}

	protected void EnablePhy()
	{
		if (base.Entity.motionMgr != null)
		{
			base.Entity.motionMgr.FreezePhySteateForSystem(v: false);
		}
	}

	protected void DisablePhy()
	{
		if (base.Entity.motionMgr != null)
		{
			base.Entity.motionMgr.FreezePhySteateForSystem(v: true);
		}
	}

	protected void BuildView()
	{
		base.Entity.biologyViewCmpt.Build();
	}

	private void FadeIn()
	{
		base.Entity.biologyViewCmpt.Fadein();
	}

	private void FadeOut()
	{
		base.Entity.biologyViewCmpt.Fadeout();
	}

	public void DestroyView()
	{
		if (base.Entity.Id != PeSingleton<PeCreature>.Instance.mainPlayerId && !(base.Entity.viewCmpt == null))
		{
			base.Entity.biologyViewCmpt.Destroy();
		}
	}

	public virtual void OnConstruct()
	{
		if (Equals(null))
		{
			SceneMan.RemoveSceneObj(this);
			return;
		}
		if (!PeGameMgr.IsMulti && base.Entity.Field == MovementField.Sky && base.Entity.gravity > -1E-45f && base.Entity.gravity < float.Epsilon)
		{
			CancelInvoke("FadeOut");
			CancelInvoke("DelayDestroy");
			if (!base.Entity.hasView)
			{
				BuildView();
			}
			else
			{
				FadeIn();
			}
			EnablePhy();
		}
		if (onConstruct != null)
		{
			onConstruct(base.Entity);
		}
	}

	public virtual void OnDestruct()
	{
		if (Equals(null))
		{
			SceneMan.RemoveSceneObj(this);
			return;
		}
		if (base.Entity.hasView)
		{
			if (!IsInvoking("FadeOut"))
			{
				Invoke("FadeOut", 3f);
			}
			if (!IsInvoking("DelayDestroy"))
			{
				Invoke("DelayDestroy", 5f);
			}
			DisablePhy();
		}
		if (onDestruct != null)
		{
			onDestruct(base.Entity);
		}
	}

	public virtual void OnActivate()
	{
		if (Equals(null))
		{
			SceneMan.RemoveSceneObj(this);
			return;
		}
		CancelInvoke("FadeOut");
		CancelInvoke("DelayDestroy");
		if (!base.Entity.hasView)
		{
			BuildView();
		}
		else
		{
			FadeIn();
		}
		EnablePhy();
		base.Entity.SendMsg(EMsg.Lod_Collider_Created);
		if (onActivate != null)
		{
			onActivate(base.Entity);
		}
	}

	public virtual void OnDeactivate()
	{
		if (Equals(null))
		{
			SceneMan.RemoveSceneObj(this);
			return;
		}
		if (PeGameMgr.IsMulti || base.Entity.Field != MovementField.Sky)
		{
			if (!base.Entity.Equals(PeSingleton<PeCreature>.Instance.mainPlayer))
			{
				if (!IsInvoking("FadeOut"))
				{
					Invoke("FadeOut", 3f);
				}
				if (!IsInvoking("DelayDestroy"))
				{
					Invoke("DelayDestroy", 5f);
				}
			}
			DisablePhy();
		}
		base.Entity.SendMsg(EMsg.Lod_Collider_Destroying);
		if (onDeactivate != null)
		{
			onDeactivate(base.Entity);
		}
	}

	private void DelayDestroy()
	{
		DestroyView();
		if (onDestroyView != null)
		{
			onDestroyView(base.Entity);
		}
	}
}
