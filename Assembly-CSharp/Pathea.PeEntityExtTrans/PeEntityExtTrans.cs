using UnityEngine;

namespace Pathea.PeEntityExtTrans;

public static class PeEntityExtTrans
{
	public static Transform ExtGetTrans(this PeEntity entity)
	{
		PeTrans peTrans = entity.peTrans;
		if (null == peTrans)
		{
			return null;
		}
		return peTrans.trans;
	}

	public static void SetStayPos(this PeEntity entity, Vector3 pos)
	{
	}

	public static void SetBirthPos(this PeEntity entity, Vector3 pos)
	{
		NpcCmpt cmpt = entity.GetCmpt<NpcCmpt>();
		if (null != cmpt)
		{
			cmpt.FixedPointPos = pos;
		}
	}

	public static Vector3 ExtGetPos(this PeEntity entity)
	{
		if (null == entity)
		{
			return Vector3.zero;
		}
		PeTrans peTrans = entity.peTrans;
		if (null == peTrans)
		{
			return Vector3.zero;
		}
		return peTrans.position;
	}

	public static void ExtSetPos(this PeEntity entity, Vector3 value)
	{
		if (!(null == entity))
		{
			PeTrans peTrans = entity.peTrans;
			if (!(null == peTrans))
			{
				peTrans.position = value;
			}
		}
	}

	public static Quaternion ExtGetRot(this PeEntity entity)
	{
		if (null == entity)
		{
			return Quaternion.identity;
		}
		PeTrans peTrans = entity.peTrans;
		if (null == peTrans)
		{
			return Quaternion.identity;
		}
		return peTrans.rotation;
	}

	public static void ExtSetRot(this PeEntity entity, Quaternion value)
	{
		if (!(null == entity))
		{
			PeTrans peTrans = entity.peTrans;
			if (!(null == peTrans))
			{
				peTrans.rotation = value;
			}
		}
	}

	public static Vector3 GetForward(this PeEntity entity)
	{
		PeTrans peTrans = entity.peTrans;
		if (null == peTrans)
		{
			return Vector3.zero;
		}
		return peTrans.forward;
	}
}
