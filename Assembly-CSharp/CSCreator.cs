using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public abstract class CSCreator : MonoBehaviour
{
	public delegate void EventListenerDel(int event_type, CSEntity entiy);

	public delegate void EventListenerPersonnelDel(int event_type, CSPersonnel p);

	public int teamNum;

	public CSConst.CreatorType m_Type;

	public CSDataInst m_DataInst;

	public virtual CSAssembly Assembly => null;

	public virtual PETimer Timer => null;

	public float Deltatime => (Timer != null) ? (Timer.ElapseSpeed * Time.deltaTime) : 0f;

	public int ID => m_DataInst.m_ID;

	protected event EventListenerDel m_EventListenser;

	protected event EventListenerPersonnelDel m_EventListenserPer;

	public void RegisterListener(EventListenerDel listener)
	{
		this.m_EventListenser = (EventListenerDel)Delegate.Combine(this.m_EventListenser, listener);
	}

	public void UnregisterListener(EventListenerDel listener)
	{
		this.m_EventListenser = (EventListenerDel)Delegate.Remove(this.m_EventListenser, listener);
	}

	public void ExecuteEvent(int event_type, CSEntity entity)
	{
		if (this.m_EventListenser != null)
		{
			this.m_EventListenser(event_type, entity);
		}
	}

	public void RegisterPersonnelListener(EventListenerPersonnelDel listener)
	{
		this.m_EventListenserPer = (EventListenerPersonnelDel)Delegate.Combine(this.m_EventListenserPer, listener);
	}

	public void UnregisterPeronnelListener(EventListenerPersonnelDel listener)
	{
		this.m_EventListenserPer = (EventListenerPersonnelDel)Delegate.Remove(this.m_EventListenserPer, listener);
	}

	protected void ExecuteEventPersonnel(int event_type, CSPersonnel p)
	{
		if (this.m_EventListenserPer != null)
		{
			this.m_EventListenserPer(event_type, p);
		}
	}

	public abstract int CreateEntity(CSEntityAttr attr, out CSEntity outEnti);

	public abstract CSEntity RemoveEntity(int id, bool bRemoveData = true);

	public abstract CSCommon GetCommonEntity(int ID);

	public abstract int GetCommonEntityCnt();

	public abstract Dictionary<int, CSCommon> GetCommonEntities();

	public virtual int CanCreate(int type, Vector3 pos)
	{
		return 4;
	}

	public virtual bool CanAddNpc()
	{
		return true;
	}

	public abstract bool AddNpc(PeEntity npc, bool bSetPos = false);

	public abstract void RemoveNpc(PeEntity npc);

	public abstract CSPersonnel[] GetNpcs();

	public abstract CSPersonnel GetNpc(int id);

	public virtual void RemoveLogic(int id)
	{
	}

	public virtual void AddLogic(int id, CSBuildingLogic csb)
	{
	}
}
