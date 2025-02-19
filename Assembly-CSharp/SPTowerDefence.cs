using UnityEngine;

public class SPTowerDefence : SPAutomatic, ITowerDefenceData
{
	private int mMissionID;

	private float mMinRadius;

	private float mMaxRadius;

	public int MissionID => mMissionID;

	public static SPTowerDefence InstantiateTowerDefence(int mission, Vector3 position, float minRadius, float maxRadius, int id, float delayTime = 0f, Transform parent = null, bool isPlay = true)
	{
		GameObject gameObject = new GameObject("SPTowerDefence");
		SPTowerDefence sPTowerDefence = gameObject.AddComponent<SPTowerDefence>();
		gameObject.transform.position = position;
		gameObject.transform.parent = parent;
		sPTowerDefence.ID = id;
		sPTowerDefence.Delay = delayTime;
		sPTowerDefence.mMissionID = mission;
		sPTowerDefence.mMinRadius = minRadius;
		sPTowerDefence.mMaxRadius = maxRadius;
		if (isPlay)
		{
			sPTowerDefence.SpawnAutomatic();
		}
		return sPTowerDefence;
	}

	protected override void OnSpawnComplete()
	{
		base.OnSpawnComplete();
	}

	protected override SPPoint Spawn(AISpawnData spData)
	{
		base.Spawn(spData);
		if (GetPositionAndRotation(out var pos, out var rot, spData.minAngle, spData.maxAngle))
		{
			SPPointMovable sPPointMovable = SPPoint.InstantiateSPPoint<SPPointMovable>(pos, rot, IntVector4.Zero, base.pointParent, (!spData.isPath) ? spData.spID : 0, spData.isPath ? spData.spID : 0, isActive: true, revisePos: true, isBoss: false, erode: false, delete: true, null, OnSpawned, this);
			sPPointMovable.target = base.transform;
			return sPPointMovable;
		}
		return null;
	}

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		AiObject component = obj.GetComponent<AiObject>();
		if (component != null)
		{
			component.tdInfo = base.transform;
		}
		SPGroup component2 = obj.GetComponent<SPGroup>();
		if (component2 != null)
		{
			component2.tdInfo = base.transform;
		}
	}

	protected override void OnDeath(AiObject aiObj)
	{
		base.OnDeath(aiObj);
		if (mMissionID <= 0)
		{
		}
	}

	private bool GetPositionAndRotation(out Vector3 pos, out Quaternion rot, float minAngle, float maxAngle)
	{
		pos = Vector3.zero;
		rot = Quaternion.identity;
		pos = AiUtil.GetRandomPosition(base.transform.position, mMinRadius, mMaxRadius, Vector3.forward, minAngle, maxAngle, 10f, AiUtil.groundedLayer, 5);
		if (pos != Vector3.zero)
		{
			return true;
		}
		if (!GameConfig.IsMultiMode)
		{
			pos = AiUtil.GetRandomPosition(base.transform.position, mMinRadius, mMaxRadius, Vector3.forward, minAngle, maxAngle);
			if (pos != Vector3.zero)
			{
				return true;
			}
		}
		return false;
	}

	public void NetWorkDestroyObject()
	{
		Object.Destroy(base.gameObject, 0.1f);
	}
}
