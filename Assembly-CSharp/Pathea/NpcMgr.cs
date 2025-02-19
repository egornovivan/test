using System.Collections.Generic;
using Pathfinding;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class NpcMgr
{
	public static int[] InFeildBuff = new int[2] { 30200053, 30200046 };

	public static int[] RecruitBuff = new int[2] { 30200049, 30200050 };

	public static bool CallBackColonyNpcImmediately(PeEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (entity.NpcCmpt == null)
		{
			return false;
		}
		CSCreator cSCreator = entity.NpcCmpt.Creater;
		if (cSCreator == null)
		{
			cSCreator = CSMain.GetCreator(0);
		}
		if (cSCreator == null || cSCreator.Assembly == null)
		{
			return false;
		}
		if (!IsOutRadiu(entity.position, cSCreator.Assembly.Position, cSCreator.Assembly.Radius))
		{
			return true;
		}
		Vector3 vector = PEUtil.GetRandomPositionOnGroundForWander(cSCreator.Assembly.Position, cSCreator.Assembly.Radius * 0.7f, cSCreator.Assembly.Radius);
		if (vector == Vector3.zero)
		{
			vector = cSCreator.Assembly.Position;
		}
		entity.NpcCmpt.Req_Translate(vector);
		return true;
	}

	public static bool CallBackCampsieNpcImmediately(PeEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (entity.NpcCmpt == null)
		{
			return false;
		}
		return false;
	}

	public static bool CallBackNpcToMainPlayer(PeEntity entity)
	{
		if (PeSingleton<PeCreature>.Instance == null || PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return false;
		}
		Vector3 position = PeSingleton<PeCreature>.Instance.mainPlayer.position;
		position.z += 2f;
		position.y += 1f;
		if (entity.NpcCmpt != null)
		{
			entity.NpcCmpt.Req_Translate(position);
			return true;
		}
		return false;
	}

	public static bool CallBackToFixPiontImmediately(PeEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (entity.NpcCmpt == null || entity.NpcCmpt.FixedPointPos == Vector3.zero)
		{
			return false;
		}
		entity.NpcCmpt.Req_Translate(entity.NpcCmpt.FixedPointPos);
		return true;
	}

	public static bool ColonyNpcLostController(PeEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (entity.NpcCmpt == null)
		{
			return false;
		}
		CSCreator creater = entity.NpcCmpt.Creater;
		if (creater == null || creater.Assembly == null)
		{
			return false;
		}
		if (entity.NpcCmpt.Processing)
		{
			return false;
		}
		float num = PEUtil.MagnitudeH(entity.position, creater.Assembly.Position);
		if (num > creater.Assembly.Radius)
		{
			Vector3 vector = PEUtil.GetRandomPositionOnGroundForWander(creater.Assembly.Position, creater.Assembly.Radius * 0.7f, creater.Assembly.Radius);
			if (vector == Vector3.zero)
			{
				vector = creater.Assembly.Position;
			}
			entity.NpcCmpt.Req_Translate(vector);
			return true;
		}
		return false;
	}

	public static void GetRandomPathForCsWander(PeEntity npc, Vector3 center, Vector3 direction, float minRadius, float maxRadius, OnPathDelegate callback = null)
	{
		if (AstarPath.active != null)
		{
			RandomPath randomPath = RandomPath.Construct(npc.position, (int)Random.Range(minRadius, maxRadius) * 100, callback);
			randomPath.spread = 40000;
			randomPath.aimStrength = 1f;
			randomPath.aim = PEUtil.GetRandomPosition(npc.position, direction, minRadius, maxRadius, -75f, 75f);
			AstarPath.StartPath(randomPath);
		}
	}

	public static bool IsIncenterAraound(Vector3 center, float radiu, Vector3 target)
	{
		float num = (target.x - center.x) * (target.x - center.x) + (target.y - center.y) * (target.y - center.y) + (target.z - center.y) * (target.y - center.y);
		return num <= radiu * radiu;
	}

	public static bool IsOutRadiu(Vector3 slf, Vector3 centor, float radiu)
	{
		float num = PEUtil.Magnitude(slf, centor);
		return num > radiu;
	}

	public static bool CallBackColonyNpcToPlayer(PeEntity entity, out ECsNpcState state)
	{
		state = ECsNpcState.None;
		if (entity == null || entity.NpcCmpt == null)
		{
			return false;
		}
		if (entity.NpcCmpt.Job != ENpcJob.Resident)
		{
			state = ECsNpcState.Working;
			return false;
		}
		if (entity.NpcCmpt.Creater == null || entity.NpcCmpt.Creater.Assembly == null)
		{
			if (entity.NpcCmpt.BaseNpcOutMission)
			{
				state = ECsNpcState.InMission;
			}
			return false;
		}
		float num = PEUtil.Magnitude(PeSingleton<PeCreature>.Instance.mainPlayer.position, entity.NpcCmpt.Creater.Assembly.Position);
		if (num > entity.NpcCmpt.Creater.Assembly.Radius)
		{
			state = ECsNpcState.OutOfRadiu;
			return false;
		}
		if (entity.target != null)
		{
			entity.target.ClearEnemy();
		}
		if (entity.biologyViewCmpt != null)
		{
			entity.biologyViewCmpt.Fadein();
		}
		Vector3 position = PeSingleton<PeCreature>.Instance.mainPlayer.position;
		position.z += 2f;
		position.y += 1f;
		entity.NpcCmpt.Req_Translate(position);
		return true;
	}

	public static bool NpcMissionReady(PeEntity npc)
	{
		if (npc == null)
		{
			return false;
		}
		if (npc.NpcCmpt == null)
		{
			return false;
		}
		CSCreator cSCreator = npc.NpcCmpt.Creater;
		if (cSCreator == null)
		{
			cSCreator = CSMain.GetCreator(0);
		}
		if (cSCreator == null || cSCreator.Assembly == null)
		{
			return false;
		}
		if (npc.UseItem != null && NpcEatDb.CanEatSthFromStorages(npc, cSCreator.Assembly.Storages, out var item))
		{
			npc.UseItem.Use(item);
		}
		for (int i = 0; i < RecruitBuff.Length; i++)
		{
			npc.skEntity.CancelBuffById(RecruitBuff[i]);
		}
		for (int j = 0; j < InFeildBuff.Length; j++)
		{
			SkEntity.MountBuff(npc.skEntity, InFeildBuff[j], new List<int>(), new List<float>());
		}
		return true;
	}

	public static void NpcMissionFinish(PeEntity npc)
	{
		for (int i = 0; i < InFeildBuff.Length; i++)
		{
			npc.skEntity.CancelBuffById(InFeildBuff[i]);
		}
		for (int j = 0; j < RecruitBuff.Length; j++)
		{
			SkEntity.MountBuff(npc.skEntity, RecruitBuff[j], new List<int>(), new List<float>());
		}
	}
}
