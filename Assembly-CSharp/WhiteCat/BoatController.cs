using System.Collections.Generic;
using UnityEngine;

namespace WhiteCat;

public class BoatController : CarrierController
{
	private VCPShipPropeller[] _propellers;

	private VCPShipRudder[] _rudders;

	private VCPSubmarineBallastTank[] _tanks;

	protected override float mass => Mathf.Clamp(base.creationController.creationData.m_Attribute.m_Weight * PEVCConfig.instance.boatMassScale, PEVCConfig.instance.boatMinMass, PEVCConfig.instance.boatMaxMass);

	protected override Vector3 centerOfMass => new Vector3(0f, base.creationController.creationData.m_Attribute.m_CenterOfMass.y, 0f);

	protected override Vector3 inertiaTensorScale => PEVCConfig.instance.boatInertiaTensorScale;

	protected override void InitDrags(out float standardDrag, out float underwaterDrag, out float standardAngularDrag, out float underwaterAngularDrag)
	{
		standardDrag = PEVCConfig.instance.boatStandardDrag;
		underwaterDrag = PEVCConfig.instance.boatUnderwaterDrag;
		standardAngularDrag = PEVCConfig.instance.boatStandardAngularDrag;
		underwaterAngularDrag = PEVCConfig.instance.boatUnderwaterAngularDrag;
	}

	protected override void InitOtherThings()
	{
		base.InitOtherThings();
		LoadParts(ref _propellers);
		LoadParts(ref _rudders);
		LoadParts(ref _tanks);
		base.gameObject.AddComponent<CreationWaterMask>();
		if (_propellers != null && _propellers.Length > 0)
		{
			VCPShipPropeller[] propellers = _propellers;
			foreach (VCPShipPropeller vCPShipPropeller in propellers)
			{
				vCPShipPropeller.Init(this, _tanks != null && _tanks.Length > 0);
			}
		}
		if (_rudders != null && _rudders.Length > 0)
		{
			VCPShipRudder[] rudders = _rudders;
			foreach (VCPShipRudder vCPShipRudder in rudders)
			{
				vCPShipRudder.Init(this);
			}
		}
		if (_tanks != null && _tanks.Length > 0)
		{
			VCPSubmarineBallastTank[] tanks = _tanks;
			foreach (VCPSubmarineBallastTank vCPSubmarineBallastTank in tanks)
			{
				vCPSubmarineBallastTank.Init(this);
			}
		}
		InitWave();
	}

	private void InitWave()
	{
		Transform transform = new GameObject("Wave").transform;
		transform.SetParent(base.creationController.centerObject, worldPositionStays: false);
		transform.localPosition = new Vector3(base.creationController.bounds.size.x * 0.25f, (0f - base.creationController.bounds.size.y) * 0.5f, 0f);
		PEWaterLineWaveTracer pEWaterLineWaveTracer = transform.gameObject.AddComponent<PEWaterLineWaveTracer>();
		pEWaterLineWaveTracer.TracerTrans = transform;
		pEWaterLineWaveTracer.Height = base.creationController.bounds.size.y;
		pEWaterLineWaveTracer.Width = base.creationController.bounds.size.x;
		pEWaterLineWaveTracer.Length = base.creationController.bounds.size.z;
		pEWaterLineWaveTracer.WaveSpeed = 0.15f;
		pEWaterLineWaveTracer.AutoGenWave = true;
		pEWaterLineWaveTracer.WaveDuration = 15f;
		pEWaterLineWaveTracer.Frequency = 25f;
		pEWaterLineWaveTracer.Strength = 40f;
		pEWaterLineWaveTracer.Frequency = 30f;
		pEWaterLineWaveTracer.TimeOffsetFactor = 8f;
		pEWaterLineWaveTracer.IntervalTime = 0.4f;
		pEWaterLineWaveTracer.DeltaTime = 0.3f;
		pEWaterLineWaveTracer.ScaleRate = 0.0015f;
		pEWaterLineWaveTracer.DefualtScale = 4f;
		pEWaterLineWaveTracer.DefualtScaleFactor = 10f;
		pEWaterLineWaveTracer.Distance = 512f;
		pEWaterLineWaveTracer.Desc = "Boat wave";
		pEWaterLineWaveTracer.IsValid = true;
		transform = Object.Instantiate(transform.gameObject).transform;
		transform.SetParent(base.creationController.centerObject, worldPositionStays: false);
		transform.localPosition = new Vector3((0f - base.creationController.bounds.size.x) * 0.25f, (0f - base.creationController.bounds.size.y) * 0.5f, 0f);
	}

	public float FluidDisplacement(Vector3 wpos)
	{
		if (VFVoxelWater.self != null && VFVoxelWater.self.Voxels != null)
		{
			int x = Mathf.RoundToInt(wpos.x);
			int z = Mathf.RoundToInt(wpos.z);
			float num = Mathf.Floor(wpos.y - 0.5f) + 0.5f;
			float t = wpos.y - num;
			int num2 = Mathf.FloorToInt(num) + 1;
			int y = num2 + 1;
			VFVoxel vFVoxel = VFVoxelWater.self.Voxels.SafeRead(x, num2, z);
			VFVoxel vFVoxel2 = VFVoxelWater.self.Voxels.SafeRead(x, y, z);
			return Mathf.Clamp01(Mathf.Lerp((int)vFVoxel.Volume, (int)vFVoxel2.Volume, t) / 255f);
		}
		return 0f;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		float num = (1f - Mathf.Clamp(base.rigidbody.velocity.y, 0f, 10f) * 0.1f) * PEVCConfig.instance.buoyancyFactor;
		float num2 = 0.16f * base.underWaterFactor * (1f - base.underWaterFactor);
		num *= Mathf.Sin(Time.timeSinceLevelLoad * 3f) * num2 + (1f - num2);
		Vector3 zero = Vector3.zero;
		List<VolumePoint> fluidDisplacement = base.creationController.creationData.m_Attribute.m_FluidDisplacement;
		for (int i = 0; i < fluidDisplacement.Count; i++)
		{
			VolumePoint volumePoint = fluidDisplacement[i];
			Vector3 vector = base.transform.TransformPoint(volumePoint.localPosition);
			zero.y = volumePoint.pos_volume * FluidDisplacement(vector) * num;
			base.rigidbody.AddForceAtPosition(zero, vector);
		}
	}
}
