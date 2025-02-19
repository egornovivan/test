using System;
using UnityEngine;
using WhiteCat;

[Serializable]
public abstract class VCComponentData
{
	public int m_ComponentId;

	public int m_ExtendData;

	public EVCComponent m_Type;

	public Vector3 m_Position = Vector3.zero;

	public Vector3 m_Rotation = Vector3.zero;

	public Vector3 m_Scale = Vector3.one;

	public bool m_Visible = true;

	public GameObject m_Entity;

	public VCIsoData m_CurrIso;

	public abstract void Validate();

	public abstract void Import(byte[] buffer);

	public abstract byte[] Export();

	protected void PositionValidate()
	{
		if (m_CurrIso != null)
		{
			m_Position = m_CurrIso.ClampPointWorldCoord(m_Position);
		}
	}

	public virtual VCComponentData Copy()
	{
		VCComponentData vCComponentData = Create(m_Type, Export());
		vCComponentData.m_Entity = m_Entity;
		vCComponentData.m_CurrIso = m_CurrIso;
		return vCComponentData;
	}

	public abstract void UpdateEntity(bool for_editor);

	public abstract GameObject CreateEntity(bool for_editor, Transform parent);

	public void DestroyEntity()
	{
		if (m_Entity != null)
		{
			UnityEngine.Object.Destroy(m_Entity);
			m_Entity = null;
		}
	}

	public void UpdateComponent()
	{
		VCPart component = m_Entity.GetComponent<VCPart>();
		if ((bool)component)
		{
			UpdateComponent(component);
		}
	}

	protected virtual void UpdateComponent(VCPart part)
	{
		part.hiddenModel = !m_Visible;
		if (part is VCPWeapon)
		{
			(part as VCPWeapon).groupIndex = m_ExtendData;
		}
	}

	public static VCComponentData Create(VCPartInfo part)
	{
		if (part == null)
		{
			return null;
		}
		VCComponentData vCComponentData = Create(part.m_Type);
		vCComponentData.m_ComponentId = part.m_ID;
		vCComponentData.m_Type = part.m_Type;
		vCComponentData.m_Position = Vector3.zero;
		vCComponentData.m_Rotation = Vector3.zero;
		vCComponentData.m_Scale = Vector3.one;
		vCComponentData.m_Visible = true;
		return vCComponentData;
	}

	public static VCComponentData CreateDecal()
	{
		return Create(EVCComponent.cpDecal);
	}

	public static VCComponentData CreateEffect()
	{
		return Create(EVCComponent.cpEffect);
	}

	public static VCComponentData Create(EVCComponent type)
	{
		switch (type)
		{
		default:
			return null;
		case EVCComponent.cpVehicleFuelCell:
		case EVCComponent.cpVtolRotor:
		case EVCComponent.cpVtolFuelCell:
		case EVCComponent.cpHeadLight:
		case EVCComponent.cpCtrlTurret:
		case EVCComponent.cpMissileLauncher:
		case EVCComponent.cpAITurretWeapon:
		case EVCComponent.cpShipPropeller:
		case EVCComponent.cpShipRudder:
		case EVCComponent.cpSubmarineBallastTank:
		case EVCComponent.cpRobotController:
		case EVCComponent.cpRobotBattery:
		case EVCComponent.cpRobotWeapon:
		case EVCComponent.cpAirshipThruster:
			return new VCFreePartData();
		case EVCComponent.cpSwordHilt:
		case EVCComponent.cpShieldHandle:
		case EVCComponent.cpGunMuzzle:
		case EVCComponent.cpVehicleCockpit:
		case EVCComponent.cpVehicleEngine:
		case EVCComponent.cpVtolCockpit:
		case EVCComponent.cpSideSeat:
		case EVCComponent.cpJetExhaust:
		case EVCComponent.cpShipCockpit:
		case EVCComponent.cpBowGrip:
		case EVCComponent.cpAxeHilt:
		case EVCComponent.cpLgSwordHilt:
		case EVCComponent.cpBed:
		case EVCComponent.cpHeadPivot:
		case EVCComponent.cpBodyPivot:
		case EVCComponent.cpArmAndLegPivot:
		case EVCComponent.cpHandAndFootPivot:
		case EVCComponent.cpDecorationPivot:
			return new VCGeneralPartData();
		case EVCComponent.cpDbSwordHilt:
			return new VCFixedHandPartData();
		case EVCComponent.cpGunHandle:
			return new VCFixedPartData();
		case EVCComponent.cpFrontCannon:
			return new VCAsymmetricGeneralPartData();
		case EVCComponent.cpLandingGear:
			return new VCTriphaseFixedPartData();
		case EVCComponent.cpVehicleWheel:
			return new VCQuadphaseFixedPartData();
		case EVCComponent.cpDecal:
			return new VCDecalData();
		case EVCComponent.cpLight:
			return new VCObjectLightData();
		case EVCComponent.cpPivot:
			return new VCObjectPivotData();
		}
	}

	public static VCComponentData Create(EVCComponent type, byte[] buffer)
	{
		VCComponentData vCComponentData = Create(type);
		vCComponentData?.Import(buffer);
		return vCComponentData;
	}
}
