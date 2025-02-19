using Pathea.Effect;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTLayEggs), "LayEggs")]
public class BTLayEggs : BTNormal
{
	private class Data
	{
		[Behave]
		public string animName = string.Empty;

		[Behave]
		public string boneName = string.Empty;

		[Behave]
		public float cdTime;

		[Behave]
		public float delayTime;

		[Behave]
		public float hpPercent;

		[Behave]
		public int protoID;

		[Behave]
		public int effectID;
	}

	private Data m_Data;

	private float m_SpawnTime;

	private float m_LastHatchTime;

	private bool m_Spawn;

	private Transform m_SpawnTrans;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_SpawnTime < float.Epsilon)
		{
			m_SpawnTime = Time.time;
		}
		if (m_SpawnTrans == null)
		{
			m_SpawnTrans = PEUtil.GetChild(base.entity.tr, m_Data.boneName);
		}
		if (m_SpawnTrans == null)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.HPPercent >= m_Data.hpPercent)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_LastHatchTime < m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		m_Spawn = false;
		m_LastHatchTime = Time.time;
		SetBool(m_Data.animName, value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!m_Spawn && Time.time - m_LastHatchTime >= m_Data.delayTime)
		{
			m_Spawn = true;
			SceneMan.AddSceneObj(MonsterEntityCreator.CreateAgent(m_SpawnTrans.position, m_Data.protoID));
			global::Singleton<EffectBuilder>.Instance.Register(m_Data.effectID, null, m_SpawnTrans.position, Quaternion.identity, m_SpawnTrans);
		}
		if (Time.time - m_LastHatchTime < 0.5f)
		{
			return BehaveResult.Running;
		}
		if (base.entity.IsAttacking)
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
