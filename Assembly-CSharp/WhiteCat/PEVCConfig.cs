using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat;

public class PEVCConfig : ScriptableObject
{
	private static PEVCConfig _instance;

	public static readonly List<WorkShopFilter> isoNames = new List<WorkShopFilter>
	{
		new WorkShopFilter(8000295, 1, "Creation"),
		new WorkShopFilter(8000296, 2, "Equipment"),
		new WorkShopFilter(8000297, 3, "Sword"),
		new WorkShopFilter(8000298, 3, "Axe"),
		new WorkShopFilter(8000299, 3, "Bow"),
		new WorkShopFilter(8000300, 3, "Shield"),
		new WorkShopFilter(8000301, 3, "Gun"),
		new WorkShopFilter(8000302, 2, "Carrier"),
		new WorkShopFilter(8000303, 3, "Vehicle"),
		new WorkShopFilter(8000304, 3, "Ship"),
		new WorkShopFilter(8000305, 3, "Aircraft"),
		new WorkShopFilter(8000306, 2, "Armor"),
		new WorkShopFilter(8000307, 3, "Head"),
		new WorkShopFilter(8000308, 3, "Body"),
		new WorkShopFilter(8000309, 3, "Arm And Leg"),
		new WorkShopFilter(8000310, 3, "Head And Foot"),
		new WorkShopFilter(8000311, 3, "Decoration"),
		new WorkShopFilter(8000312, 2, "Robot"),
		new WorkShopFilter(8000313, 2, "AI Turret"),
		new WorkShopFilter(8000314, 2, "Object")
	};

	[Header("Common")]
	public GameObject canvasObject;

	public PhysicMaterial physicMaterial;

	public Material lineMaterial;

	public Material handleMaterial;

	public LayerMask creationDraggingLayerMask;

	public float creationColliderScale = 0.85f;

	public Color dragValidLineColor = Color.green;

	public Color dragInvalidLineColor = Color.red;

	public Color dragValidPlaneColor = new Color(0f, 1f, 0f, 0.4f);

	public Color dragInvalidPlaneColor = new Color(1f, 0f, 0f, 0.4f);

	[Range(20f, 100f)]
	public float maxRigidbodySpeed = 40f;

	[Range(0f, 10f)]
	public float maxRigidbodyAngularSpeed = (float)Math.PI;

	public AnimationCurve speedScaleCurve;

	public float minSyncSqrDistance = 0.0001f;

	public float minSyncAngle = 0.1f;

	public float minSyncSqrSpeed = 0.0001f;

	public float minSyncSqrAngularSpeed = 5E-05f;

	public float minSyncSqrAimPoint = 0.001f;

	public float netDataApplyDamping = 0.2f;

	[Header("Sword & Bow & Axe & armor & Object")]
	public float swordStandardWeight = 100f;

	public float minStaminaCostRatioOfWeight = 0.5f;

	public float maxStaminaCostRatioOfWeight = 2f;

	public float bowDurabilityScale = 1f;

	public float bowDurabilityBase = 200f;

	public float axeDurabilityScale = 2f;

	public float axeAttackScale = 1f;

	public float axeCutDamageScale = 2f;

	public float armorDurabilityScale = 1f;

	public float maxArmorDurability = 100f;

	public float armorDamageRatio = 0.1f;

	public float maxArmorDefence = 10f;

	public float pivotRotateSpeed = 60f;

	public float bedLimitAngle = 30f;

	public float sleepTimeScale = 120f;

	public AnimationCurve swordAnimSpeedToASPD;

	public AnimationCurve axeAnimSpeedToASPD;

	public static float equipDurabilityShowScale = 0.05f;

	[Header("AI Turret")]
	public float aiTurretDurabilityScale = 50f;

	public float aiTurretMassScale = 0.1f;

	public float aiTurretMinMass = 10f;

	public float aiTurretMaxMass = 10000f;

	[Header("Robot")]
	public float robotDurabilityScale = 100f;

	public float robotAttackRange = 50f;

	public GameObject robotTrail;

	public float robotCureExpendEnergyPerSecond = 200f;

	public float robotMassScale = 0.1f;

	public float robotMinMass = 10f;

	public float robotMaxMass = 50f;

	public float robotStandardDrag = 0.1f;

	public float robotUnderwaterDrag = 1f;

	public float robotStandardAngularDrag = 0.1f;

	public float robotUnderwaterAngularDrag = 1f;

	public float robotMinHeight = 3f;

	public float robotMaxHeight = 5f;

	public float robotMinDistance = 2f;

	public float robotMaxDistance = 4f;

	public float robotSpeedScale = 1f;

	public float robotSwingRange = 0.1f;

	public float robotSwingPeriod = 1f;

	public float robotVelocityChangeSpeed = 5f;

	public float robotVelocityRotateSpeed = 3.14f;

	public float robotRotateSpeed = 90f;

	[Header("Carrier")]
	public LayerMask attackRayLayerMask = -1;

