using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class SceneEntityPosRect
{
	private class RndNpcAgentInfo : NpcEntityCreator.AgentInfo
	{
		private SceneEntityPosRect _posRect;

		public RndNpcAgentInfo(SceneEntityPosRect posRect)
		{
			_posRect = posRect;
		}

		public override void OnSuceededToCreate(SceneEntityPosAgent agent)
		{
			base.OnSuceededToCreate(agent);
			_posRect._posRndNpcAgents.Remove(agent);
		}
	}

	private const int CntNpc = 2;

	private const int EntityCreationRadius = 128;

	private List<ISceneObjAgent> _posRndNpcAgents = new List<ISceneObjAgent>();

	private List<ISceneObjAgent> _posRndMonsterAgents = new List<ISceneObjAgent>();

	private List<int> _posFixedMonsterIds = new List<int>();

	private RndNpcAgentInfo _rndNpcAgentInfo;

	private IntVector2 _idx;

	private static int CntMonAdv => 6 + UnityEngine.Random.Range(0, 2);

	private static int CntMonOth => 5 + UnityEngine.Random.Range(0, 2);

	public static int EntityNpcNum => (!PeGameMgr.IsStory && !PeGameMgr.IsCustom) ? 2 : 0;

	public static int EntityMonsterNum => (PeGameMgr.playerType != PeGameMgr.EPlayerType.Tutorial) ? ((!PeGameMgr.IsAdventure) ? CntMonOth : CntMonAdv) : 0;

	public int CntNpcNotCreated => _posRndNpcAgents.Count;

	public SceneEntityPosRect(int ix, int iz)
	{
		_rndNpcAgentInfo = new RndNpcAgentInfo(this);
		_idx = new IntVector2(ix, iz);
	}

	public static void EntityPosToRectIdx(Vector3 pos, IntVector2 outIdx)
	{
		outIdx.x = Mathf.FloorToInt(pos.x / 256f);
		outIdx.y = Mathf.FloorToInt(pos.z / 256f);
	}

	public static Vector3 RectIdxToCenterPos(IntVector2 idx)
	{
		return new Vector3((idx.x * 2 + 1) * 128, 0f, (idx.y * 2 + 1) * 128);
	}

	public void Fill(int cntNpc, int cntMonster)
	{
		Vector3 cpos = RectIdxToCenterPos(_idx);
		bool bSuc = false;
		System.Random random = new System.Random();
		for (int i = 0; i < cntNpc; i++)
		{
			Vector3 pos = (PeGameMgr.randomMap ? GetNpcPointInRndTer(cpos, out bSuc) : GetEntityPoint(cpos, out bSuc));
			if (bSuc)
			{
				if (VFDataRTGen.IsTownConnectionType(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z)))
				{
					SceneEntityPosAgent sceneEntityPosAgent = NpcEntityCreator.CreateAgent(pos);
					sceneEntityPosAgent.spInfo = _rndNpcAgentInfo;
					_posRndNpcAgents.Add(sceneEntityPosAgent);
				}
				else if (random.NextDouble() < 0.25)
				{
					SceneEntityPosAgent sceneEntityPosAgent = NpcEntityCreator.CreateAgent(pos);
					sceneEntityPosAgent.spInfo = _rndNpcAgentInfo;
					_posRndNpcAgents.Add(sceneEntityPosAgent);
				}
			}
		}
		for (int j = 0; j < cntMonster; j++)
		{
			Vector3 entityPoint = GetEntityPoint(cpos, out bSuc);
			if (bSuc && AIErodeMap.IsInErodeArea2D(entityPoint) == null)
			{
				_posRndMonsterAgents.Add(MonsterEntityCreator.CreateAgent(entityPoint));
			}
		}
		SceneMan.AddSceneObjs(_posRndNpcAgents);
		SceneMan.AddSceneObjs(_posRndMonsterAgents);
		if (PeGameMgr.IsStory)
		{
			_posFixedMonsterIds = AISpawnPoint.Find(cpos.x - 128f, cpos.z - 128f, cpos.x + 128f, cpos.z + 128f);
			if (_posFixedMonsterIds.Count > 0)
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.AddFixedSpawnPointToScene(_posFixedMonsterIds);
			}
		}
		else if (PeGameMgr.IsSingleAdventure)
		{
			AddProcedualBossSpawnPointToScene(_idx);
		}
	}

	private Vector3 GetNpcPointInRndTer(Vector3 cpos, out bool bSuc)
	{
		Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
		normalized *= (float)UnityEngine.Random.Range(-128, 128);
		IntVector2 intVector = new IntVector2((int)(cpos.x + normalized.x), (int)(cpos.z + normalized.y));
		float posTop = VFDataRTGen.GetPosTop(intVector, out bSuc);
		if (!bSuc)
		{
			return Vector3.zero;
		}
		return new Vector3(intVector.x, posTop + 1f, intVector.y);
	}

	private Vector3 GetEntityPoint(Vector3 cpos, out bool bSuc)
	{
		Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
		normalized *= (float)UnityEngine.Random.Range(-128, 128);
		bSuc = true;
		return new Vector3(cpos.x + normalized.x, 0f, cpos.z + normalized.y);
	}

	public static void AddProcedualBossSpawnPointToScene(IntVector2 idxPosRect)
	{
		int num = idxPosRect.x >> 2;
		int num2 = idxPosRect.y >> 2;
		int num3 = (idxPosRect.y - (num2 << 2) << 2) + (idxPosRect.x - (num << 2));
		int maxValue = 16;
		int num4 = RandomMapConfig.RandSeed + num2 * 722 + num;
		System.Random random = new System.Random(num4);
		int num5 = random.Next(maxValue);
		int num6;
		for (num6 = random.Next(maxValue); num6 == num5; num6 = random.Next(maxValue))
		{
		}
		if (num3 == num5 || num3 == num6)
		{
			random = new System.Random(num4 + num3);
			float num7 = (float)random.NextDouble();
			float num8 = (float)random.NextDouble();
			Vector3 vector = new Vector3((num7 + (float)idxPosRect.x) * 2f * 128f, 0f, (num8 + (float)idxPosRect.y) * 2f * 128f);
			if (AIErodeMap.IsInScaledErodeArea2D(vector, 1.2f) == null)
			{
				SceneEntityPosAgent sceneEntityPosAgent = MonsterEntityCreator.CreateAgent(vector);
				sceneEntityPosAgent.spInfo = new MonsterEntityCreator.AgentInfo((float)random.NextDouble());
				SceneMan.AddSceneObj(sceneEntityPosAgent);
				Debug.Log("<color=red>Boss Spawn Point</color>" + vector);
			}
		}
	}
}
