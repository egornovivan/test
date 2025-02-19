using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTDark), "Dark")]
public class BTDark : BTNormal
{
	private Vector3 m_Position;

	private float m_Time;

	private float m_LastTime;

	private BehaveResult Tick(Tree sender)
	{
		if (!base.entity.IsDarkInDaytime)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_LastTime > m_Time || Stucking(3f))
		{
			Vector3 dir = base.transform.forward;
			if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				dir = base.position - PeSingleton<PeCreature>.Instance.mainPlayer.position;
			}
			m_Position = PEUtil.GetRandomPositionOnGround(base.position, dir, 32f, 64f, -75f, 75f);
			m_LastTime = Time.time;
			m_Time = Random.Range(5f, 10f);
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer != null && PEUtil.SqrMagnitude(base.position, PeSingleton<PeCreature>.Instance.mainPlayer.position) > 4096f)
		{
			global::Singleton<PeLogicGlobal>.Instance.DestroyEntity(base.entity.skEntity);
		}
		MoveToPosition(m_Position, SpeedState.Run);
		return BehaveResult.Running;
	}
}
