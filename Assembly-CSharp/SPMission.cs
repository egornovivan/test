using UnityEngine;

public class SPMission : SPAutomatic
{
	private float mRadius;

	public static SPMission InstantiateMission(Vector3 position, float radius, int id, float delayTime = 0f, bool isPlay = true)
	{
		GameObject gameObject = new GameObject("SPMission");
		SPMission sPMission = gameObject.AddComponent<SPMission>();
		gameObject.transform.position = position;
		sPMission.mRadius = radius;
		sPMission.ID = id;
		sPMission.Delay = delayTime;
		if (isPlay)
		{
			sPMission.SpawnAutomatic();
		}
		return sPMission;
	}

	protected override SPPoint Spawn(AISpawnData spData)
	{
		base.Spawn(spData);
		if (GetPositionAndRotation(out var pos, out var rot))
		{
			SPPointMovable sPPointMovable = SPPoint.InstantiateSPPoint<SPPointMovable>(pos, rot, IntVector4.Zero, base.pointParent, (!spData.isPath) ? spData.spID : 0, spData.isPath ? spData.spID : 0, isActive: true, revisePos: true, isBoss: false, erode: false, delete: true, null, OnSpawned, this);
			sPPointMovable.target = base.transform;
			return sPPointMovable;
		}
		return null;
	}

	protected override void OnSpawnComplete()
	{
		base.OnSpawnComplete();
		Delete();
	}

	private bool GetPositionAndRotation(out Vector3 pos, out Quaternion rot)
	{
		pos = Vector3.zero;
		rot = Quaternion.identity;
		pos = AiUtil.GetRandomPosition(base.transform.position, 0f, mRadius, 10f, AiUtil.groundedLayer, 5);
		if (pos != Vector3.zero)
		{
			return true;
		}
		pos = AiUtil.GetRandomPosition(base.transform.position, 0f, mRadius);
		if (pos != Vector3.zero)
		{
			return true;
		}
		return false;
	}
}
