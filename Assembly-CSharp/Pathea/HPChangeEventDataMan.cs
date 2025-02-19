using System.Collections.Generic;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class HPChangeEventDataMan : PeSingleton<HPChangeEventDataMan>, IPesingleton, IHPEventData
{
	private Stack<HPChangeEventData> _datas;

	void IPesingleton.Init()
	{
		_datas = new Stack<HPChangeEventData>();
	}

	public HPChangeEventData Pop()
	{
		if (_datas != null && _datas.Count > 0)
		{
			return _datas.Pop();
		}
		return null;
	}

	public void OnHpChange(SkEntity self, SkEntity caster, float hpChange)
	{
		if (null == self)
		{
			return;
		}
		SkAliveEntity skAliveEntity = self as SkAliveEntity;
		if (null != skAliveEntity)
		{
			PeTrans peTrans = skAliveEntity.Entity.peTrans;
			CommonCmpt commonCmpt = skAliveEntity.Entity.commonCmpt;
			if (null != peTrans)
			{
				HPChangeEventData hPChangeEventData = new HPChangeEventData();
				hPChangeEventData.m_Self = self;
				hPChangeEventData.m_HPChange = hpChange;
				hPChangeEventData.m_Transfrom = peTrans.trans;
				hPChangeEventData.m_Proto = ((!(null != commonCmpt) || commonCmpt.entityProto == null) ? EEntityProto.Doodad : commonCmpt.entityProto.proto);
				hPChangeEventData.m_AddTime = Time.realtimeSinceStartup;
				_datas.Push(hPChangeEventData);
			}
		}
	}
}
