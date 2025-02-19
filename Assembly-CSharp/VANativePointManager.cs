using System.Collections.Generic;
using TownData;
using UnityEngine;

public class VANativePointManager : MonoBehaviour
{
	private static VANativePointManager mInstance;

	public Dictionary<IntVector2, NativePointInfo> nativePointInfoMap;

	public Dictionary<IntVector2, int> createdPosList;

	public static VANativePointManager Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		nativePointInfoMap = new Dictionary<IntVector2, NativePointInfo>();
		createdPosList = new Dictionary<IntVector2, int>();
	}

	public void RenderNative(IntVector2 nativeXZ)
	{
		NativePointInfo nativePointInfo = nativePointInfoMap[nativeXZ];
		if (nativePointInfo.PosY != -1f)
		{
			RenderNative(nativePointInfo);
		}
	}

	public void RenderNative(NativePointInfo nativePointInfo)
	{
		int iD = nativePointInfo.ID;
		Vector3 position = nativePointInfo.position;
		int allyId = VArtifactTownManager.Instance.GetTownByID(nativePointInfo.townId).AllyId;
		int playerId = VATownGenerator.Instance.GetPlayerId(allyId);
		int allyColor = VATownGenerator.Instance.GetAllyColor(allyId);
		SceneEntityPosAgent sceneEntityPosAgent = MonsterEntityCreator.CreateAdAgent(position, iD, allyColor, playerId);
		SceneMan.AddSceneObj(sceneEntityPosAgent);
		VArtifactTownManager.Instance.AddMonsterPointAgent(nativePointInfo.townId, sceneEntityPosAgent);
	}

	internal NativePointInfo GetNativePointByPosXZ(IntVector2 nativePointXZ)
	{
		if (!nativePointInfoMap.ContainsKey(nativePointXZ))
		{
			return null;
		}
		return nativePointInfoMap[nativePointXZ];
	}

	internal void AddNative(NativePointInfo nativePointInfo)
	{
		nativePointInfoMap[nativePointInfo.index] = nativePointInfo;
	}
}
