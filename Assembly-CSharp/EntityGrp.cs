using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PeCustom;
using UnityEngine;

public class EntityGrp : PeEntity
{
	private List<ISceneObjAgent> _lstAgents = new List<ISceneObjAgent>();

	[HideInInspector]
	public int _grpProtoId;

	public int _protoId;

	public int _cntMin;

	public int _cntMax;

	public int _atkMin;

	public int _atkMax;

	public float _radius;

	public float _sqrRejectRadius;

	[HideInInspector]
	public Action<PeEntity> handlerMonsterCreated;

	public List<ISceneObjAgent> memberAgents => _lstAgents;

	public static EntityGrp CreateMonsterGroup(int grpProtoId, Vector3 center, int colorType, int playerId, int entityId = -1, int buffId = 0)
	{
		int entityId2 = ((entityId != -1) ? entityId : PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId());
		MonsterGroupProtoDb.Item item = MonsterGroupProtoDb.Get(grpProtoId);
		if (item == null)
		{
			return null;
		}
		int num = 0;
		EntityGrp entityGrp = PeSingleton<EntityMgr>.Instance.Create(entityId2, "EntityGroup", center, Quaternion.identity, Vector3.one) as EntityGrp;
		if (entityGrp == null)
		{
			return null;
		}
		entityGrp._grpProtoId = grpProtoId;
		entityGrp._protoId = item.protoID;
		entityGrp._cntMin = item.cntMinMax[0];
		entityGrp._cntMax = item.cntMinMax[1];
		entityGrp._atkMin = item.atkMinMax[0];
		entityGrp._atkMax = item.atkMinMax[1];
		entityGrp._radius = item.radiusDesc[0];
		entityGrp._sqrRejectRadius = ((item.radiusDesc.Length <= 1) ? 1f : (item.radiusDesc[1] * item.radiusDesc[1]));
		num = UnityEngine.Random.Range(entityGrp._cntMin, entityGrp._cntMax);
		if (entityGrp._protoId > 0)
		{
			Vector3 pos = Vector3.zero;
			for (int i = 0; i < num; i++)
			{
				if (entityGrp.GetRandPos(center, ref pos, 5))
				{
					SceneEntityPosAgent sceneEntityPosAgent = MonsterEntityCreator.CreateAgent(pos, entityGrp._protoId);
					sceneEntityPosAgent.ScenarioId = entityGrp.scenarioId;
					sceneEntityPosAgent.spInfo = new MonsterEntityCreator.AgentInfo(entityGrp, colorType, playerId, buffId);
					entityGrp._lstAgents.Add(sceneEntityPosAgent);
				}
			}
		}
		num = ((item.subProtoID != null) ? item.subProtoID.Length : 0);
		for (int j = 0; j < num; j++)
		{
			if (item.subProtoID[j] > 0)
			{
				SceneEntityPosAgent sceneEntityPosAgent2 = MonsterEntityCreator.CreateAgent(item.subPos[j] + center, item.subProtoID[j], item.subScl[j], Quaternion.Euler(item.subRot[j]));
				sceneEntityPosAgent2.ScenarioId = entityGrp.scenarioId;
				sceneEntityPosAgent2.spInfo = new MonsterEntityCreator.AgentInfo(entityGrp, colorType, playerId, buffId);
				entityGrp._lstAgents.Add(sceneEntityPosAgent2);
			}
		}
		entityGrp.StartCoroutine(entityGrp.AddMemberAgents());
		return entityGrp;
	}

	private bool GetRandPos(Vector3 center, ref Vector3 pos, int nMaxTry)
	{
		int num = 0;
		do
		{
			Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
			pos = center;
			pos.x += insideUnitCircle.x * _radius;
			pos.z += insideUnitCircle.y * _radius;
			pos.y = 0f;
			int i;
			for (i = 0; i < _lstAgents.Count; i++)
			{
				Vector3 vector = _lstAgents[i].Pos - pos;
				if (vector.x * vector.x + vector.z * vector.z < _sqrRejectRadius)
				{
					break;
				}
			}
			if (i >= _lstAgents.Count)
			{
				return true;
			}
			num++;
		}
		while (num < nMaxTry);
		return false;
	}

	private IEnumerator AddMemberAgents()
	{
		yield return new WaitForSeconds(0.1f);
		int n = _lstAgents.Count;
		for (int i = 0; i < n; i++)
		{
			SceneMan.AddSceneObj(_lstAgents[i]);
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		SceneMan.RemoveSceneObjs(_lstAgents);
	}

	public void RemoveAllAgent()
	{
		SceneMan.RemoveSceneObjs(_lstAgents);
	}

	public void OnMemberCreated(PeEntity e)
	{
		if (!(e != null))
		{
			return;
		}
		e.transform.parent = base.transform;
		if (!PeGameMgr.IsMulti)
		{
			LodCmpt lodCmpt = e.lodCmpt;
			if (lodCmpt != null)
			{
				lodCmpt.onDestroyEntity = (Action<PeEntity>)Delegate.Combine(lodCmpt.onDestroyEntity, new Action<PeEntity>(OnMemberDestroy));
			}
		}
		if (handlerMonsterCreated != null)
		{
			handlerMonsterCreated(e);
		}
	}

	private void OnMemberDestroy(PeEntity e)
	{
		foreach (ISceneObjAgent lstAgent in _lstAgents)
		{
			if (lstAgent is SceneEntityPosAgent sceneEntityPosAgent)
			{
				if (sceneEntityPosAgent.entity != null && sceneEntityPosAgent.entity != e)
				{
					return;
				}
				continue;
			}
			SceneEntityAgent sceneEntityAgent = lstAgent as SceneEntityAgent;
			if (sceneEntityAgent.entity != null && sceneEntityAgent.entity != e)
			{
				return;
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
