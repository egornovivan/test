using ItemAsset;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(NpcBaseCollect), "NpcBaseCollect")]
public class NpcBaseCollect : BTNormal
{
	private static Vector3 s_Position = new Vector3(0f, -10000f, 0f);

	private float m_StartTime;

	private Vector3 m_Moveposition;

	private bool m_Transparent;

	private bool m_SetPos;

	private Vector3 GetMovePosition()
	{
		return PEUtil.GetRandomPosition(base.position, 1024f, 2048f);
	}

	private Vector3 GetPosition()
	{
		return PEUtil.GetRandomPositionOnGroundForWander(base.Creater.Assembly.Position, base.Creater.Assembly.Radius * 0.7f, base.Creater.Assembly.Radius);
	}

	private Vector3 GetCSWokePosition(CSEntity WokeEnity)
	{
		return WokeEnity.Position;
	}

	private Vector3 GetCollectPos(CSEntity WokeEnity)
	{
		return (WokeEnity == null || WokeEnity.workTrans.Length <= 0) ? WokeEnity.Position : WokeEnity.workTrans[8].position;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Processor)
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcProcessing || base.WorkEntity == null)
		{
			return BehaveResult.Failure;
		}
		m_SetPos = false;
		m_Transparent = false;
		m_StartTime = Time.time;
		m_Moveposition = GetMovePosition();
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcBaseJobProcessor_Collect);
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Processor)
		{
			return BehaveResult.Failure;
		}
		if (base.NpcJob != ENpcJob.Processor && base.IsNpcProcessing)
		{
			Fadein(3f);
			Vector3 setPos = GetPosition();
			SetPosition(setPos);
			return BehaveResult.Success;
		}
		if (!base.IsNpcProcessing)
		{
			Fadein(3f);
			Vector3 setPos2 = GetPosition();
			SetPosition(setPos2);
			return BehaveResult.Success;
		}
		float num = Time.time - m_StartTime;
		if (num < 10f)
		{
			bool flag = PEUtil.IsUnderBlock(base.entity);
			bool flag2 = PEUtil.IsForwardBlock(base.entity, base.existent.forward, 2f);
			if (flag)
			{
				SetPosition(m_Moveposition);
			}
			else
			{
				MoveToPosition(m_Moveposition, SpeedState.Run);
			}
			if (flag2)
			{
				SetPosition(m_Moveposition);
			}
		}
		else if (num < 13f)
		{
			if (!m_Transparent)
			{
				Fadeout(3f);
				m_Transparent = true;
			}
		}
		else if (!m_SetPos)
		{
			SetPosition(s_Position);
			m_SetPos = true;
		}
		ItemObject item = null;
		CSStorage storage = null;
		if (NpcEatDb.CanEatSthFromStorages(base.entity, base.Creater.Assembly.Storages, out item, out storage) && base.entity.UseItem.GetCdByItemProtoId(item.protoId) < float.Epsilon)
		{
			UseItem(item);
			CSUtils.DeleteItemInStorage(storage, item.protoId);
		}
		return BehaveResult.Running;
	}
}
