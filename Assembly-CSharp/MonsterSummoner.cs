using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathea;
using PETools;
using UnityEngine;
using VANativeCampXML;

public class MonsterSummoner : MonoBehaviour
{
	[SerializeField]
	private int _maxCntOfMonsters;

	[SerializeField]
	private float _radius = 64f;

	[SerializeField]
	private float _playerRadius = 168f;

	[SerializeField]
	private Vector2 _timeIntervalMinMax = new Vector2(10f, 50f);

	[SerializeField]
	private int[] _protoIdsOfMonsters;

	[SerializeField]
	private Vector3[] _posOfMonsters;

	public float Radius
	{
		set
		{
			_radius = value;
		}
	}

	public Vector3 Center => base.transform.position;

	public int MaxCntOfMonsters
	{
		set
		{
			_maxCntOfMonsters = value;
		}
	}

	public int[] ProtoIdsOfMonsters
	{
		set
		{
			_protoIdsOfMonsters = value;
		}
	}

	public Vector3[] PosOfMonsters
	{
		set
		{
			_posOfMonsters = value;
		}
	}

	private void Start()
	{
		if (PeGameMgr.IsAdventure)
		{
			SceneDoodadLodCmpt componentInParent = GetComponentInParent<SceneDoodadLodCmpt>();
			if (componentInParent != null && componentInParent.Index >= 0)
			{
				List<Vector3> posList;
				DynamicNative[] allDynamicNativePoint = VArtifactUtil.GetAllDynamicNativePoint(componentInParent.Index, out posList);
				if (allDynamicNativePoint == null)
				{
					return;
				}
				int num = allDynamicNativePoint.Length;
				_posOfMonsters = posList.ToArray();
				_protoIdsOfMonsters = new int[num];
				for (int i = 0; i < num; i++)
				{
					_protoIdsOfMonsters[i] = ((allDynamicNativePoint[i].type != 0) ? allDynamicNativePoint[i].did : (allDynamicNativePoint[i].did | 0x40000000));
				}
				_maxCntOfMonsters = 8;
			}
		}
		_radius = 256f;
		StartCoroutine(RefreshAgents());
	}

	private IEnumerator RefreshAgents()
	{
		while (true)
		{
			if (PEUtil.SqrMagnitudeH(PeSingleton<PeCreature>.Instance.mainPlayer.position, Center) <= _playerRadius * _playerRadius)
			{
				int n = GetEntitiesCnt();
				if (n < _maxCntOfMonsters && _posOfMonsters != null && _posOfMonsters.Length > 0 && _protoIdsOfMonsters != null && _protoIdsOfMonsters.Length > 0)
				{
					Vector3 pos = GetSpawnPos();
					int protoId = GetProtoId();
					if (PeGameMgr.IsAdventure)
					{
						SceneDoodadLodCmpt lod = GetComponentInParent<SceneDoodadLodCmpt>();
						if (lod != null && lod.Index >= 0)
						{
							int allyId = VArtifactTownManager.Instance.GetTownByID(lod.Index).AllyId;
							int playerId = VATownGenerator.Instance.GetPlayerId(allyId);
							int allyColor = VATownGenerator.Instance.GetAllyColor(allyId);
							MonsterEntityCreator.CreateAdMonster(protoId, pos, allyColor, playerId);
						}
					}
					else
					{
						MonsterEntityCreator.CreateMonster(protoId, pos);
					}
				}
			}
			yield return new WaitForSeconds(Random.Range(_timeIntervalMinMax.x, _timeIntervalMinMax.y));
		}
	}

	private Vector3 GetSpawnPos()
	{
		return _posOfMonsters[Random.Range(0, _posOfMonsters.Length)];
	}

	private int GetProtoId()
	{
		return _protoIdsOfMonsters[Random.Range(0, _protoIdsOfMonsters.Length)];
	}

	private int GetEntitiesCnt()
	{
		List<PeEntity> source = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.mDicEntity.Values);
		return source.Count((PeEntity it) => it.commonCmpt != null && (it.commonCmpt.Race == ERace.Paja || it.commonCmpt.Race == ERace.Puja) && PEUtil.SqrMagnitudeH(it.position, Center) <= _radius * _radius);
	}
}
