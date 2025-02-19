using Pathea;
using UnityEngine;

namespace WhiteCat;

public class HelicopterController : CarrierController
{
	private VCPVtolRotor[] _vtolRotors;

	private VCPVtolThruster[] _vtolThrusters;

	private float _liftForceFactor;

	public float liftForceFactor => _liftForceFactor;

	protected override float mass => Mathf.Clamp(base.creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.helicopterMassScale, PEVCConfig.instance.helicopterMinMass, PEVCConfig.instance.helicopterMaxMass);

	protected override Vector3 centerOfMass => new Vector3(0f, base.creationController.creationData.m_Attribute.m_CenterOfMass.y, 0f);

	protected override Vector3 inertiaTensorScale => PEVCConfig.instance.helicopterInertiaTensorScale;

	protected override void InitDrags(out float standardDrag, out float underwaterDrag, out float standardAngularDrag, out float underwaterAngularDrag)
	{
		standardDrag = PEVCConfig.instance.helicopterStandardDrag;
		underwaterDrag = PEVCConfig.instance.helicopterUnderwaterDrag;
		standardAngularDrag = PEVCConfig.instance.helicopterStandardAngularDrag;
		underwaterAngularDrag = PEVCConfig.instance.helicopterUnderwaterAngularDrag;
	}

	protected override void InitOtherThings()
	{
		base.InitOtherThings();
		LoadParts(ref _vtolRotors);
		LoadParts(ref _vtolThrusters);
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
		if (_vtolRotors != null && _vtolRotors.Length > 0)
		{
			VCPVtolRotor[] vtolRotors = _vtolRotors;
			foreach (VCPVtolRotor vCPVtolRotor in vtolRotors)
			{
				vCPVtolRotor.Init(this);
			}
			int[,] array = new int[2, 6];
			VCPVtolRotor[] vtolRotors2 = _vtolRotors;
			foreach (VCPVtolRotor vCPVtolRotor2 in vtolRotors2)
			{
				array[vCPVtolRotor2.sizeType, vCPVtolRotor2.directionType]++;
			}
			VCPVtolRotor[] vtolRotors3 = _vtolRotors;
			foreach (VCPVtolRotor vCPVtolRotor3 in vtolRotors3)
			{
				vCPVtolRotor3.InitSoundScale(array[vCPVtolRotor3.sizeType, vCPVtolRotor3.directionType]);
			}
		}
		if (_vtolThrusters != null && _vtolThrusters.Length > 0)
		{
			VCPVtolThruster[] vtolThrusters = _vtolThrusters;
			foreach (VCPVtolThruster vCPVtolThruster in vtolThrusters)
			{
				vCPVtolThruster.Init(this);
			}
		}
		float num = 0f;
		for (int m = 0; m < _vtolRotors.Length; m++)
		{
			num += _vtolRotors[m].maxLiftForce;
		}
		for (int n = 0; n < _vtolThrusters.Length; n++)
		{
			num += _vtolThrusters[n].maxLiftForce;
		}
		float num2 = 1f;
		if (num > 1f)
		{
			float num3 = Mathf.Min(num / base.rigidbody.mass + Physics.gravity.y, PEVCConfig.instance.maxLiftAccelerate);
			num2 = (num3 - Physics.gravity.y) * base.rigidbody.mass / num;
		}
		float upValue = num2 * PEVCConfig.instance.rotorMaxRotateSpeed;
		for (int num4 = 0; num4 < _vtolRotors.Length; num4++)
		{
			_vtolRotors[num4].InitMaxRotateSpeed(upValue);
		}
		for (int num5 = 0; num5 < _vtolThrusters.Length; num5++)
		{
			_vtolThrusters[num5].InitMaxForceRatio(num2);
		}
		base.transform.position = position;
		base.transform.rotation = rotation;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		Vector3 vector = ((!(Vector3.Dot(base.transform.up, Vector3.up) > 0f)) ? Vector3.Cross(base.transform.up, Vector3.up).normalized : Vector3.Cross(base.transform.up, Vector3.up));
		base.rigidbody.AddTorque(base.rigidbody.mass * PEVCConfig.instance.helicopterBalanceHelp * vector);
		Vector3 position = base.transform.position;
		float num = GetMaxTerrainHeight();
		if (num < 100f)
		{
			num = 100f;
		}
		_liftForceFactor = PEVCConfig.instance.liftForceFactor.Evaluate((position.y - num) / PEVCConfig.instance.helicopterMaxHeight);
	}

	public float GetMaxTerrainHeight()
	{
		Vector3 position = base.transform.position;
		float terrainHeight = PeSingleton<PeMappingMgr>.Instance.GetTerrainHeight(position);
		position.x += 100f;
		terrainHeight = Mathf.Max(PeSingleton<PeMappingMgr>.Instance.GetTerrainHeight(position), terrainHeight);
		position.x -= 200f;
		terrainHeight = Mathf.Max(PeSingleton<PeMappingMgr>.Instance.GetTerrainHeight(position), terrainHeight);
		position.x += 100f;
		position.y += 100f;
		terrainHeight = Mathf.Max(PeSingleton<PeMappingMgr>.Instance.GetTerrainHeight(position), terrainHeight);
		position.y -= 200f;
		return Mathf.Max(PeSingleton<PeMappingMgr>.Instance.GetTerrainHeight(position), terrainHeight);
	}
}
