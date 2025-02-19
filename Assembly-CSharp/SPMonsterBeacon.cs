using UnityEngine;

public class SPMonsterBeacon : SPAutomatic
{
	public int id;

	public float delayTime;

	public float minRadius;

	public float maxRadius;

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

	protected override void OnSpawnComplete()
	{
		base.OnSpawnComplete();
		Delete();
	}

	protected override void OnTerrainExit(IntVector4 node)
	{
		base.OnTerrainExit(node);
		if (!GameConfig.IsMultiMode)
		{
			Delete();
		}
	}

	private bool GetPositionAndRotation(out Vector3 pos, out Quaternion rot, float minAngle, float maxAngle)
	{
		pos = Vector3.zero;
		rot = Quaternion.identity;
		pos = AiUtil.GetRandomPosition(base.transform.position, minRadius, maxRadius, Vector3.forward, minAngle, maxAngle, 10f, AiUtil.groundedLayer, 5);
		if (pos != Vector3.zero)
		{
			return true;
		}
		pos = AiUtil.GetRandomPosition(base.transform.position, minRadius, maxRadius, Vector3.forward, minAngle, maxAngle);
		if (pos != Vector3.zero)
		{
			return true;
		}
		return false;
	}

	public new void Awake()
	{
		base.Awake();
		base.ID = id;
		base.Delay = delayTime;
	}
}