	public LayerMask getOffLayerMask = -1;

	public float treeHardness = 10000f;

	public float lockTargetDuration = 2f;

	public float maxJetAccelerate = 10f;

	[Range(0f, 1f)]
	public float jetIncreaseSpeed = 0.2f;

	[Range(0f, 1f)]
	public float jetDecreaseSpeed = 0.2f;

	[Range(0f, 5f)]
	public float jetDecToIncInterval = 1f;

	public float minPassengerDamage = 0.01f;

	public float maxPassengerDamage = 0.1f;

	[Header("Vehicle")]
	public float vehicleDurabilityScale = 1f;

	public float vehicleMassScale = 0.1f;

	public float vehicleMinMass = 1000f;

	public float vehicleMaxMass = 10000f;

	public Vector3 vehicleInertiaTensorScale;

	public float vehicleStandardDrag = 0.1f;

	public float vehicleUnderwaterDrag = 1f;

	public float vehicleStandardAngularDrag = 0.1f;

	public float vehicleUnderwaterAngularDrag = 1f;

	[Range(0f, 20f)]
	public float naturalFrequency = 10f;

	[Range(0f, 3f)]
	public float dampingRatio = 0.8f;

	[Range(-0.1f, 0.1f)]
	public float wheelForceAppPointOffset = -0.03f;

	public float maxWheelSteerAngle = 20f;

	public float vehicleSteerRadiusBase = 4f;

	public float vehicleSteerRadiusExtend = 4f;

	public float motorcycleBiasAngle = 25f;

	public float motorcycleBalanceHelp = 0.1f;

	public float sideStiffnessBase = 0.5f;

	public float sideStiffnessFactor = 0.5f;

	public float fwdStiffnessBase = 0.5f;

	public float fwdStiffnessFactor = 0.5f;

	public AnimationCurve motorForce;

	public AnimationCurve speedToRotateFactor;

	[Header("Helicopter")]
	public float helicopterDurabilityScale = 1f;

	public float helicopterMassScale = 0.1f;

	public float helicopterMinMass = 1000f;

	public float helicopterMaxMass = 200000f;

	public Vector3 helicopterInertiaTensorScale;

	public float helicopterStandardDrag = 0.1f;

	public float helicopterUnderwaterDrag = 1f;

	public float helicopterStandardAngularDrag = 0.1f;

	public float helicopterUnderwaterAngularDrag = 1f;

	public float rotorSteerHelp = 0.08f;

	public float thrusterSteerHelp = 0.03f;

	public float rotorAccelerateFactor = 1200f;

	public float rotorDecelerateFactor = 960f;

	public float rotorMaxRotateSpeed = 2901f;

	public float rotorDeflectSpeed = 20f;

	[Range(0f, 1f)]
	public float rotorBalanceAdjust = 0.75f;

	public AnimationCurve rotorBalaceScale;

	public float helicopterMaxUpSpeed = 10f;

	public float helicopterMaxDownSpeed = 5f;

	public float helicopterBalanceHelp = 10f;

	public float helicopterMaxHeight = 150f;

	public AnimationCurve liftForceFactor;

	public float maxLiftAccelerate = 10f;

	public float rotorEnergySpeed = 0.001f;

	public float thrusterEnergySpeed = 0.001f;

	[Header("Boat")]
	public float boatDurabilityScale = 1f;

	public float boatMassScale = 0.1f;

	public float boatMinMass = 1000f;

	public float boatMaxMass = 10000f;

	public float boatPropellerEnergySpeed = 0.0005f;

	public Vector3 boatInertiaTensorScale;

	public float boatStandardDrag = 0.1f;

	public float boatUnderwaterDrag = 1f;

	public float boatStandardAngularDrag = 0.1f;

	public float boatUnderwaterAngularDrag = 1f;

	public float boatBalanceHelp = 20f;

	public float buoyancyFactor = 10000f;

	public float submarineMaxUpSpeed = 8f;

	public float submarineMaxDownSpeed = 6f;

	[Header("Sound")]
	public AudioClip crashSound;

	public AudioClip explotionSound;

	public float minWeaponSoundInterval = 0.05f;

	[Range(0f, 1f)]
	public float explotionVolume = 1f;

	[Range(0f, 1f)]
	public float crashVolume = 1f;

	public static PEVCConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<PEVCConfig>("PEVCConfig");
			}
			return _instance;
		}
	}

	public float maxSqrRigidbodySpeed => maxRigidbodySpeed * maxRigidbodySpeed;

	public float randomRobotDistance => UnityEngine.Random.Range(robotMinDistance, robotMaxDistance);

	public float randomRobotHeight => UnityEngine.Random.Range(robotMinHeight, robotMaxHeight);

	public float sqrRobotAttackRange => robotAttackRange * robotAttackRange;

	public float randomPassengerDamage => UnityEngine.Random.Range(minPassengerDamage, maxPassengerDamage);
}
