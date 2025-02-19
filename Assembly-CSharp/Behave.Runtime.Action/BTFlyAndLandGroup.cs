using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTFlyAndLandGroup), "FlyAndLandGroup")]
public class BTFlyAndLandGroup : BTNormalGroup
{
	private class Data
	{
		[Behave]
		public bool fly;
	}

	private Data m_Data;

	private float m_StartTime;

	private bool IsFlyEquals(BehaveGroup group)
	{
		foreach (PeEntity entity in group.Entities)
		{
			if (entity != null && !entity.IsDeath())
			{
				MonsterCmpt component = entity.GetComponent<MonsterCmpt>();
				if (component != null && component.IsFly != m_Data.fly)
				{
					return true;
				}
			}
		}
		return false;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		BehaveGroup behaveGroup = sender.ActiveAgent as BehaveGroup;
		if (behaveGroup == null || behaveGroup.Leader == null)
		{
			return BehaveResult.Failure;
		}
		if (!IsFlyEquals(behaveGroup))
		{
			return BehaveResult.Failure;
		}
		m_StartTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		BehaveGroup behaveGroup = sender.ActiveAgent as BehaveGroup;
		if (behaveGroup == null || behaveGroup.Leader == null)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.fly)
		{
			if (Time.time - m_StartTime < 0.5f)
			{
				return BehaveResult.Running;
			}
			behaveGroup.ActivateGravity(value: false);
			behaveGroup.Fly(value: true);
			return BehaveResult.Success;
		}
		behaveGroup.ActivateGravity(value: true);
		bool flag = false;
		for (int i = 0; i < behaveGroup.Entities.Count; i++)
		{
			PeEntity peEntity = behaveGroup.Entities[i];
			if (peEntity != null)
			{
				MonsterCmpt component = peEntity.GetComponent<MonsterCmpt>();
				Motion_Move component2 = peEntity.GetComponent<Motion_Move>();
				BehaveCmpt component3 = peEntity.GetComponent<BehaveCmpt>();
				if (component != null && component2 != null && component3 != null && component2.grounded)
				{
					component.Fly(value: false);
					component3.Pause(value: false);
					flag = true;
				}
			}
		}
		if (flag)
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
