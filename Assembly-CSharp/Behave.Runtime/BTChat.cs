using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTChat), "Chat")]
public class BTChat : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float radius;

		[Behave]
		public float chatRadius;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		[Behave]
		public float minChatTime;

		[Behave]
		public float maxChatTime;

		[Behave]
		public string[] chats = new string[0];

		public float m_Time;

		public float m_ChatTime;

		public float m_StartTime;

		public float m_LastChatTime;

		public float m_LastCDTime;
	}

	private Data m_Data;

	private void GetChatTarget()
	{
		if (base.entity.Chat == null)
		{
			int playerID = (int)base.entity.GetAttribute(AttribType.DefaultPlayerID);
			List<PeEntity> entitiesFriendly = PeSingleton<EntityMgr>.Instance.GetEntitiesFriendly(base.position, m_Data.radius, playerID, base.entity.ProtoID, isDeath: false, base.entity);
			if (entitiesFriendly.Count > 0)
			{
				PeEntity peEntity = entitiesFriendly[Random.Range(0, entitiesFriendly.Count)];
				base.entity.Chat = peEntity;
				peEntity.Chat = base.entity;
			}
		}
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_LastCDTime < m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Chat == null && Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		GetChatTarget();
		if (base.entity.Chat == null)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartTime = Time.time;
		m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
		m_Data.m_ChatTime = Random.Range(m_Data.minChatTime, m_Data.maxChatTime);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (GetBool("Chatting"))
		{
			return BehaveResult.Running;
		}
		if (base.entity.Chat == null)
		{
			return BehaveResult.Success;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime > m_Data.m_Time)
		{
			return BehaveResult.Success;
		}
		if (PEUtil.MagnitudeH(base.position, base.entity.Chat.position) > base.radius + base.entity.Chat.maxRadius + m_Data.chatRadius)
		{
			MoveToPosition(base.entity.Chat.position);
		}
		else
		{
			MoveToPosition(Vector3.zero);
			if (!PEUtil.IsScopeAngle(base.entity.Chat.position - base.position, base.transform.forward, Vector3.up, -15f, 15f))
			{
				FaceDirection(base.entity.Chat.position - base.position);
			}
			else
			{
				FaceDirection(Vector3.zero);
				if (Time.time - m_Data.m_LastChatTime > m_Data.m_ChatTime)
				{
					if (m_Data.chats.Length > 0)
					{
						SetBool(m_Data.chats[Random.Range(0, m_Data.chats.Length)], value: true);
					}
					m_Data.m_LastChatTime = Time.time;
					m_Data.m_ChatTime = Random.Range(m_Data.minChatTime, m_Data.maxChatTime);
				}
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			if (base.entity.Chat != null)
			{
				base.entity.Chat.Chat = null;
				base.entity.Chat = null;
			}
			m_Data.m_Time = 0f;
			m_Data.m_ChatTime = 0f;
			m_Data.m_StartTime = 0f;
			m_Data.m_LastChatTime = 0f;
			m_Data.m_LastCDTime = Time.time;
		}
	}
}
