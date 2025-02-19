using UnityEngine;

namespace WhiteCat;

public class AITurretController : AIBehaviourController
{
	protected override AIMode defaultAIMode => AIMode.Attack;

	protected override float mass => Mathf.Clamp(base.creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.aiTurretMassScale, PEVCConfig.instance.aiTurretMinMass, PEVCConfig.instance.aiTurretMaxMass);

	protected override Vector3 centerOfMass => base.creationController.creationData.m_Attribute.m_CenterOfMass;

	protected override Vector3 inertiaTensorScale => Vector3.one;

	public int bulletProtoId => GetWeapon().bulletProtoID;

	protected override void InitDrags(out float standardDrag, out float underwaterDrag, out float standardAngularDrag, out float underwaterAngularDrag)
	{
		standardDrag = PEVCConfig.instance.robotStandardDrag;
		underwaterDrag = PEVCConfig.instance.robotUnderwaterDrag;
		standardAngularDrag = PEVCConfig.instance.robotStandardAngularDrag;
		underwaterAngularDrag = PEVCConfig.instance.robotUnderwaterAngularDrag;
	}

	protected override void InitOtherThings()
	{
		base.InitOtherThings();
		base.gameObject.AddComponent<ItemScript>();
		base.gameObject.AddComponent<DragItemMousePickTowerCreation>().Init(this);
		base.rigidbody.constraints = (RigidbodyConstraints)122;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isPlayerHost)
		{
			UpdateAttactTarget();
		}
	}
}
