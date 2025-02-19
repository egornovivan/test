using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTPhotophobia), "Photophobia")]
public class BTPhotophobia : BTNormal
{
	private LightUnit m_Light;

	private Vector3 m_Position;

	private float m_RandomTime;

	private float m_LastRandomTime;

	private float m_BeatTime;

	private float m_StartBeatTime;

	private BehaveResult Init(Tree sender)
	{
		if (Time.time - m_StartBeatTime < m_BeatTime)
		{
			return BehaveResult.Failure;
		}
		if (m_Light == null)
		{
			m_Light = LightMgr.Instance.GetLight(base.entity.tr, base.entity.bounds);
		}
		if (m_Light == null || !m_Light.isActiveAndEnabled)
		{
			return BehaveResult.Failure;
		}
		m_LastRandomTime = Time.time;
		m_Position = m_Light.GetPositionOutOfLight(base.entity.position);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (m_Light == null || !m_Light.isActiveAndEnabled || m_Position == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		if (PEUtil.SqrMagnitude(base.position, m_Position, is3D: false) <= 1f || Stucking(3f))
		{
			if (!m_Light.IsInLight(base.entity.tr, base.entity.bounds))
			{
				return BehaveResult.Success;
			}
			m_Position = m_Light.GetPositionOutOfLight(base.entity.position);
		}
		else if (Time.time - m_LastRandomTime > m_RandomTime)
		{
			m_LastRandomTime = Time.time;
			m_RandomTime = Random.Range(2f, 5f);
			if (!m_Light.IsInLight(base.entity.tr, base.entity.bounds))
			{
				return BehaveResult.Success;
			}
			if (base.attackEnemy != null && base.attackEnemy.MoveDir == EEnemyMoveDir.Close && Random.value < 0.3f)
			{
				m_StartBeatTime = Time.time;
				m_BeatTime = Random.Range(5f, 10f);
				return BehaveResult.Failure;
			}
			m_Position = m_Light.GetPositionOutOfLight(base.entity.position);
		}
		FaceDirection(m_Light.transform.position - base.position);
		MoveDirection(m_Position - base.position, SpeedState.Run);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_Light != null)
		{
			m_Light = null;
			FaceDirection(Vector3.zero);
		}
	}
}
