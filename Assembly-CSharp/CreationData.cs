using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using SkillAsset;
using UnityEngine;
using WhiteCat;

public class CreationData
{
	public const int ObjectStartID = 100000000;

	public int m_ObjectID;

	public byte[] m_Resource;

	public float m_RandomSeed;

	public VCIsoData m_IsoData;

	public ulong m_HashCode;

	public CreationAttr m_Attribute;

	private DateTime m_LastUseTime;

	public GameObject m_Prefab;

	private GameObject m_Root;

	private GameObject m_PartGroup;

	private GameObject m_MeshGroup;

	private GameObject m_DecalGroup;

	private GameObject m_EffectGroup;

	private GameObject m_RootL;

	private GameObject m_PartGroupL;

	private GameObject m_MeshGroupL;

	private GameObject m_DecalGroupL;

	private GameObject m_EffectGroupL;

	public ulong m_MatGUID;

	public CreationController creationController;

	public VCMeshMgr m_MeshMgr;

	public VCMeshMgr m_MeshMgrL;

	public static int s_SubSkillStartID = 600000000;

	public static int s_BuffStartID = 100000000;

	public string HashString => m_HashCode.ToString("X").PadLeft(16, '0');

	public void UpdateUseTime()
	{
		m_LastUseTime = DateTime.Now;
	}

	public bool TooLongNoUse()
	{
		if (DateTime.Now.Subtract(m_LastUseTime).TotalSeconds > 300.0)
		{
			return true;
		}
		return false;
	}

	public void Destroy()
	{
		m_ObjectID = 0;
		m_Resource = null;
		m_RandomSeed = 0f;
		m_HashCode = 0uL;
		if (m_IsoData != null)
		{
			m_IsoData.Destroy();
			m_IsoData = null;
		}
		m_Attribute = null;
		DestroyPrefab();
	}

	public void GenCreationAttr()
	{
		CalcCreationAttr(m_IsoData, m_RandomSeed, ref m_Attribute);
	}

	public static void CalcCreationAttr(VCIsoData iso, float random_seed, ref CreationAttr attr)
	{
		attr = new CreationAttr();
		CalcCreationType(iso, attr);
		CalcCommonAttr(iso, attr);
		switch (attr.m_Type)
		{
		case ECreation.Sword:
		case ECreation.SwordLarge:
		case ECreation.SwordDouble:
			CalcSwordAttr(iso, random_seed, attr);
			break;
		case ECreation.Bow:
			CalcBowAttr(iso, random_seed, attr);
			break;
		case ECreation.Axe:
			CalcSwordAttr(iso, random_seed, attr);
			attr.m_Durability *= PEVCConfig.instance.axeDurabilityScale;
			break;
		case ECreation.Shield:
			CalcShieldAttr(iso, random_seed, attr);
			break;
		case ECreation.HandGun:
		case ECreation.Rifle:
			CalcGunAttr(iso, random_seed, attr);
			break;
		case ECreation.Vehicle:
			CalcVehicleAttr(iso, random_seed, attr);
			break;
		case ECreation.Aircraft:
			CalcAircraftAttr(iso, random_seed, attr);
			break;
		case ECreation.Boat:
			CalcBoatAttr(iso, random_seed, attr);
			break;
		case ECreation.AITurret:
			CalcAITurretAttr(iso, random_seed, attr);
			break;
		case ECreation.SimpleObject:
			CalcSimpleObjectAttr(iso, random_seed, attr);
			break;
		case ECreation.Robot:
			CalcRobotAttr(iso, random_seed, attr);
			break;
		case ECreation.ArmorHead:
		case ECreation.ArmorBody:
		case ECreation.ArmorArmAndLeg:
		case ECreation.ArmorHandAndFoot:
		case ECreation.ArmorDecoration:
			CalcArmorAttr(iso, random_seed, attr);
			break;
		}
	}

	public static void CalcCreationType(VCIsoData iso, CreationAttr attr)
	{
		attr.m_Type = ECreation.Null;
		bool flag = false;
		foreach (KeyValuePair<int, VCVoxel> voxel in iso.m_Voxels)
		{
			if (voxel.Value.Volume >= 128)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			attr.m_Errors.Add("No visible material voxels".ToLocalizationString());
		}
		switch (iso.m_HeadInfo.Category)
		{
		case EVCCategory.cgSword:
		{
			int num16 = 0;
			foreach (VCComponentData component in iso.m_Components)
			{
				if (component.m_Type == EVCComponent.cpSwordHilt)
				{
					num16++;
				}
			}
			if (num16 == 0)
			{
				attr.m_Errors.Add("No hilt".ToLocalizationString());
			}
			if (num16 > 1)
			{
				attr.m_Errors.Add("Too many hilts, max is 1".ToLocalizationString());
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.Sword;
			}
			break;
		}
		case EVCCategory.cgLgSword:
		{
			int num47 = 0;
			foreach (VCComponentData component2 in iso.m_Components)
			{
				if (component2.m_Type == EVCComponent.cpLgSwordHilt)
				{
					num47++;
				}
			}
			if (num47 == 0)
			{
				attr.m_Errors.Add("No hilt".ToLocalizationString());
			}
			if (num47 > 1)
			{
				attr.m_Errors.Add("Too many hilts, max is 1".ToLocalizationString());
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.SwordLarge;
			}
			break;
		}
		case EVCCategory.cgDbSword:
		{
			int num11 = 0;
			int num12 = 0;
			int num13 = 0;
			int num14 = 0;
			int num15 = 0;
			foreach (VCComponentData component3 in iso.m_Components)
			{
				if (component3.m_Type == EVCComponent.cpDbSwordHilt)
				{
					num11++;
				}
				if (component3 is VCFixedHandPartData)
				{
					if ((component3 as VCFixedHandPartData).m_LeftHand)
					{
						num12++;
					}
					else
					{
						num13++;
					}
					if (component3.m_Position.x < (0.5f * (float)iso.m_HeadInfo.xSize + 5f) * iso.m_HeadInfo.FindSceneSetting().m_VoxelSize)
					{
						num14++;
					}
					if (component3.m_Position.x > (0.5f * (float)iso.m_HeadInfo.xSize - 5f) * iso.m_HeadInfo.FindSceneSetting().m_VoxelSize)
					{
						num15++;
					}
				}
			}
			switch (num11)
			{
			case 0:
				attr.m_Errors.Add(PELocalization.GetString(9500449));
				break;
			case 1:
				attr.m_Errors.Add(PELocalization.GetString(9500449));
				break;
			case 2:
				if (num12 == 0)
				{
					attr.m_Errors.Add(PELocalization.GetString(9500447));
				}
				if (num13 == 0)
				{
					attr.m_Errors.Add(PELocalization.GetString(9500448));
				}
				break;
			}
			if (num14 > 1 || num15 > 1)
			{
				attr.m_Errors.Add(PELocalization.GetString(9500446));
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.SwordDouble;
			}
			break;
		}
		case EVCCategory.cgBow:
		{
			int num52 = 0;
			foreach (VCComponentData component4 in iso.m_Components)
			{
				if (component4.m_Type == EVCComponent.cpBowGrip)
				{
					num52++;
				}
			}
			if (num52 == 0)
			{
				attr.m_Errors.Add("No bow grip".ToLocalizationString());
			}
			if (num52 > 1)
			{
				attr.m_Errors.Add("Too many bow grips, max is 1".ToLocalizationString());
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.Bow;
			}
			break;
		}
		case EVCCategory.cgAxe:
		{
			int num43 = 0;
			foreach (VCComponentData component5 in iso.m_Components)
			{
				if (component5.m_Type == EVCComponent.cpAxeHilt)
				{
					num43++;
				}
			}
			if (num43 == 0)
			{
				attr.m_Errors.Add("No hilt".ToLocalizationString());
			}
			if (num43 > 1)
			{
				attr.m_Errors.Add("Too many hilts, max is 1".ToLocalizationString());
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.Axe;
			}
			break;
		}
		case EVCCategory.cgShield:
		{
			int num39 = 0;
			foreach (VCComponentData component6 in iso.m_Components)
			{
				if (component6.m_Type == EVCComponent.cpShieldHandle)
				{
					num39++;
				}
			}
			if (num39 == 0)
			{
				attr.m_Errors.Add("No handle".ToLocalizationString());
			}
			if (num39 > 1)
			{
				attr.m_Errors.Add("Too many handles, max is 1".ToLocalizationString());
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.Shield;
			}
			break;
		}
		case EVCCategory.cgGun:
		{
			int num48 = 0;
			int num49 = 0;
			VCPGunHandle vCPGunHandle = null;
			VCPGunMuzzle vCPGunMuzzle = null;
			foreach (VCComponentData component7 in iso.m_Components)
			{
				if (component7.m_Type == EVCComponent.cpGunHandle)
				{
					vCPGunHandle = VCConfig.s_Parts[component7.m_ComponentId].m_ResObj.GetComponent<VCPGunHandle>();
					num48++;
				}
				else if (component7.m_Type == EVCComponent.cpGunMuzzle)
				{
					vCPGunMuzzle = VCConfig.s_Parts[component7.m_ComponentId].m_ResObj.GetComponent<VCPGunMuzzle>();
					num49++;
				}
			}
			if (num48 == 0)
			{
				attr.m_Errors.Add("No handle".ToLocalizationString());
			}
			if (num48 > 1)
			{
				attr.m_Errors.Add("Too many handles, max is 1".ToLocalizationString());
			}
			bool flag3 = false;
			int num50 = 0;
			int num51 = 0;
			if (vCPGunHandle != null)
			{
				flag3 = vCPGunHandle.DualHand;
				num50 = vCPGunHandle.GunType;
			}
			if (vCPGunMuzzle != null)
			{
				num51 = vCPGunMuzzle.GunType;
			}
			if (num49 == 0)
			{
				attr.m_Errors.Add("No muzzle".ToLocalizationString());
			}
			if (num49 > 1)
			{
				attr.m_Errors.Add("Too many muzzles, max is 1".ToLocalizationString());
			}
			if (num50 != num51)
			{
				attr.m_Errors.Add("Gun type mismatch".ToLocalizationString());
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ((!flag3) ? ECreation.HandGun : ECreation.Rifle);
			}
			break;
		}
		case EVCCategory.cgVehicle:
		{
			int num18 = 0;
			int num19 = 0;
			int num20 = 0;
			int num21 = 0;
			int num22 = 0;
			int num23 = 0;
			int num24 = 0;
			int num25 = 0;
			int num26 = 0;
			int num27 = 0;
			int num28 = 0;
			foreach (VCComponentData component8 in iso.m_Components)
			{
				if (component8.m_Type == EVCComponent.cpVehicleCockpit)
				{
					num18++;
				}
				else if (component8.m_Type == EVCComponent.cpVehicleWheel)
				{
					num19++;
					if ((component8 as VCQuadphaseFixedPartData).isSteerWheel)
					{
						num20++;
					}
					if ((component8 as VCQuadphaseFixedPartData).isMotorWheel)
					{
						num21++;
					}
				}
				else if (component8.m_Type == EVCComponent.cpVehicleFuelCell)
				{
					num22++;
				}
				else if (component8.m_Type == EVCComponent.cpVehicleEngine)
				{
					num23++;
				}
				else if (component8.m_Type == EVCComponent.cpJetExhaust)
				{
					num24++;
				}
				else if (component8.m_Type == EVCComponent.cpCtrlTurret)
				{
					num25++;
				}
				else if (component8.m_Type == EVCComponent.cpFrontCannon)
				{
					num26++;
				}
				else if (component8.m_Type == EVCComponent.cpMissileLauncher)
				{
					num27++;
				}
			}
			if (num18 != 1)
			{
				attr.m_Errors.Add("Zero or more than one cockpit".ToLocalizationString());
			}
			if (num19 == 0)
			{
				attr.m_Errors.Add("No wheel".ToLocalizationString());
			}
			else if (num19 == 1)
			{
				attr.m_Warnings.Add("Need more wheel(s) to keep balance".ToLocalizationString());
			}
			else if (num19 > 16)
			{
				attr.m_Errors.Add("Too many wheels, max is 16".ToLocalizationString());
			}
			if (num21 == 0)
			{
				attr.m_Errors.Add("No motor wheel".ToLocalizationString());
			}
			switch (num20)
			{
			case 0:
				attr.m_Errors.Add("No steering wheel".ToLocalizationString());
				break;
			case 1:
				attr.m_Warnings.Add("More than one steering wheel needed".ToLocalizationString());
				break;
			}
			if (num22 == 0)
			{
				attr.m_Errors.Add("No fuel cell".ToLocalizationString());
			}
			else if (num22 > 8)
			{
				attr.m_Errors.Add("Too many fuel cells, max is".ToLocalizationString() + " 8");
			}
			if (num23 != 1)
			{
				attr.m_Errors.Add("Zero or more than one engine".ToLocalizationString());
			}
			if (num24 > 4)
			{
				attr.m_Errors.Add("Too many jet exhausts, max is".ToLocalizationString() + " 4");
			}
			if (num25 > 20)
			{
				attr.m_Errors.Add("Too many turrets, max is".ToLocalizationString() + " 20");
			}
			if (num26 > 10)
			{
				attr.m_Errors.Add("Too many canons, max is".ToLocalizationString() + " 10");
			}
			if (num27 > 2)
			{
				attr.m_Errors.Add("Too many missiles, max is".ToLocalizationString() + " 2");
			}
			if (num28 > 8)
			{
				attr.m_Errors.Add("Too many ai-towers, max is".ToLocalizationString() + " 8");
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.Vehicle;
			}
			break;
		}
		case EVCCategory.cgAircraft:
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			int num9 = 0;
			int num10 = 0;
			foreach (VCComponentData component9 in iso.m_Components)
			{
				if (component9.m_Type == EVCComponent.cpVtolCockpit)
				{
					num2++;
				}
				else if (component9.m_Type == EVCComponent.cpVtolRotor)
				{
					num3++;
				}
				else if (component9.m_Type == EVCComponent.cpVtolFuelCell || component9.m_Type == EVCComponent.cpVehicleFuelCell)
				{
					num4++;
				}
				else if (component9.m_Type == EVCComponent.cpJetExhaust)
				{
					num5++;
				}
				else if (component9.m_Type == EVCComponent.cpAirshipThruster)
				{
					num6++;
				}
				else if (component9.m_Type == EVCComponent.cpCtrlTurret)
				{
					num7++;
				}
				else if (component9.m_Type == EVCComponent.cpFrontCannon)
				{
					num8++;
				}
				else if (component9.m_Type == EVCComponent.cpMissileLauncher)
				{
					num9++;
				}
			}
			VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
			bool flag2 = vCESceneSetting.m_EditorSize.z > 200;
			if (num2 == 0)
			{
				attr.m_Errors.Add("No cockpit".ToLocalizationString());
			}
			if (num2 > 1)
			{
				attr.m_Errors.Add("Too many cockpits, max is".ToLocalizationString() + " 1");
			}
			if (num3 + num6 == 0)
			{
				attr.m_Errors.Add("No rotor or thruster".ToLocalizationString());
			}
			else if (num3 + num6 > 32)
			{
				attr.m_Errors.Add("Too many rotors, max is".ToLocalizationString() + " 32");
			}
			if (num4 == 0)
			{
				attr.m_Errors.Add("No fuel cell".ToLocalizationString());
			}
			else if (num4 > ((!flag2) ? 8 : 32))
			{
				attr.m_Errors.Add("Too many fuel cells, max is".ToLocalizationString() + " " + ((!flag2) ? 8 : 32));
			}
			if (num5 > ((!flag2) ? 16 : 32))
			{
				attr.m_Errors.Add("Too many jet exhausts, max is".ToLocalizationString() + " " + ((!flag2) ? 16 : 32));
			}
			if (num6 > ((!flag2) ? 2 : 16))
			{
				attr.m_Errors.Add("Too many thrusters, max is".ToLocalizationString() + " " + ((!flag2) ? 2 : 16));
			}
			if (num7 > 20)
			{
				attr.m_Errors.Add("Too many turrets, max is".ToLocalizationString() + " 20");
			}
			if (num8 > 10)
			{
				attr.m_Errors.Add("Too many canons, max is".ToLocalizationString() + " 10");
			}
			if (num9 > 2)
			{
				attr.m_Errors.Add("Too many missiles, max is".ToLocalizationString() + " 2");
			}
			if (num10 > 8)
			{
				attr.m_Errors.Add("Too many ai-towers, max is".ToLocalizationString() + " 8");
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.Aircraft;
			}
			break;
		}
		case EVCCategory.cgBoat:
		{
			int num29 = 0;
			int num30 = 0;
			int num31 = 0;
			int num32 = 0;
			int num33 = 0;
			int num34 = 0;
			int num35 = 0;
			int num36 = 0;
			int num37 = 0;
			int num38 = 0;
			foreach (VCComponentData component10 in iso.m_Components)
			{
				if (component10.m_Type == EVCComponent.cpShipCockpit)
				{
					num29++;
				}
				else if (component10.m_Type == EVCComponent.cpShipPropeller)
				{
					num30++;
				}
				else if (component10.m_Type == EVCComponent.cpVehicleFuelCell || component10.m_Type == EVCComponent.cpVtolFuelCell)
				{
					num31++;
				}
				else if (component10.m_Type == EVCComponent.cpJetExhaust)
				{
					num32++;
				}
				else if (component10.m_Type == EVCComponent.cpShipRudder)
				{
					num33++;
				}
				else if (component10.m_Type == EVCComponent.cpCtrlTurret)
				{
					num35++;
				}
				else if (component10.m_Type == EVCComponent.cpFrontCannon)
				{
					num36++;
				}
				else if (component10.m_Type == EVCComponent.cpMissileLauncher)
				{
					num37++;
				}
				else if (component10.m_Type == EVCComponent.cpSubmarineBallastTank)
				{
					num34++;
				}
			}
			if (num29 == 0)
			{
				attr.m_Errors.Add("No cockpit".ToLocalizationString());
			}
			if (num29 > 1)
			{
				attr.m_Errors.Add("Too many cockpits, max is".ToLocalizationString() + " 1");
			}
			if (num30 == 0)
			{
				attr.m_Errors.Add("No propeller".ToLocalizationString());
			}
			else if (num30 > 32)
			{
				attr.m_Errors.Add("Too many propellers, max is".ToLocalizationString() + " 32");
			}
			if (num31 == 0)
			{
				attr.m_Errors.Add("No fuel cell".ToLocalizationString());
			}
			else if (num31 > 16)
			{
				attr.m_Errors.Add("Too many fuel cells, max is".ToLocalizationString() + " 16");
			}
			if (num32 > 4)
			{
				attr.m_Errors.Add("Too many jet exhausts, max is".ToLocalizationString() + " 4");
			}
			if (num35 > 20)
			{
				attr.m_Errors.Add("Too many turrets, max is".ToLocalizationString() + " 20");
			}
			if (num36 > 10)
			{
				attr.m_Errors.Add("Too many canons, max is".ToLocalizationString() + " 10");
			}
			if (num37 > 2)
			{
				attr.m_Errors.Add("Too many missiles, max is".ToLocalizationString() + " 2");
			}
			if (num38 > 8)
			{
				attr.m_Errors.Add("Too many ai-towers, max is".ToLocalizationString() + " 8");
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.Boat;
			}
			break;
		}
		case EVCCategory.cgObject:
		{
			int num40 = 0;
			int num41 = 0;
			int num42 = 0;
			foreach (VCComponentData component11 in iso.m_Components)
			{
				if (component11.m_Type == EVCComponent.cpBed)
				{
					num40++;
				}
				if (component11.m_Type == EVCComponent.cpLight)
				{
					num41++;
				}
				if (component11.m_Type == EVCComponent.cpPivot)
				{
					num42++;
				}
			}
			if (num40 > 4)
			{
				attr.m_Errors.Add("Too many beds, max is".ToLocalizationString() + " 4");
			}
			if (num41 > 4)
			{
				attr.m_Errors.Add("Too many lights, max is".ToLocalizationString() + " 4");
			}
			if (num42 > 1)
			{
				attr.m_Errors.Add("Too many pivots, max is".ToLocalizationString() + " 1");
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.SimpleObject;
			}
			else
			{
				attr.m_Type = ECreation.Null;
			}
			break;
		}
		case EVCCategory.cgRobot:
		{
			int num44 = 0;
			int num45 = 0;
			int num46 = 0;
			foreach (VCComponentData component12 in iso.m_Components)
			{
				if (component12.m_Type == EVCComponent.cpRobotController)
				{
					num44++;
				}
				else if (component12.m_Type == EVCComponent.cpRobotBattery)
				{
					num45++;
				}
				else if (component12.m_Type == EVCComponent.cpRobotWeapon)
				{
					num46++;
				}
			}
			if (num44 != 1)
			{
				attr.m_Errors.Add("Zero or more than 1 controller".ToLocalizationString());
			}
			if (num45 != 1)
			{
				attr.m_Errors.Add("Zero or more than 1 battery".ToLocalizationString());
			}
			if (num46 > 2)
			{
				attr.m_Errors.Add("More than 2 weapons".ToLocalizationString());
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.Robot;
			}
			else
			{
				attr.m_Type = ECreation.Null;
			}
			break;
		}
		case EVCCategory.cgAITurret:
		{
			int num17 = 0;
			foreach (VCComponentData component13 in iso.m_Components)
			{
				if (component13.m_Type == EVCComponent.cpAITurretWeapon)
				{
					num17++;
				}
			}
			if (num17 != 1)
			{
				attr.m_Errors.Add("Zero or more than 1 weapon".ToLocalizationString());
			}
			if (attr.m_Errors.Count == 0)
			{
				attr.m_Type = ECreation.AITurret;
			}
			else
			{
				attr.m_Type = ECreation.Null;
			}
			break;
		}
		case EVCCategory.cgHeadArmor:
		case EVCCategory.cgBodyArmor:
		case EVCCategory.cgArmAndLegArmor:
		case EVCCategory.cgHandAndFootArmor:
		case EVCCategory.cgDecorationArmor:
		{
			int num = 0;
			foreach (VCComponentData component14 in iso.m_Components)
			{
				if (component14.m_Type == EVCComponent.cpHeadPivot || component14.m_Type == EVCComponent.cpBodyPivot || component14.m_Type == EVCComponent.cpArmAndLegPivot || component14.m_Type == EVCComponent.cpHandAndFootPivot || component14.m_Type == EVCComponent.cpDecorationPivot)
				{
					num++;
				}
			}
			if (num != 1)
			{
				attr.m_Errors.Add("Zero or more than one pivot".ToLocalizationString());
				attr.m_Type = ECreation.Null;
			}
			else
			{
				attr.m_Type = (ECreation)iso.m_HeadInfo.Category;
			}
			break;
		}
		default:
			attr.m_Errors.Add("Unknown category".ToLocalizationString());
			break;
		}
	}

	private static void CalcCommonAttr(VCIsoData iso, CreationAttr attr)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		double num10 = 0.0;
		VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
		foreach (KeyValuePair<int, VCMatterInfo> s_Matter in VCConfig.s_Matters)
		{
			attr.m_Cost.Add(s_Matter.Value.ItemId, 0);
		}
		Vector3 vector = Vector3.one * 0.5f;
		double num11 = vCESceneSetting.m_VoxelSize;
		double num12 = num11 * num11 * num11;
		foreach (KeyValuePair<int, VCVoxel> voxel in iso.m_Voxels)
		{
			if (voxel.Value.Volume < 128)
			{
				continue;
			}
			IntVector3 intVector = VCIsoData.KeyToIPos(voxel.Key);
			Vector3 vector2 = (intVector.ToVector3() + vector) * (float)num11;
			double num13 = 1.0;
			double num14 = num13;
			VCMaterial vCMaterial = iso.m_Materials[voxel.Value.Type];
			if (vCMaterial != null)
			{
				VCMatterInfo vCMatterInfo = VCConfig.s_Matters[vCMaterial.m_MatterId];
				num14 = (double)(vCMatterInfo.Density * 1000f) * num13;
				attr.m_Durability += vCMatterInfo.Durability;
				if (num13 > 0.5)
				{
					Dictionary<int, int> cost;
					Dictionary<int, int> dictionary = (cost = attr.m_Cost);
					int itemId;
					int key = (itemId = vCMaterial.ItemId);
					itemId = cost[itemId];
					dictionary[key] = itemId + 1;
				}
			}
			num8 += (double)vector2.x * num14;
			num9 += (double)vector2.y * num14;
			num10 += (double)vector2.z * num14;
			num2 += num14;
			num4 += num13;
		}
		if (iso.m_Colors.Count > 0)
		{
			float num15 = 0f;
			foreach (KeyValuePair<int, Color32> color in iso.m_Colors)
			{
				float num16 = Mathf.Abs(VCIsoData.BLANK_COLOR.r - color.Value.r);
				float num17 = Mathf.Abs(VCIsoData.BLANK_COLOR.g - color.Value.g);
				float num18 = Mathf.Abs(VCIsoData.BLANK_COLOR.b - color.Value.b);
				float num19 = Mathf.Abs(VCIsoData.BLANK_COLOR.a - color.Value.a);
				float num20 = Mathf.Clamp01((num16 + num17 + num18 + num19) * 0.01f);
				IntVector3 intVector2 = VCIsoData.KeyToIPos(color.Key);
				Vector3 iso_pos = intVector2.ToVector3() * 0.5f - 0.5f * Vector3.one;
				if (iso.CanSee(iso_pos))
				{
					num15 += num20;
				}
			}
			attr.m_Cost.Add(VCConfig.s_DyeID, Mathf.CeilToInt(num15 / vCESceneSetting.m_DyeUnit - 0.1f));
		}
		foreach (KeyValuePair<int, VCMatterInfo> s_Matter2 in VCConfig.s_Matters)
		{
			attr.m_Cost[s_Matter2.Value.ItemId] = Mathf.CeilToInt((float)attr.m_Cost[s_Matter2.Value.ItemId] / vCESceneSetting.m_BlockUnit);
			attr.m_SellPrice += s_Matter2.Value.SellPrice * (float)attr.m_Cost[s_Matter2.Value.ItemId];
		}
		num4 *= num12;
		num2 *= num12;
		num8 *= num12;
		num9 *= num12;
		num10 *= num12;
		foreach (VCComponentData component2 in iso.m_Components)
		{
			if (!VCConfig.s_Parts.ContainsKey(component2.m_ComponentId))
			{
				continue;
			}
			VCPartInfo vCPartInfo = VCConfig.s_Parts[component2.m_ComponentId];
			if (vCPartInfo.m_ResObj == null)
			{
				throw new Exception("Can't load resource of part".ToLocalizationString() + ": [" + vCPartInfo.m_Name + "]");
			}
			VCEComponentTool component = vCPartInfo.m_ResObj.GetComponent<VCEComponentTool>();
			if (component == null || component.m_SelBound == null)
			{
				throw new Exception("Incomplete resource of part".ToLocalizationString() + ": [" + vCPartInfo.m_Name + "]");
			}
			Vector3 position = component2.m_Position;
			if (component.m_MassCenter != null)
			{
				vCPartInfo.m_ResObj.transform.position = component2.m_Position;
				vCPartInfo.m_ResObj.transform.eulerAngles = component2.m_Rotation;
				vCPartInfo.m_ResObj.transform.localScale = component2.m_Scale;
				position = component.m_MassCenter.position;
				vCPartInfo.m_ResObj.transform.position = Vector3.zero;
				vCPartInfo.m_ResObj.transform.rotation = Quaternion.identity;
				vCPartInfo.m_ResObj.transform.localScale = Vector3.one;
			}
			Transform transform = component.m_SelBound.transform;
			double num21 = transform.localScale.x * transform.localScale.y * transform.localScale.z * component2.m_Scale.x * component2.m_Scale.y * component2.m_Scale.z * vCPartInfo.m_Volume;
			double num22 = VCConfig.s_Parts[component2.m_ComponentId].m_Weight * component2.m_Scale.x * component2.m_Scale.y * component2.m_Scale.z;
			num5 += (double)position.x * num22;
			num6 += (double)position.y * num22;
			num7 += (double)position.z * num22;
			num += num22;
			num3 += num21;
			if (vCPartInfo.m_CostCount > 0)
			{
				int itemID = vCPartInfo.m_ItemID;
				if (!attr.m_Cost.ContainsKey(itemID))
				{
					attr.m_Cost.Add(itemID, 0);
				}
				Dictionary<int, int> cost2;
				Dictionary<int, int> dictionary2 = (cost2 = attr.m_Cost);
				int itemId;
				int key2 = (itemId = itemID);
				itemId = cost2[itemId];
				dictionary2[key2] = itemId + vCPartInfo.m_CostCount;
				attr.m_SellPrice += vCPartInfo.m_SellPrice * (float)vCPartInfo.m_CostCount;
			}
		}
		attr.m_Weight = (float)(num + num2);
		attr.m_Volume = (float)(num3 + num4);
		attr.m_CenterOfMass = Vector3.zero;
		if (attr.m_Weight > 0f)
		{
			Vector3 vector3 = new Vector3((float)(num5 + num8), (float)(num6 + num9), (float)(num7 + num10));
			attr.m_CenterOfMass = vector3 / attr.m_Weight;
		}
	}

	private static void CalcSwordAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		int num = 1024;
		int num2 = 1048576;
		int[] array = new int[10];
		int[] array2 = new int[iso.m_HeadInfo.ySize];
		int[] array3 = new int[iso.m_HeadInfo.ySize];
		VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
		foreach (KeyValuePair<int, VCVoxel> voxel in iso.m_Voxels)
		{
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = voxel.Key & 0x3FF;
			int num7 = voxel.Key >> 20;
			int num8 = (voxel.Key >> 10) & 0x3FF;
			if (num6 == 0 || iso.GetVoxel(voxel.Key - 1).Volume < 128)
			{
				num3 |= 1;
				num5++;
			}
			if (num6 == iso.m_HeadInfo.xSize - 1 || iso.GetVoxel(voxel.Key + 1).Volume < 128)
			{
				num4 |= 1;
				num5++;
			}
			if (num7 == 0 || iso.GetVoxel(voxel.Key - num2).Volume < 128)
			{
				num3 |= 2;
				num5++;
			}
			if (num7 == iso.m_HeadInfo.ySize - 1 || iso.GetVoxel(voxel.Key + num2).Volume < 128)
			{
				num4 |= 2;
				num5++;
			}
			if (num8 == 0 || iso.GetVoxel(voxel.Key - num).Volume < 128)
			{
				num3 |= 4;
				num5++;
			}
			if (num8 == iso.m_HeadInfo.zSize - 1 || iso.GetVoxel(voxel.Key + num).Volume < 128)
			{
				num4 |= 4;
				num5++;
			}
			switch (num5)
			{
			case 0:
				array[0]++;
				break;
			case 1:
				array[1]++;
				break;
			case 2:
				array[((num3 & num4) != 0) ? 3 : 2]++;
				break;
			case 3:
				array[((num3 & num4) != 0) ? 5 : 4]++;
				break;
			case 4:
				array[((num3 | num4) != 7) ? 7 : 6]++;
				break;
			case 5:
				array[8]++;
				break;
			case 6:
				array[9]++;
				break;
			}
			IntVector3 intVector = VCIsoData.KeyToIPos(voxel.Key);
			if (attr.m_Type == ECreation.SwordDouble)
			{
				if ((float)intVector.x < 0.5f * (float)iso.m_HeadInfo.xSize)
				{
					array2[intVector.y] = 1;
				}
				else
				{
					array3[intVector.y] = 1;
				}
			}
			else
			{
				array2[intVector.y] = 1;
			}
		}
		float[] array4 = new float[10] { 1f, 2f, 5f, 3f, 12f, 6f, 35f, 10f, 70f, 3f };
		float[] array5 = new float[10] { 10000f, 10000f, 1500f, 7000f, 1200f, 1200f, 350f, 350f, 300f, 200f };
		for (int i = 0; i < 10; i++)
		{
			float p = (float)array[i] / array5[i];
			float t = Mathf.Pow(0.4f, p);
			array4[i] = Mathf.Lerp(0.2f, array4[i], t);
		}
		float num9 = 0f;
		float num10 = 0f;
		foreach (KeyValuePair<int, VCVoxel> voxel2 in iso.m_Voxels)
		{
			int volume = voxel2.Value.Volume;
			float num11 = 0f;
			float num12 = 0f;
			float num13 = 0f;
			int num14 = 0;
			int num15 = 0;
			int num16 = 0;
			int num17 = voxel2.Key % num;
			int num18 = voxel2.Key / num2;
			int num19 = voxel2.Key % num2 / num;
			int num20 = 0;
			num20 = iso.GetVoxel(voxel2.Key - 1).Volume;
			if (num17 == 0 || num20 < 128)
			{
				num14 |= 1;
				num16++;
			}
			else
			{
				num11 += (float)num20;
				num12 += 1f;
			}
			num20 = iso.GetVoxel(voxel2.Key + 1).Volume;
			if (num17 == iso.m_HeadInfo.xSize - 1 || num20 < 128)
			{
				num15 |= 1;
				num16++;
			}
			else
			{
				num11 += (float)num20;
				num12 += 1f;
			}
			num20 = iso.GetVoxel(voxel2.Key - num2).Volume;
			if (num18 == 0 || num20 < 128)
			{
				num14 |= 2;
				num16++;
			}
			else
			{
				num11 += (float)num20;
				num12 += 1f;
			}
			num20 = iso.GetVoxel(voxel2.Key + num2).Volume;
			if (num18 == iso.m_HeadInfo.ySize - 1 || num20 < 128)
			{
				num15 |= 2;
				num16++;
			}
			else
			{
				num11 += (float)num20;
				num12 += 1f;
			}
			num20 = iso.GetVoxel(voxel2.Key - num).Volume;
			if (num19 == 0 || num20 < 128)
			{
				num14 |= 4;
				num16++;
			}
			else
			{
				num11 += (float)num20;
				num12 += 1f;
			}
			num20 = iso.GetVoxel(voxel2.Key + num).Volume;
			if (num19 == iso.m_HeadInfo.zSize - 1 || num20 < 128)
			{
				num15 |= 4;
				num16++;
			}
			else
			{
				num11 += (float)num20;
				num12 += 1f;
			}
			switch (num16)
			{
			case 0:
				num13 = array4[0];
				break;
			case 1:
				num13 = array4[1];
				break;
			case 2:
				num13 = (((num14 & num15) != 0) ? array4[3] : array4[2]);
				break;
			case 3:
				num13 = (((num14 & num15) != 0) ? array4[5] : array4[4]);
				break;
			case 4:
				num13 = (((num14 | num15) != 7) ? array4[7] : array4[6]);
				break;
			case 5:
				num13 = array4[8];
				break;
			case 6:
				num13 = array4[9];
				break;
			}
			float f = 1f;
			if ((num16 > 3 || (num16 == 3 && (num14 & num15) != 0)) && volume >= 128)
			{
				if (num12 > 0f)
				{
					num11 /= num12;
					f = 128f / (float)volume * (255f / num11);
				}
				else
				{
					f = 255f / (float)volume;
				}
			}
			VCMatterInfo vCMatterInfo = VCConfig.s_Matters[iso.m_Materials[voxel2.Value.Type].m_MatterId];
			num9 += vCMatterInfo.Attack * num13 * Mathf.Max(1f, Mathf.Pow(f, 4f));
			num10 += vCMatterInfo.Attack;
		}
		float num21 = Mathf.Pow(Mathf.Clamp(num10 / num9, 0f, 1f), 0.7f);
		float durability = attr.m_Durability;
		durability *= num21;
		durability *= 0.08f;
		num9 *= 0.027f;
		float num22 = 0f;
		float num23 = 0f;
		int num24 = 0;
		bool flag = true;
		foreach (VCComponentData component in iso.m_Components)
		{
			if (attr.m_Type == ECreation.SwordDouble)
			{
				if (component.m_Type == EVCComponent.cpDbSwordHilt && (component as VCFixedHandPartData).m_LeftHand)
				{
					num24++;
					flag = component.m_Position.x < (float)iso.m_HeadInfo.xSize * vCESceneSetting.m_VoxelSize * 0.5f;
				}
				if (component.m_Type == EVCComponent.cpDbSwordHilt && !(component as VCFixedHandPartData).m_LeftHand)
				{
					num24++;
				}
				if (component.m_Position.x < (float)iso.m_HeadInfo.xSize * vCESceneSetting.m_VoxelSize * 0.5f)
				{
					num22 = component.m_Position.y;
				}
				else
				{
					num23 = component.m_Position.y;
				}
				if (num24 == 2)
				{
					num9 *= VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().AttackEnh;
					num9 += VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().AttackInc;
					durability *= VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().DurabilityEnh;
					durability += VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().DurabilityInc;
					break;
				}
			}
			else if (component.m_Type == EVCComponent.cpSwordHilt || component.m_Type == EVCComponent.cpLgSwordHilt || component.m_Type == EVCComponent.cpDbSwordHilt)
			{
				num9 *= VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().AttackEnh;
				num9 += VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().AttackInc;
				durability *= VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().DurabilityEnh;
				durability += VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPSwordHilt>().DurabilityInc;
				num22 = component.m_Position.y;
				num23 = component.m_Position.y;
				break;
			}
		}
		float num25 = ((attr.m_Type != ECreation.SwordDouble && attr.m_Type != ECreation.SwordLarge) ? 0.1f : 0.12f);
		int num26 = Mathf.CeilToInt((num22 - num25) / vCESceneSetting.m_VoxelSize);
		int num27 = Mathf.CeilToInt((num22 + num25) / vCESceneSetting.m_VoxelSize);
		int num28 = Mathf.CeilToInt((num23 - num25) / vCESceneSetting.m_VoxelSize);
		int num29 = Mathf.CeilToInt((num23 + num25) / vCESceneSetting.m_VoxelSize);
		int num30 = 0;
		int num31 = 0;
		for (int j = 0; j < array2.Length; j++)
		{
			num30 += array2[j];
			if (array2[j] != 0 && j >= num26 && j <= num27)
			{
				num30--;
			}
			if (array2[j] != 0 && j < num26)
			{
				num31++;
			}
		}
		num30 += num27 - num26;
		int num32 = 0;
		int num33 = 0;
		for (int k = 0; k < array3.Length; k++)
		{
			num32 += array3[k];
			if (array3[k] != 0 && k >= num28 && k <= num29)
			{
				num32--;
			}
			if (array2[k] != 0 && k < num28)
			{
				num33++;
			}
		}
		num32 += num29 - num28;
		float num34 = (float)(num30 + 1) * vCESceneSetting.m_VoxelSize;
		float num35 = (float)(num32 + 1) * vCESceneSetting.m_VoxelSize;
		float num36 = (float)num31 * vCESceneSetting.m_VoxelSize;
		float num37 = (float)num33 * vCESceneSetting.m_VoxelSize;
		if (!flag)
		{
			float num38 = num34;
			num34 = num35;
			num35 = num38;
			num38 = num36;
			num36 = num37;
			num37 = num38;
		}
		attr.m_Attack = VCEMath.SmoothConstraint(num9, 210f, 1.25f);
		attr.m_Durability = VCEMath.SmoothConstraint(durability, 2000f, 0.5f);
		attr.m_AtkHeight = new Vector4(num34, num35, num36, num37);
	}

	private static void CalcShieldAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCPShieldHandle vCPShieldHandle = null;
		Vector3 vector = Vector3.zero;
		foreach (VCComponentData component in iso.m_Components)
		{
			if (component.m_Type == EVCComponent.cpShieldHandle)
			{
				vCPShieldHandle = VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPShieldHandle>();
				vector = component.m_Position;
				break;
			}
		}
		float voxelSize = iso.m_HeadInfo.FindSceneSetting().m_VoxelSize;
		float[,] array = new float[iso.m_HeadInfo.xSize / 5 + 1, iso.m_HeadInfo.ySize / 5 + 1];
		float[,] array2 = new float[iso.m_HeadInfo.xSize / 5 + 1, iso.m_HeadInfo.ySize / 5 + 1];
		int[,] array3 = new int[iso.m_HeadInfo.xSize / 5 + 1, iso.m_HeadInfo.ySize / 5 + 1];
		int num = (int)(vector.x / voxelSize) / 5;
		int num2 = (int)(vector.y / voxelSize) / 5;
		foreach (KeyValuePair<int, VCVoxel> voxel in iso.m_Voxels)
		{
			int num3 = voxel.Key & 0x3FF;
			int num4 = voxel.Key >> 20;
			if (voxel.Value.Volume > 127)
			{
				array3[num3 / 5, num4 / 5]++;
				VCMatterInfo vCMatterInfo = VCConfig.s_Matters[iso.m_Materials[voxel.Value.Type].m_MatterId];
				array[num3 / 5, num4 / 5] += vCMatterInfo.Defence;
				array2[num3 / 5, num4 / 5] += vCMatterInfo.Durability;
			}
		}
		float num5 = 0f;
		float num6 = 0f;
		for (int i = 0; i < iso.m_HeadInfo.xSize / 5 + 1; i++)
		{
			for (int j = 0; j < iso.m_HeadInfo.ySize / 5 + 1; j++)
			{
				if (array3[i, j] >= 17)
				{
					float num7 = Vector2.Distance(new Vector2(i, j), new Vector2(num, num2));
					float num8 = Mathf.Clamp01((9f - num7) / 8f) + 0.05f;
					if (num8 > 0f)
					{
						float num9 = array[i, j] / (float)array3[i, j];
						float num10 = array2[i, j] / (float)array3[i, j];
						float num11 = Mathf.Pow(array3[i, j], 0.25f);
						float num12 = num9 * num11 * num8;
						float num13 = num10 * num11 * num8;
						num5 += num12;
						num6 += num13;
					}
				}
			}
		}
		num5 *= 2.4f;
		num6 *= 0.3f;
		num5 *= vCPShieldHandle.DefenceEnh;
		num5 += vCPShieldHandle.BaseDefence;
		num6 *= vCPShieldHandle.DurabilityEnh;
		num6 += vCPShieldHandle.BaseDurability;
		attr.m_Attack = 0f;
		attr.m_Defense = VCEMath.SmoothConstraint(num5, 232f, 0.5f);
		attr.m_Durability = VCEMath.SmoothConstraint(num6, 1500f, 0.5f);
	}

	private static void CalcBowAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		attr.m_Durability = PEVCConfig.instance.bowDurabilityScale * Mathf.Sqrt(attr.m_Durability) + PEVCConfig.instance.bowDurabilityBase;
		attr.m_Attack = 0f;
		float num = 0f;
		foreach (KeyValuePair<int, VCVoxel> voxel in iso.m_Voxels)
		{
			if (voxel.Value.Volume >= 128)
			{
				VCMaterial vCMaterial = iso.m_Materials[voxel.Value.Type];
				if (vCMaterial != null)
				{
					VCMatterInfo vCMatterInfo = VCConfig.s_Matters[vCMaterial.m_MatterId];
					num += vCMatterInfo.Attack * 3.3f * voxel.Value.VolumeF;
				}
			}
		}
		num = Mathf.Clamp(num / 100000f, 0f, 2f);
		num *= 2f - num;
		foreach (VCComponentData component2 in iso.m_Components)
		{
			if (component2.m_Type == EVCComponent.cpBowGrip)
			{
				VCPBowGrip component = VCConfig.s_Parts[component2.m_ComponentId].m_ResObj.GetComponent<VCPBowGrip>();
				if ((bool)component)
				{
					attr.m_Attack = component.baseAttack + component.maxExtendAttack * num;
					break;
				}
			}
		}
	}

	private static void CalcGunAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		float f = attr.m_Weight + 1f;
		float num = Mathf.Log10(f);
		float num2 = Mathf.Pow(num - 1.1f, 2f);
		float val = (num2 + 0.05f) * 10f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		foreach (KeyValuePair<int, VCVoxel> voxel in iso.m_Voxels)
		{
			if (voxel.Value.Volume >= 128)
			{
				VCMaterial vCMaterial = iso.m_Materials[voxel.Value.Type];
				if (vCMaterial != null)
				{
					VCMatterInfo vCMatterInfo = VCConfig.s_Matters[vCMaterial.m_MatterId];
					num5 += vCMatterInfo.Attack * voxel.Value.VolumeF;
				}
			}
		}
		foreach (VCComponentData component2 in iso.m_Components)
		{
			if (component2.m_Type == EVCComponent.cpGunMuzzle)
			{
				VCPGunMuzzle component = VCConfig.s_Parts[component2.m_ComponentId].m_ResObj.GetComponent<VCPGunMuzzle>();
				num3 += component.Attack;
				num4 = ((!component.Multishot) ? (num4 + 0.75f) : (num4 + 1f / (component.FireInterval + 0.001f)));
			}
		}
		attr.m_MuzzleAtkInc = VCEMath.SmoothConstraint(num5 * 0.002f, 0.724f, 1f);
		attr.m_Attack = num3 * attr.m_MuzzleAtkInc;
		attr.m_FireSpeed = num4;
		attr.m_Durability = VCEMath.SmoothConstraint(attr.m_Durability * 0.01f + 200f, 1200f, 0.5f);
		attr.m_Accuracy = VCEMath.SmoothConstraint(val, 6f, 1f);
	}

	private static void CalcVehicleAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * vCESceneSetting.m_VoxelSize * 500f * PEVCConfig.instance.vehicleDurabilityScale;
		attr.m_MaxFuel = 0f;
		foreach (VCComponentData component in iso.m_Components)
		{
			if (component.m_Type == EVCComponent.cpVehicleFuelCell)
			{
				attr.m_MaxFuel += VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPFuelCell>().energyCapacity;
			}
		}
		CalcVCPWeaponAttack(iso, attr);
	}

	private static void CalcAircraftAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * Mathf.Pow(vCESceneSetting.m_VoxelSize, 3f) * 50000f * PEVCConfig.instance.helicopterDurabilityScale;
		attr.m_MaxFuel = 0f;
		foreach (VCComponentData component in iso.m_Components)
		{
			if (component.m_Type == EVCComponent.cpVtolFuelCell || component.m_Type == EVCComponent.cpVehicleFuelCell)
			{
				attr.m_MaxFuel += VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPFuelCell>().energyCapacity;
			}
		}
		CalcVCPWeaponAttack(iso, attr);
	}

	private static void CalcBoatAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * Mathf.Pow(vCESceneSetting.m_VoxelSize, 3f) * 50000f * PEVCConfig.instance.boatDurabilityScale;
		attr.m_MaxFuel = 0f;
		foreach (VCComponentData component in iso.m_Components)
		{
			if (component.m_Type == EVCComponent.cpVehicleFuelCell || component.m_Type == EVCComponent.cpVtolFuelCell)
			{
				attr.m_MaxFuel += VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPFuelCell>().energyCapacity;
			}
		}
		attr.m_FluidDisplacement = new List<VolumePoint>();
		int num = 20;
		float num2 = (float)num * 0.5f;
		float num3 = vCESceneSetting.m_VoxelSize * (float)num;
		float a = num3 * num3 * num3 / 2f;
		double num4 = vCESceneSetting.m_VoxelSize * vCESceneSetting.m_VoxelSize * vCESceneSetting.m_VoxelSize;
		Vector3 centerOfMass = attr.m_CenterOfMass;
		centerOfMass.y = 0f;
		for (int i = 0; i < vCESceneSetting.m_EditorSize.y; i += num / 2)
		{
			for (int j = 0; j < vCESceneSetting.m_EditorSize.z; j += num)
			{
				for (int k = 0; k < vCESceneSetting.m_EditorSize.x; k += num)
				{
					Vector3 lpos = new Vector3((float)k + num2, (float)i + num2, (float)j + num2) * vCESceneSetting.m_VoxelSize - centerOfMass;
					double num5 = 0.0;
					for (int l = 0; l < num; l++)
					{
						for (int m = 0; m < num; m++)
						{
							for (int n = 0; n < num; n++)
							{
								if (iso.GetVoxel(VCIsoData.IPosToKey(k + n, i + l, j + m)).Volume >= 128)
								{
									num5 += num4;
								}
							}
						}
					}
					float num6 = Mathf.Min(a, (float)num5 * 20f);
					if (num6 > 0f)
					{
						attr.m_FluidDisplacement.Add(new VolumePoint(lpos, num6, num6));
					}
				}
			}
		}
		CalcVCPWeaponAttack(iso, attr);
	}

	private static void CalcSimpleObjectAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * vCESceneSetting.m_VoxelSize * 500f;
	}

	private static void CalcRobotAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * PEVCConfig.instance.robotDurabilityScale;
		attr.m_MaxFuel = 0f;
		foreach (VCComponentData component in iso.m_Components)
		{
			if (component.m_Type == EVCComponent.cpRobotBattery)
			{
				attr.m_MaxFuel += VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPFuelCell>().energyCapacity;
			}
		}
		CalcVCPWeaponAttack(iso, attr);
	}

	private static void CalcAITurretAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * PEVCConfig.instance.aiTurretDurabilityScale;
		attr.m_MaxFuel = 0f;
		attr.m_Defense = 1f;
		foreach (VCComponentData component in iso.m_Components)
		{
			if (component.m_Type == EVCComponent.cpAITurretWeapon)
			{
				attr.m_MaxFuel += VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPWeapon>().bulletCapacity;
				attr.m_Defense = ((VCConfig.s_Parts[component.m_ComponentId].m_ResObj.GetComponent<VCPWeapon>().bulletProtoID != 0) ? 0f : 1f);
			}
		}
		CalcVCPWeaponAttack(iso, attr);
	}

	private static void CalcArmorAttr(VCIsoData iso, float random_seed, CreationAttr attr)
	{
		VCESceneSetting vCESceneSetting = iso.m_HeadInfo.FindSceneSetting();
		attr.m_Durability = Mathf.Sqrt(attr.m_Durability) * PEVCConfig.instance.armorDurabilityScale;
		attr.m_Defense = VCUtility.GetArmorDefence(attr.m_Durability);
	}

	private static void CalcVCPWeaponAttack(VCIsoData iso, CreationAttr attr)
	{
		attr.m_Attack = 0f;
		foreach (VCComponentData component2 in iso.m_Components)
		{
			switch (component2.m_Type)
			{
			case EVCComponent.cpCtrlTurret:
			case EVCComponent.cpFrontCannon:
			case EVCComponent.cpMissileLauncher:
			case EVCComponent.cpAITurretWeapon:
			case EVCComponent.cpRobotWeapon:
			{
				VCPWeapon component = VCConfig.s_Parts[component2.m_ComponentId].m_ResObj.GetComponent<VCPWeapon>();
				if ((bool)component)
				{
					attr.m_Attack += component.attackPerSecond;
				}
				break;
			}
			}
		}
	}

	public string TypeString()
	{
		switch (m_Attribute.m_Type)
		{
		case ECreation.Null:
			return "Null".ToLocalizationString();
		case ECreation.Sword:
		case ECreation.SwordLarge:
		case ECreation.SwordDouble:
			return "Sword".ToLocalizationString();
		case ECreation.Axe:
			return "Axe".ToLocalizationString();
		case ECreation.Bow:
			return "Bow".ToLocalizationString();
		case ECreation.HandGun:
			return "Hand gun".ToLocalizationString();
		case ECreation.Rifle:
			return "Rifle".ToLocalizationString();
		case ECreation.Shield:
			return "Shield".ToLocalizationString();
		case ECreation.Vehicle:
			return "Vehicle".ToLocalizationString();
		case ECreation.Aircraft:
			return "Aircraft".ToLocalizationString();
		case ECreation.Boat:
			return "Boat".ToLocalizationString();
		case ECreation.SimpleObject:
			return "Object".ToLocalizationString();
		case ECreation.ArmorHead:
			return "Head Armor".ToLocalizationString();
		case ECreation.ArmorBody:
			return "Body Armor".ToLocalizationString();
		case ECreation.ArmorArmAndLeg:
			return "Arm And Leg Armor".ToLocalizationString();
		case ECreation.ArmorHandAndFoot:
			return "Hand And Foot Armor".ToLocalizationString();
		case ECreation.ArmorDecoration:
			return "Decoration Armor".ToLocalizationString();
		case ECreation.Robot:
			return "Robot".ToLocalizationString();
		case ECreation.AITurret:
			return "AI Turret".ToLocalizationString();
		default:
			return "??????";
		}
	}

	private string DivideValString(string name, float curr, float max, float red = 0.333f)
	{
		if (curr / max < red)
		{
			return name + ": [FF3030]" + curr.ToString("0") + "[-] / [1FD0FF]" + max.ToString("0") + "[-]\r\n";
		}
		return name + ": [1FD0FF]" + curr.ToString("0") + "[-] / [1FD0FF]" + max.ToString("0") + "[-]\r\n";
	}

	private string WeaponDescString()
	{
		VCPWeapon[] componentsInChildren = m_Prefab.GetComponentsInChildren<VCPWeapon>(includeInactive: true);
		string empty = string.Empty;
		empty = empty + "Weapons".ToLocalizationString() + ":\r\n";
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		VCPWeapon[] array = componentsInChildren;
		foreach (VCPWeapon vCPWeapon in array)
		{
			string name = vCPWeapon.gameObject.name;
			if (dictionary.ContainsKey(name))
			{
				Dictionary<string, int> dictionary2;
				Dictionary<string, int> dictionary3 = (dictionary2 = dictionary);
				string key;
				string key2 = (key = name);
				int num = dictionary2[key];
				dictionary3[key2] = num + 1;
			}
			else
			{
				dictionary.Add(name, 1);
			}
		}
		if (dictionary.Count > 0)
		{
			foreach (KeyValuePair<string, int> item in dictionary)
			{
				string key = empty;
				empty = key + "  - [ [C0C0C0]" + item.Key + "[-] ]   x  " + item.Value + "\r\n";
			}
		}
		else
		{
			empty = empty + "    [A0A0A0]" + "No weapon".ToLocalizationString() + "[-]\r\n";
		}
		return empty;
	}

	public string AttrDescString(ItemObject obj)
	{
		if (m_Prefab == null || obj == null || obj.instanceId != m_ObjectID)
		{
			return "< No information >".ToLocalizationString();
		}
		string empty = string.Empty;
		float num = 0f;
		float num2 = 0f;
		Strengthen cmpt = obj.GetCmpt<Strengthen>();
		Property cmpt2 = obj.GetCmpt<Property>();
		LifeLimit cmpt3 = obj.GetCmpt<LifeLimit>();
		if (cmpt3 != null)
		{
			num = cmpt3.floatValue.current;
		}
		Durability cmpt4 = obj.GetCmpt<Durability>();
		if (cmpt4 != null)
		{
			num = cmpt4.floatValue.current;
		}
		Energy cmpt5 = obj.GetCmpt<Energy>();
		if (cmpt5 != null)
		{
			num2 = cmpt5.floatValue.current;
		}
		string text = m_IsoData.m_HeadInfo.Name;
		if (string.IsNullOrEmpty(text))
		{
			text = PELocalization.GetString(8000635);
		}
		string text2 = m_IsoData.m_HeadInfo.Desc;
		if (string.IsNullOrEmpty(text2))
		{
			text2 = PELocalization.GetString(8000636);
		}
		switch (m_Attribute.m_Type)
		{
		case ECreation.Sword:
		case ECreation.SwordLarge:
		case ECreation.SwordDouble:
			return string.Format(PELocalization.GetString(8000626), text, cmpt2.GetProperty(AttribType.Atk), PELocalization.GetString(VCUtility.GetSwordAtkSpeedTextID(VCUtility.GetSwordAnimSpeed(m_Attribute.m_Weight))), Mathf.CeilToInt(num * PEVCConfig.equipDurabilityShowScale), Mathf.CeilToInt(cmpt4.valueMax * PEVCConfig.equipDurabilityShowScale), obj.GetSellPrice(), text2);
		case ECreation.Axe:
			return string.Format(PELocalization.GetString(8000627), text, cmpt2.GetProperty(AttribType.Atk), PELocalization.GetString(VCUtility.GetAxeAtkSpeedTextID(VCUtility.GetAxeAnimSpeed(m_Attribute.m_Weight))), Mathf.CeilToInt(num * PEVCConfig.equipDurabilityShowScale), Mathf.CeilToInt(cmpt4.valueMax * PEVCConfig.equipDurabilityShowScale), obj.GetSellPrice(), text2);
		case ECreation.Bow:
			return string.Format(PELocalization.GetString(8000628), text, cmpt2.GetProperty(AttribType.Atk), Mathf.CeilToInt(num * PEVCConfig.equipDurabilityShowScale), Mathf.CeilToInt(cmpt4.valueMax * PEVCConfig.equipDurabilityShowScale), obj.GetSellPrice(), text2);
		case ECreation.Shield:
			return string.Format(PELocalization.GetString(8000629), text, cmpt2.GetProperty(AttribType.Def), Mathf.CeilToInt(num * PEVCConfig.equipDurabilityShowScale), Mathf.CeilToInt(cmpt4.valueMax * PEVCConfig.equipDurabilityShowScale), obj.GetSellPrice(), text2);
		case ECreation.HandGun:
		case ECreation.Rifle:
			return string.Format(PELocalization.GetString(8000630), text, cmpt2.GetProperty(AttribType.Atk), 1f / m_Prefab.GetComponent<PEGun>().m_FireRate, Mathf.CeilToInt(num * PEVCConfig.equipDurabilityShowScale), Mathf.CeilToInt(cmpt4.valueMax * PEVCConfig.equipDurabilityShowScale), obj.GetSellPrice(), text2);
		case ECreation.Vehicle:
			return string.Format(PELocalization.GetString(8000631), text, creationController.bounds.size.x, creationController.bounds.size.y, creationController.bounds.size.z, num, cmpt3.valueMax, num2, cmpt5.valueMax, m_Attribute.m_Attack, obj.GetSellPrice(), text2);
		case ECreation.Aircraft:
			return string.Format(PELocalization.GetString(8000632), text, creationController.bounds.size.x, creationController.bounds.size.y, creationController.bounds.size.z, num, cmpt3.valueMax, num2, cmpt5.valueMax, m_Attribute.m_Attack, obj.GetSellPrice(), text2);
		case ECreation.Boat:
			return string.Format(PELocalization.GetString(8000633), text, creationController.bounds.size.x, creationController.bounds.size.y, creationController.bounds.size.z, num, cmpt3.valueMax, num2, cmpt5.valueMax, m_Attribute.m_Attack, obj.GetSellPrice(), text2);
		case ECreation.SimpleObject:
			return string.Format(PELocalization.GetString(8000634), text, creationController.bounds.size.x, creationController.bounds.size.y, creationController.bounds.size.z, obj.GetSellPrice(), text2);
		case ECreation.ArmorHead:
			return string.Format(PELocalization.GetString(8000640), text, VCUtility.GetArmorDefence(cmpt4.valueMax), num, cmpt4.valueMax, obj.GetSellPrice(), text2);
		case ECreation.ArmorBody:
			return string.Format(PELocalization.GetString(8000641), text, VCUtility.GetArmorDefence(cmpt4.valueMax), num, cmpt4.valueMax, obj.GetSellPrice(), text2);
		case ECreation.ArmorArmAndLeg:
			return string.Format(PELocalization.GetString(8000642), text, VCUtility.GetArmorDefence(cmpt4.valueMax), num, cmpt4.valueMax, obj.GetSellPrice(), text2);
		case ECreation.ArmorHandAndFoot:
			return string.Format(PELocalization.GetString(8000643), text, VCUtility.GetArmorDefence(cmpt4.valueMax), num, cmpt4.valueMax, obj.GetSellPrice(), text2);
		case ECreation.ArmorDecoration:
			return string.Format(PELocalization.GetString(8000644), text, VCUtility.GetArmorDefence(cmpt4.valueMax), num, cmpt4.valueMax, obj.GetSellPrice(), text2);
		case ECreation.Robot:
			return string.Format(PELocalization.GetString(8000645), text, num, cmpt3.valueMax, num2, cmpt5.valueMax, m_Attribute.m_Attack, obj.GetSellPrice(), text2);
		case ECreation.AITurret:
			return string.Format(PELocalization.GetString((!obj.protoData.unchargeable) ? 8000646 : 8000647), text, num, cmpt3.valueMax, num2, cmpt5.valueMax, m_Attribute.m_Attack, obj.GetSellPrice(), text2);
		default:
			return empty;
		}
	}

	public void CalcHash()
	{
		m_HashCode = CRC64.Compute(m_Resource);
	}

	public bool LoadRes()
	{
		string text = VCConfig.s_CreationPath + HashString + VCConfig.s_CreationFileExt;
		string text2 = VCConfig.s_CreationNetCachePath + HashString + VCConfig.s_CreationNetCacheFileExt;
		string text3 = string.Empty;
		if (File.Exists(text))
		{
			text3 = text;
		}
		else if (File.Exists(text2))
		{
			text3 = text2;
		}
		if (text3.Length == 0)
		{
			return false;
		}
		try
		{
			using (FileStream fileStream = new FileStream(text3, FileMode.Open))
			{
				m_Resource = new byte[(int)fileStream.Length];
				fileStream.Read(m_Resource, 0, (int)fileStream.Length);
				fileStream.Close();
			}
			return ReadRes();
		}
		catch (Exception ex)
		{
			Debug.LogError("Load Creation Resource Error: \r\n" + ex);
			return false;
		}
	}

	public bool ReadRes()
	{
		if (m_IsoData != null)
		{
			m_IsoData.Destroy();
		}
		m_IsoData = new VCIsoData();
		return m_IsoData.Import(m_Resource, new VCIsoOption(editor: false));
	}

	private bool SaveResToFile(string filename)
	{
		try
		{
			using (FileStream fileStream = new FileStream(filename, FileMode.Create))
			{
				fileStream.Write(m_Resource, 0, m_Resource.Length);
				fileStream.Close();
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Save Creation Resource Error: \r\n" + ex);
			return false;
		}
	}

	public bool SaveRes()
	{
		CalcHash();
		string filename = VCConfig.s_CreationPath + HashString + VCConfig.s_CreationFileExt;
		if (!Directory.Exists(VCConfig.s_CreationPath))
		{
			Directory.CreateDirectory(VCConfig.s_CreationPath);
		}
		return SaveResToFile(filename);
	}

	public bool SaveNetCache()
	{
		CalcHash();
		string filename = VCConfig.s_CreationNetCachePath + HashString + VCConfig.s_CreationNetCacheFileExt;
		if (!Directory.Exists(VCConfig.s_CreationNetCachePath))
		{
			Directory.CreateDirectory(VCConfig.s_CreationNetCachePath);
		}
		return SaveResToFile(filename);
	}

	public void BuildPrefab()
	{
		BuildStructure();
		BuildMaterial();
		BuildCreation();
	}

	private static void AlignPivotInChild(Transform root, Transform pivot)
	{
		Transform parent = root.parent;
		root.SetParent(null, worldPositionStays: false);
		parent.position = pivot.position;
		parent.rotation = pivot.rotation;
		root.SetParent(parent, worldPositionStays: true);
		parent.localPosition = Vector3.zero;
		parent.localRotation = Quaternion.identity;
	}

	private void BuildStructure()
	{
		m_Prefab = new GameObject("Creation_" + m_ObjectID + " (" + m_IsoData.m_HeadInfo.Name + ")");
		m_Prefab.transform.parent = VCEditor.Instance.m_CreationGroup.transform;
		m_Prefab.transform.localPosition = Vector3.zero;
		m_Prefab.layer = VCConfig.s_ProductLayer;
		m_Prefab.SetActive(value: false);
		m_Root = new GameObject("Root");
		m_Root.transform.parent = m_Prefab.transform;
		m_Root.transform.localPosition = Vector3.zero;
		m_Root.layer = VCConfig.s_ProductLayer;
		m_PartGroup = new GameObject("Parts");
		m_PartGroup.transform.parent = m_Root.transform;
		m_PartGroup.transform.localPosition = Vector3.zero;
		m_PartGroup.layer = VCConfig.s_ProductLayer;
		m_MeshGroup = new GameObject("Meshes");
		m_MeshGroup.transform.parent = m_Root.transform;
		m_MeshGroup.transform.localPosition = Vector3.zero;
		m_MeshGroup.layer = VCConfig.s_ProductLayer;
		m_DecalGroup = new GameObject("Decals");
		m_DecalGroup.transform.parent = m_Root.transform;
		m_DecalGroup.transform.localPosition = Vector3.zero;
		m_DecalGroup.layer = VCConfig.s_ProductLayer;
		m_EffectGroup = new GameObject("Effects");
		m_EffectGroup.transform.parent = m_Root.transform;
		m_EffectGroup.transform.localPosition = Vector3.zero;
		m_EffectGroup.layer = VCConfig.s_ProductLayer;
		m_RootL = new GameObject("Root_L");
		m_RootL.transform.parent = m_Prefab.transform;
		m_RootL.transform.localPosition = Vector3.zero;
		m_RootL.layer = VCConfig.s_ProductLayer;
		m_PartGroupL = new GameObject("Parts_L");
		m_PartGroupL.transform.parent = m_RootL.transform;
		m_PartGroupL.transform.localPosition = Vector3.zero;
		m_PartGroupL.layer = VCConfig.s_ProductLayer;
		m_MeshGroupL = new GameObject("Meshes_L");
		m_MeshGroupL.transform.parent = m_RootL.transform;
		m_MeshGroupL.transform.localPosition = Vector3.zero;
		m_MeshGroupL.layer = VCConfig.s_ProductLayer;
		m_DecalGroupL = new GameObject("Decals_L");
		m_DecalGroupL.transform.parent = m_RootL.transform;
		m_DecalGroupL.transform.localPosition = Vector3.zero;
		m_DecalGroupL.layer = VCConfig.s_ProductLayer;
		m_EffectGroupL = new GameObject("Effects_L");
		m_EffectGroupL.transform.parent = m_RootL.transform;
		m_EffectGroupL.transform.localPosition = Vector3.zero;
		m_EffectGroupL.layer = VCConfig.s_ProductLayer;
		bool flag = m_IsoData.m_HeadInfo.Category == EVCCategory.cgDbSword;
		VCESceneSetting vCESceneSetting = m_IsoData.m_HeadInfo.FindSceneSetting();
		foreach (VCComponentData component in m_IsoData.m_Components)
		{
			if (component.m_Type == EVCComponent.cpDecal)
			{
				if (flag)
				{
					if (component.m_Position.x < 0.5f * (float)m_IsoData.m_HeadInfo.xSize * vCESceneSetting.m_VoxelSize)
					{
						component.CreateEntity(for_editor: false, m_DecalGroupL.transform);
					}
					else
					{
						component.CreateEntity(for_editor: false, m_DecalGroup.transform);
					}
				}
				else
				{
					component.CreateEntity(for_editor: false, m_DecalGroup.transform);
				}
			}
			else if (component.m_Type == EVCComponent.cpEffect)
			{
				if (flag)
				{
					if (component.m_Position.x < 0.5f * (float)m_IsoData.m_HeadInfo.xSize * vCESceneSetting.m_VoxelSize)
					{
						component.CreateEntity(for_editor: false, m_EffectGroupL.transform);
					}
					else
					{
						component.CreateEntity(for_editor: false, m_EffectGroup.transform);
					}
				}
				else
				{
					component.CreateEntity(for_editor: false, m_EffectGroup.transform);
				}
			}
			else if (component.m_Type == EVCComponent.cpDbSwordHilt)
			{
				if ((component as VCFixedHandPartData).m_LeftHand)
				{
					component.CreateEntity(for_editor: false, m_PartGroupL.transform);
					VCParticlePlayer vCParticlePlayer = component.m_Entity.AddComponent<VCParticlePlayer>();
					vCParticlePlayer.FunctionTag = 1;
					vCParticlePlayer.LocalPosition = component.m_Entity.GetComponent<VCEComponentTool>().m_SelBound.transform.localPosition;
					m_MeshMgrL = m_MeshGroupL.AddComponent<VCMeshMgr>();
					m_MeshMgrL.m_VoxelSize = vCESceneSetting.m_VoxelSize;
					m_MeshMgrL.m_ColorMap = m_IsoData.m_Colors;
					m_MeshMgrL.m_ColliderDirty = false;
					m_MeshMgrL.m_MeshMat = null;
					m_MeshMgrL.m_DaggerMesh = true;
					m_MeshMgrL.m_LeftSidePos = component.m_Position.x < 0.5f * (float)m_IsoData.m_HeadInfo.xSize * vCESceneSetting.m_VoxelSize;
					m_MeshMgrL.Init();
				}
				else
				{
					component.CreateEntity(for_editor: false, m_PartGroup.transform);
					VCParticlePlayer vCParticlePlayer2 = component.m_Entity.AddComponent<VCParticlePlayer>();
					vCParticlePlayer2.FunctionTag = 1;
					vCParticlePlayer2.LocalPosition = component.m_Entity.GetComponent<VCEComponentTool>().m_SelBound.transform.localPosition;
					m_MeshMgr = m_MeshGroup.AddComponent<VCMeshMgr>();
					m_MeshMgr.m_VoxelSize = vCESceneSetting.m_VoxelSize;
					m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
					m_MeshMgr.m_ColliderDirty = false;
					m_MeshMgr.m_MeshMat = null;
					m_MeshMgr.m_DaggerMesh = true;
					m_MeshMgr.m_LeftSidePos = component.m_Position.x < 0.5f * (float)m_IsoData.m_HeadInfo.xSize * vCESceneSetting.m_VoxelSize;
					m_MeshMgr.Init();
				}
			}
			else
			{
				component.CreateEntity(for_editor: false, m_PartGroup.transform);
				VCParticlePlayer vCParticlePlayer3 = component.m_Entity.AddComponent<VCParticlePlayer>();
				vCParticlePlayer3.FunctionTag = 1;
				vCParticlePlayer3.LocalPosition = component.m_Entity.GetComponent<VCEComponentTool>().m_SelBound.transform.localPosition;
				m_MeshMgr = m_MeshGroup.AddComponent<VCMeshMgr>();
				m_MeshMgr.m_VoxelSize = vCESceneSetting.m_VoxelSize;
				m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
				m_MeshMgr.m_ColliderDirty = false;
				m_MeshMgr.m_MeshMat = null;
				m_MeshMgr.m_DaggerMesh = false;
				m_MeshMgr.m_LeftSidePos = false;
				m_MeshMgr.Init();
			}
		}
		if (m_MeshMgr == null)
		{
			m_MeshMgr = m_MeshGroup.AddComponent<VCMeshMgr>();
			m_MeshMgr.m_VoxelSize = vCESceneSetting.m_VoxelSize;
			m_MeshMgr.m_ColorMap = m_IsoData.m_Colors;
			m_MeshMgr.m_ColliderDirty = false;
			m_MeshMgr.m_MeshMat = null;
			m_MeshMgr.m_DaggerMesh = false;
			m_MeshMgr.m_LeftSidePos = false;
			m_MeshMgr.Init();
		}
	}

	private void BuildMaterial()
	{
		if (VCMatGenerator.Instance != null && VCMatManager.Instance != null)
		{
			m_MatGUID = VCMatGenerator.Instance.GenMeshMaterial(m_IsoData.m_Materials);
			if (VCMatManager.Instance.m_mapMaterials.ContainsKey(m_MatGUID))
			{
				m_MeshMgr.m_MeshMat = VCMatManager.Instance.m_mapMaterials[m_MatGUID];
			}
		}
	}

	private T FindComponent<T>(EVCComponent evcComponent) where T : MonoBehaviour
	{
		foreach (VCComponentData component in m_IsoData.m_Components)
		{
			if (component.m_Type == evcComponent)
			{
				return component.m_Entity.GetComponent<T>();
			}
		}
		return (T)null;
	}

	private void BuildCreation()
	{
		if (m_Attribute.m_Type == ECreation.Sword)
		{
			Transform transform = null;
			foreach (VCComponentData component3 in m_IsoData.m_Components)
			{
				if (component3.m_Type == EVCComponent.cpSwordHilt || component3.m_Type == EVCComponent.cpLgSwordHilt)
				{
					transform = component3.m_Entity.transform;
					break;
				}
			}
			m_Root.transform.localRotation = Quaternion.Inverse(transform.rotation);
			m_Root.transform.Rotate(Vector3.up, -90f, Space.World);
			m_Root.transform.Rotate(Vector3.right, 90f, Space.World);
			m_Root.transform.localPosition = -transform.position;
		}
		else if (m_Attribute.m_Type == ECreation.SwordLarge)
		{
			Transform transform2 = null;
			foreach (VCComponentData component4 in m_IsoData.m_Components)
			{
				if (component4.m_Type == EVCComponent.cpLgSwordHilt)
				{
					transform2 = component4.m_Entity.transform;
					break;
				}
			}
			m_Root.transform.localRotation = Quaternion.Inverse(transform2.rotation);
			m_Root.transform.Rotate(Vector3.up, -90f, Space.World);
			m_Root.transform.Rotate(Vector3.right, 90f, Space.World);
			m_Root.transform.localPosition = -transform2.position;
		}
		else if (m_Attribute.m_Type == ECreation.SwordDouble)
		{
			Transform transform3 = null;
			int num = 0;
			foreach (VCComponentData component5 in m_IsoData.m_Components)
			{
				if (component5.m_Type == EVCComponent.cpDbSwordHilt && (component5 as VCFixedHandPartData).m_LeftHand)
				{
					transform3 = component5.m_Entity.transform;
					Transform transform4 = component5.m_Entity.transform.FindChild("SingleHandSword");
					transform4.gameObject.name = "SingleHandSwordL";
					m_RootL.transform.localRotation = Quaternion.Inverse(transform3.rotation);
					m_RootL.transform.Rotate(Vector3.up, -90f, Space.World);
					m_RootL.transform.Rotate(Vector3.right, 90f, Space.World);
					m_RootL.transform.Rotate(Vector3.up, 180f, Space.World);
					m_RootL.transform.Rotate(Vector3.forward, 180f, Space.World);
					m_RootL.transform.localPosition = -transform3.position;
					num++;
				}
				if (component5.m_Type == EVCComponent.cpDbSwordHilt && !(component5 as VCFixedHandPartData).m_LeftHand)
				{
					transform3 = component5.m_Entity.transform;
					Transform transform5 = component5.m_Entity.transform.FindChild("SingleHandSword");
					transform5.gameObject.name = "SingleHandSwordR";
					m_Root.transform.localRotation = Quaternion.Inverse(transform3.rotation);
					m_Root.transform.Rotate(Vector3.up, -90f, Space.World);
					m_Root.transform.Rotate(Vector3.right, 90f, Space.World);
					m_Root.transform.Rotate(Vector3.up, 180f, Space.World);
					m_Root.transform.Rotate(Vector3.forward, 180f, Space.World);
					m_Root.transform.localPosition = -transform3.position;
					num++;
				}
				if (num == 2)
				{
					break;
				}
			}
		}
		else if (m_Attribute.m_Type == ECreation.Bow)
		{
			Transform pivot = null;
			foreach (VCComponentData component6 in m_IsoData.m_Components)
			{
				if (component6.m_Type == EVCComponent.cpBowGrip)
				{
					pivot = component6.m_Entity.transform;
					break;
				}
			}
			AlignPivotInChild(m_Root.transform, pivot);
		}
		else if (m_Attribute.m_Type == ECreation.Axe)
		{
			Transform transform6 = null;
			foreach (VCComponentData component7 in m_IsoData.m_Components)
			{
				if (component7.m_Type == EVCComponent.cpAxeHilt)
				{
					transform6 = component7.m_Entity.transform;
					break;
				}
			}
			m_Root.transform.localRotation = Quaternion.Inverse(transform6.rotation);
			m_Root.transform.Rotate(Vector3.up, -90f, Space.World);
			m_Root.transform.Rotate(Vector3.right, 90f, Space.World);
			m_Root.transform.localPosition = -transform6.position;
		}
		else if (m_Attribute.m_Type == ECreation.Shield)
		{
			Transform transform7 = null;
			foreach (VCComponentData component8 in m_IsoData.m_Components)
			{
				if (component8.m_Type == EVCComponent.cpShieldHandle)
				{
					transform7 = component8.m_Entity.GetComponent<VCPShieldHandle>().m_PivotPoint;
					break;
				}
			}
			m_Root.transform.localRotation = Quaternion.Inverse(transform7.rotation);
			m_Root.transform.Rotate(Vector3.right, 90f, Space.World);
			m_Root.transform.localPosition = -transform7.position;
		}
		else if (m_Attribute.m_Type == ECreation.HandGun || m_Attribute.m_Type == ECreation.Rifle)
		{
			Transform transform8 = null;
			foreach (VCComponentData component9 in m_IsoData.m_Components)
			{
				if (component9.m_Type == EVCComponent.cpGunHandle)
				{
					transform8 = component9.m_Entity.GetComponent<VCPGunHandle>().m_FirstHandPoint;
					break;
				}
			}
			m_Root.transform.localRotation = Quaternion.Inverse(transform8.rotation);
			m_Root.transform.Rotate(Vector3.up, -90f, Space.World);
			m_Root.transform.Rotate(Vector3.right, 90f, Space.World);
			m_Root.transform.localPosition = -transform8.position;
		}
		else if (m_Attribute.m_Type == ECreation.Vehicle)
		{
			Vector3 centerOfMass = m_Attribute.m_CenterOfMass;
			float num2 = 0f;
			int num3 = 0;
			foreach (VCComponentData component10 in m_IsoData.m_Components)
			{
				if (component10.m_Type == EVCComponent.cpVehicleWheel)
				{
					num2 += component10.m_Entity.GetComponent<VCEComponentTool>().m_DrawPivot.position.y;
					num3++;
				}
			}
			centerOfMass.y = num2 / (float)num3 - 0.1f;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -centerOfMass;
		}
		else if (m_Attribute.m_Type == ECreation.Aircraft)
		{
			Vector3 centerOfMass2 = m_Attribute.m_CenterOfMass;
			centerOfMass2.y = 0f;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -centerOfMass2;
		}
		else if (m_Attribute.m_Type == ECreation.Boat)
		{
			Vector3 centerOfMass3 = m_Attribute.m_CenterOfMass;
			centerOfMass3.y = 0f;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -centerOfMass3;
		}
		else if (m_Attribute.m_Type == ECreation.SimpleObject)
		{
			VCComponentData vCComponentData = null;
			foreach (VCComponentData component11 in m_IsoData.m_Components)
			{
				if (component11.m_Type == EVCComponent.cpPivot)
				{
					vCComponentData = component11;
					break;
				}
			}
			Vector3 vector = vCComponentData?.m_Position ?? m_Attribute.m_CenterOfMass;
			vector.y = 0f;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -vector;
		}
		else if (m_Attribute.m_Type == ECreation.Robot)
		{
			Vector3 centerOfMass4 = m_Attribute.m_CenterOfMass;
			centerOfMass4.y = 0f;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -centerOfMass4;
		}
		else if (m_Attribute.m_Type == ECreation.AITurret)
		{
			Vector3 centerOfMass5 = m_Attribute.m_CenterOfMass;
			centerOfMass5.y = 0f;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -centerOfMass5;
		}
		else if (m_Attribute.m_Type == ECreation.ArmorHead || m_Attribute.m_Type == ECreation.ArmorBody || m_Attribute.m_Type == ECreation.ArmorArmAndLeg || m_Attribute.m_Type == ECreation.ArmorHandAndFoot || m_Attribute.m_Type == ECreation.ArmorDecoration)
		{
			Vector3 centerOfMass6 = m_Attribute.m_CenterOfMass;
			centerOfMass6.y = 0f;
			m_Root.transform.localRotation = Quaternion.identity;
			m_Root.transform.localPosition = -centerOfMass6;
		}
		creationController = m_Prefab.AddComponent<CreationController>();
		creationController.enabled = false;
		creationController.Init(m_PartGroup.transform, m_MeshGroup.transform, m_DecalGroup.transform, m_EffectGroup.transform, this);
		switch (m_Attribute.m_Type)
		{
		case ECreation.Sword:
		{
			PeSword peSword2 = m_Prefab.AddComponent<PeSword>();
			VCPSwordHilt vCPSwordHilt3 = FindComponent<VCPSwordHilt>(EVCComponent.cpSwordHilt);
			vCPSwordHilt3.CopyTo(peSword2, this);
			float weight4 = creationController.creationData.m_Attribute.m_Weight;
			peSword2.m_AnimSpeed = VCUtility.GetSwordAnimSpeed(weight4);
			if (vCPSwordHilt3.Attacktrigger != null)
			{
				for (int l = 0; l < vCPSwordHilt3.Attacktrigger.attackParts.Length; l++)
				{
					vCPSwordHilt3.Attacktrigger.attackParts[l].capsule.heigh = creationController.creationData.m_Attribute.m_AtkHeight.x;
					vCPSwordHilt3.Attacktrigger.attackParts[l].capsule.offset.y -= creationController.creationData.m_Attribute.m_AtkHeight.z;
				}
			}
			break;
		}
		case ECreation.SwordLarge:
		{
			PeSword peSword = m_Prefab.AddComponent<PeSword>();
			VCPSwordHilt vCPSwordHilt2 = FindComponent<VCPSwordHilt>(EVCComponent.cpLgSwordHilt);
			vCPSwordHilt2.CopyTo(peSword, this);
			float weight3 = creationController.creationData.m_Attribute.m_Weight;
			peSword.m_AnimSpeed = VCUtility.GetSwordAnimSpeed(weight3);
			if (vCPSwordHilt2.Attacktrigger != null)
			{
				for (int k = 0; k < vCPSwordHilt2.Attacktrigger.attackParts.Length; k++)
				{
					vCPSwordHilt2.Attacktrigger.attackParts[k].capsule.heigh = creationController.creationData.m_Attribute.m_AtkHeight.x;
					vCPSwordHilt2.Attacktrigger.attackParts[k].capsule.offset.y -= creationController.creationData.m_Attribute.m_AtkHeight.z;
				}
			}
			break;
		}
		case ECreation.SwordDouble:
		{
			PETwoHandWeapon pETwoHandWeapon = m_Prefab.AddComponent<PETwoHandWeapon>();
			VCPSwordHilt vCPSwordHilt = FindComponent<VCPSwordHilt>(EVCComponent.cpDbSwordHilt);
			vCPSwordHilt.CopyTo(pETwoHandWeapon, this);
			pETwoHandWeapon.m_LHandWeapon = m_RootL;
			float weight2 = creationController.creationData.m_Attribute.m_Weight;
			pETwoHandWeapon.m_AnimSpeed = VCUtility.GetSwordAnimSpeed(weight2);
			int num4 = 0;
			{
				foreach (VCComponentData component12 in m_IsoData.m_Components)
				{
					if (component12.m_Type == EVCComponent.cpDbSwordHilt && (component12 as VCFixedHandPartData).m_LeftHand && vCPSwordHilt.Attacktrigger != null)
					{
						num4++;
						VCPSwordHilt component = component12.m_Entity.GetComponent<VCPSwordHilt>();
						if (component == null)
						{
							continue;
						}
						for (int i = 0; i < vCPSwordHilt.Attacktrigger.attackParts.Length; i++)
						{
							component.Attacktrigger.attackParts[i].capsule.heigh = creationController.creationData.m_Attribute.m_AtkHeight.x;
							component.Attacktrigger.attackParts[i].capsule.offset.y -= creationController.creationData.m_Attribute.m_AtkHeight.z;
						}
					}
					if (component12.m_Type == EVCComponent.cpDbSwordHilt && !(component12 as VCFixedHandPartData).m_LeftHand && vCPSwordHilt.Attacktrigger != null)
					{
						num4++;
						VCPSwordHilt component2 = component12.m_Entity.GetComponent<VCPSwordHilt>();
						if (component2 == null)
						{
							continue;
						}
						for (int j = 0; j < vCPSwordHilt.Attacktrigger.attackParts.Length; j++)
						{
							component2.Attacktrigger.attackParts[j].capsule.heigh = creationController.creationData.m_Attribute.m_AtkHeight.y;
							component2.Attacktrigger.attackParts[j].capsule.offset.y -= creationController.creationData.m_Attribute.m_AtkHeight.w;
						}
					}
					if (num4 == 2)
					{
						break;
					}
				}
				break;
			}
		}
		case ECreation.Bow:
		{
			PEBow target2 = m_Prefab.AddComponent<PEBow>();
			VCPBowGrip vCPBowGrip = FindComponent<VCPBowGrip>(EVCComponent.cpBowGrip);
			vCPBowGrip.CopyTo(target2, this);
			break;
		}
		case ECreation.Axe:
		{
			PEAxe pEAxe = m_Prefab.AddComponent<PEAxe>();
			VCPAxeHilt vCPAxeHilt = FindComponent<VCPAxeHilt>(EVCComponent.cpAxeHilt);
			vCPAxeHilt.CopyTo(pEAxe, this);
			float weight = creationController.creationData.m_Attribute.m_Weight;
			pEAxe.m_AnimSpeed = VCUtility.GetAxeAnimSpeed(weight);
			break;
		}
		case ECreation.Shield:
		{
			PESheild pESheild = m_Prefab.AddComponent<PESheild>();
			pESheild.showOnVehicle = false;
			break;
		}
		case ECreation.HandGun:
		case ECreation.Rifle:
		{
			PEGun target = m_Prefab.AddComponent<PEGun>();
			VCPGunHandle vCPGunHandle = FindComponent<VCPGunHandle>(EVCComponent.cpGunHandle);
			VCPGunMuzzle vCPGunMuzzle = FindComponent<VCPGunMuzzle>(EVCComponent.cpGunMuzzle);
			vCPGunHandle.CopyTo(target);
			vCPGunMuzzle.CopyTo(target);
			break;
		}
		case ECreation.Vehicle:
		{
			VCParticlePlayer vCParticlePlayer5 = m_Prefab.AddComponent<VCParticlePlayer>();
			vCParticlePlayer5.FunctionTag = 2;
			vCParticlePlayer5.LocalPosition = creationController.bounds.center;
			break;
		}
		case ECreation.Aircraft:
		{
			VCParticlePlayer vCParticlePlayer4 = m_Prefab.AddComponent<VCParticlePlayer>();
			vCParticlePlayer4.FunctionTag = 2;
			vCParticlePlayer4.LocalPosition = creationController.bounds.center;
			break;
		}
		case ECreation.Boat:
		{
			VCParticlePlayer vCParticlePlayer3 = m_Prefab.AddComponent<VCParticlePlayer>();
			vCParticlePlayer3.FunctionTag = 2;
			vCParticlePlayer3.LocalPosition = creationController.bounds.center;
			break;
		}
		case ECreation.Robot:
		{
			VCParticlePlayer vCParticlePlayer2 = m_Prefab.AddComponent<VCParticlePlayer>();
			vCParticlePlayer2.FunctionTag = 2;
			vCParticlePlayer2.LocalPosition = creationController.bounds.center;
			break;
		}
		case ECreation.AITurret:
		{
			VCParticlePlayer vCParticlePlayer = m_Prefab.AddComponent<VCParticlePlayer>();
			vCParticlePlayer.FunctionTag = 2;
			vCParticlePlayer.LocalPosition = creationController.bounds.center;
			break;
		}
		}
	}

	public int SendToPlayer(out ItemObject item, bool pay = true)
	{
		item = null;
		if (m_Attribute.m_Type == ECreation.Null)
		{
			return 0;
		}
		if (m_Prefab == null)
		{
			return 0;
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return 0;
		}
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(m_ObjectID);
		int result = 1;
		if (!pay)
		{
			item = PeSingleton<ItemMgr>.Instance.CreateItem(m_ObjectID);
			return result;
		}
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		SlotList slotList = cmpt.package.GetSlotList((ItemPackage.ESlotType)itemProto.tabIndex);
		if (slotList.GetVacancyCount() > 0)
		{
			item = PeSingleton<ItemMgr>.Instance.CreateItem(m_ObjectID);
			if (slotList.Add(item, isNew: true) && !VCEditor.Instance.m_CheatWhenMakeCreation && !PeGameMgr.IsSingleBuild)
			{
				foreach (KeyValuePair<int, int> item2 in m_Attribute.m_Cost)
				{
					if (item2.Value > 0)
					{
						cmpt.package.Destroy(item2.Key, item2.Value);
					}
				}
			}
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.EXPORT_FULL);
			result = -1;
		}
		GameUI.Instance.mItemPackageCtrl.ResetItem();
		return result;
	}

	public void DestroyPrefab()
	{
		if (m_Prefab != null)
		{
			UnityEngine.Object.Destroy(m_Prefab);
		}
		m_Prefab = null;
		m_Root = null;
		m_PartGroup = null;
		m_MeshGroup = null;
		m_DecalGroup = null;
		m_EffectGroup = null;
	}

	public static int QueryNewSubSkillID(int objectid)
	{
		int num = s_SubSkillStartID + (objectid - 100000000) * 1024 - 1;
		int num2 = num + 1024;
		foreach (EffSkill s_tblEffSkill in EffSkill.s_tblEffSkills)
		{
			if (s_tblEffSkill.m_id > num && s_tblEffSkill.m_id < num2)
			{
				num = s_tblEffSkill.m_id;
			}
		}
		return num + 1;
	}

	public static ItemProto StaticGenItemData(int obj_id, VCIsoHeadData headinfo, CreationAttr attr)
	{
		if (attr.m_Type == ECreation.Null)
		{
			return null;
		}
		ItemProto itemProto = new ItemProto();
		itemProto.name = headinfo.Name;
		itemProto.englishDescription = headinfo.Desc;
		itemProto.itemLabel = 100;
		itemProto.setUp = 0;
		itemProto.resourcePath = "0";
		itemProto.resourcePath1 = "0";
		itemProto.equipReplacePos = 0;
		itemProto.currencyValue = Mathf.CeilToInt(attr.m_SellPrice);
		itemProto.currencyValue2 = Mathf.CeilToInt(attr.m_SellPrice);
		itemProto.durabilityMax = Mathf.CeilToInt(attr.m_Durability);
		itemProto.repairLevel = 1;
		itemProto.maxStackNum = 1;
		itemProto.equipSex = PeSex.Undefined;
		itemProto.id = obj_id;
		itemProto.level = 1;
		itemProto.repairMaterialList = new List<MaterialItem>(attr.m_Cost.Count);
		itemProto.strengthenMaterialList = new List<MaterialItem>(attr.m_Cost.Count);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (KeyValuePair<int, int> item in attr.m_Cost)
		{
			if (item.Value == 0)
			{
				continue;
			}
			Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.FindByProductId(item.Key);
			int key = item.Key;
			int value = item.Value;
			if (formula == null)
			{
				if (dictionary.ContainsKey(key))
				{
					Dictionary<int, int> dictionary2;
					Dictionary<int, int> dictionary3 = (dictionary2 = dictionary);
					int key2;
					int key3 = (key2 = key);
					key2 = dictionary2[key2];
					dictionary3[key3] = key2 + value;
				}
				else
				{
					dictionary[key] = value;
				}
				continue;
			}
			for (int i = 0; i < formula.materials.Count; i++)
			{
				key = formula.materials[i].itemId;
				value = formula.materials[i].itemCount * item.Value;
				if (dictionary.ContainsKey(key))
				{
					Dictionary<int, int> dictionary4;
					Dictionary<int, int> dictionary5 = (dictionary4 = dictionary);
					int key2;
					int key4 = (key2 = key);
					key2 = dictionary4[key2];
					dictionary5[key4] = key2 + value;
				}
				else
				{
					dictionary[key] = value;
				}
			}
		}
		foreach (KeyValuePair<int, int> item2 in dictionary)
		{
			int num = item2.Value / 2;
			if (num > 0)
			{
				itemProto.repairMaterialList.Add(new MaterialItem
				{
					protoId = item2.Key,
					count = num
				});
				itemProto.strengthenMaterialList.Add(new MaterialItem
				{
					protoId = item2.Key,
					count = num
				});
			}
		}
		switch (attr.m_Type)
		{
		case ECreation.Sword:
			itemProto.equipType = EquipType.Sword;
			itemProto.icon = new string[3] { "0", "0", "0" };
			itemProto.equipPos = 16;
			itemProto.itemClassId = 68;
			itemProto.tabIndex = 1;
			itemProto.sortLabel = 9910;
			itemProto.durabilityFactor = 1f;
			itemProto.editorTypeId = 4;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.Atk, attr.m_Attack);
			itemProto.buffId = 30200087;
			break;
		case ECreation.SwordLarge:
		case ECreation.SwordDouble:
			itemProto.icon = new string[3] { "0", "0", "0" };
			itemProto.equipPos = 528;
			itemProto.itemClassId = 68;
			itemProto.tabIndex = 1;
			itemProto.sortLabel = 9910;
			itemProto.durabilityFactor = 1f;
			itemProto.editorTypeId = 4;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.Atk, attr.m_Attack);
			itemProto.buffId = 30200087;
			break;
		case ECreation.Bow:
			itemProto.equipType = EquipType.Bow;
			itemProto.icon = new string[3] { "0", "0", "0" };
			itemProto.equipPos = 528;
			itemProto.itemClassId = 85;
			itemProto.tabIndex = 1;
			itemProto.sortLabel = 9914;
			itemProto.durabilityFactor = 1f;
			itemProto.editorTypeId = 10;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.Atk, attr.m_Attack);
			itemProto.buffId = 30200087;
			break;
		case ECreation.Axe:
			itemProto.equipType = EquipType.Axe;
			itemProto.icon = new string[3] { "0", "0", "0" };
			itemProto.equipPos = 16;
			itemProto.itemClassId = 84;
			itemProto.tabIndex = 1;
			itemProto.sortLabel = 9912;
			itemProto.durabilityFactor = 1f;
			itemProto.editorTypeId = 7;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.Atk, attr.m_Attack * PEVCConfig.instance.axeAttackScale);
			itemProto.propertyList.AddProperty(AttribType.CutDamage, attr.m_Attack * PEVCConfig.instance.axeCutDamageScale);
			itemProto.propertyList.AddProperty(AttribType.CutBouns, 0.03f);
			itemProto.buffId = 30200092;
			break;
		case ECreation.Shield:
			itemProto.equipType = EquipType.Shield_Hand;
			itemProto.icon = new string[3] { "0", "0", "0" };
			itemProto.equipPos = 512;
			itemProto.itemClassId = 69;
			itemProto.tabIndex = 1;
			itemProto.sortLabel = 9920;
			itemProto.durabilityFactor = 0.01f;
			itemProto.editorTypeId = 9;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.Def, attr.m_Defense);
			itemProto.propertyList.AddProperty(AttribType.ShieldMeleeProtect, Mathf.Clamp(attr.m_Defense / 330f + 0.2f, 0.2f, 0.87f));
			itemProto.buffId = 30200093;
			break;
		case ECreation.HandGun:
			itemProto.equipType = EquipType.HandGun;
			itemProto.icon = new string[3] { "0", "0", "0" };
			itemProto.equipPos = 528;
			itemProto.itemClassId = 70;
			itemProto.tabIndex = 1;
			itemProto.sortLabel = 9930;
			itemProto.durabilityFactor = 1f;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.Atk, attr.m_Attack);
			itemProto.buffId = 30200095;
			itemProto.editorTypeId = 11;
			break;
		case ECreation.Rifle:
			itemProto.equipType = EquipType.Rifle;
			itemProto.icon = new string[3] { "0", "0", "0" };
			itemProto.equipPos = 528;
			itemProto.itemClassId = 71;
			itemProto.tabIndex = 1;
			itemProto.sortLabel = 9940;
			itemProto.durabilityFactor = 1f;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.Atk, attr.m_Attack);
			itemProto.buffId = 30200095;
			itemProto.editorTypeId = 11;
			break;
		case ECreation.Vehicle:
			itemProto.equipType = EquipType.Null;
			itemProto.icon = new string[3] { "0", "leftup_putdown", "0" };
			itemProto.equipPos = 0;
			itemProto.itemClassId = 63;
			itemProto.tabIndex = 0;
			itemProto.sortLabel = 9950;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.HpMax, attr.m_Durability);
			itemProto.propertyList.AddProperty(AttribType.Hp, attr.m_Durability);
			itemProto.engergyMax = (int)attr.m_MaxFuel;
			break;
		case ECreation.Aircraft:
			itemProto.equipType = EquipType.Null;
			itemProto.icon = new string[3] { "0", "leftup_putdown", "0" };
			itemProto.equipPos = 0;
			itemProto.itemClassId = 66;
			itemProto.tabIndex = 0;
			itemProto.sortLabel = 9960;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.HpMax, attr.m_Durability);
			itemProto.propertyList.AddProperty(AttribType.Hp, attr.m_Durability);
			itemProto.engergyMax = (int)attr.m_MaxFuel;
			break;
		case ECreation.Boat:
			itemProto.equipType = EquipType.Null;
			itemProto.icon = new string[3] { "0", "leftup_putdown", "0" };
			itemProto.equipPos = 0;
			itemProto.itemClassId = 65;
			itemProto.tabIndex = 0;
			itemProto.sortLabel = 9970;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.HpMax, attr.m_Durability);
			itemProto.propertyList.AddProperty(AttribType.Hp, attr.m_Durability);
			itemProto.engergyMax = (int)attr.m_MaxFuel;
			break;
		case ECreation.SimpleObject:
			itemProto.equipType = EquipType.Null;
			itemProto.icon = new string[3] { "0", "leftup_putdown", "0" };
			itemProto.equipPos = 0;
			itemProto.itemClassId = 78;
			itemProto.sortLabel = 9980;
			itemProto.tabIndex = 0;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.HpMax, attr.m_Durability);
			itemProto.propertyList.AddProperty(AttribType.Hp, attr.m_Durability);
			break;
		case ECreation.ArmorHead:
		case ECreation.ArmorBody:
		case ECreation.ArmorArmAndLeg:
		case ECreation.ArmorHandAndFoot:
		case ECreation.ArmorDecoration:
			itemProto.equipType = EquipType.Null;
			itemProto.icon = new string[3] { "0", "0", "0" };
			itemProto.equipPos = 0;
			itemProto.itemClassId = 82;
			itemProto.sortLabel = (int)(9990 + attr.m_Type);
			itemProto.tabIndex = 3;
			itemProto.durabilityFactor = 0.01f;
			break;
		case ECreation.Robot:
			itemProto.equipType = EquipType.Null;
			itemProto.icon = new string[3] { "0", "leftup_putdown", "0" };
			itemProto.equipPos = 0;
			itemProto.itemClassId = 86;
			itemProto.tabIndex = 0;
			itemProto.sortLabel = 9945;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.HpMax, attr.m_Durability);
			itemProto.propertyList.AddProperty(AttribType.Hp, attr.m_Durability);
			itemProto.engergyMax = (int)attr.m_MaxFuel;
			break;
		case ECreation.AITurret:
			itemProto.equipType = EquipType.Null;
			itemProto.icon = new string[3] { "0", "leftup_putdown", "0" };
			itemProto.equipPos = 0;
			itemProto.itemClassId = 87;
			itemProto.tabIndex = 0;
			itemProto.sortLabel = 9946;
			if (itemProto.propertyList == null)
			{
				itemProto.propertyList = new ItemProto.PropertyList();
			}
			itemProto.propertyList.AddProperty(AttribType.HpMax, attr.m_Durability);
			itemProto.propertyList.AddProperty(AttribType.Hp, attr.m_Durability);
			itemProto.engergyMax = (int)attr.m_MaxFuel;
			itemProto.unchargeable = attr.m_Defense < 0.5f;
			break;
		default:
			itemProto.equipType = EquipType.Null;
			itemProto.icon = new string[3] { "0", "0", "0" };
			itemProto.equipPos = 0;
			itemProto.sortLabel = 10000;
			itemProto.tabIndex = 0;
			break;
		}
		return itemProto;
	}

	private ItemProto GenItemData()
	{
		return StaticGenItemData(m_ObjectID, m_IsoData.m_HeadInfo, m_Attribute);
	}

	public void Register()
	{
		if (m_Attribute.m_Type == ECreation.Null || m_Prefab == null)
		{
			return;
		}
		ItemProto itemProto = GenItemData();
		AddWeaponInfo(itemProto, m_Prefab);
		if (itemProto != null)
		{
			byte[] iconTex = m_IsoData.m_HeadInfo.IconTex;
			if (iconTex != null)
			{
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(iconTex);
				texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
				itemProto.iconTex = texture2D;
			}
			if (PeSingleton<ItemProto.Mgr>.Instance.Get(itemProto.id) == null)
			{
				PeSingleton<ItemProto.Mgr>.Instance.Add(itemProto);
			}
		}
	}

	private void AddWeaponInfo(ItemProto itemProto, GameObject obj)
	{
		switch (m_Attribute.m_Type)
		{
		case ECreation.Sword:
		case ECreation.SwordLarge:
		case ECreation.SwordDouble:
		{
			PeSword[] componentsInChildren3 = obj.GetComponentsInChildren<PeSword>(includeInactive: true);
			if (componentsInChildren3 != null && componentsInChildren3.Length > 0)
			{
				PeSword peSword = componentsInChildren3[0];
				AttackMode attackMode = new AttackMode();
				itemProto.weaponInfo = new ItemProto.WeaponInfo();
				itemProto.weaponInfo.attackModes = new AttackMode[1];
				itemProto.weaponInfo.attackModes[0] = attackMode;
				attackMode.type = peSword.m_AttackMode[0].type;
				attackMode.minRange = peSword.m_AttackMode[0].minRange;
				attackMode.maxRange = peSword.m_AttackMode[0].maxRange;
				attackMode.minSwitchRange = peSword.m_AttackMode[0].minSwitchRange;
				attackMode.maxSwitchRange = peSword.m_AttackMode[0].maxSwitchRange;
				attackMode.minAngle = peSword.m_AttackMode[0].minAngle;
				attackMode.maxAngle = peSword.m_AttackMode[0].maxAngle;
				attackMode.frequency = peSword.m_AttackMode[0].frequency;
				attackMode.damage = itemProto.propertyList.GetProperty(AttribType.Atk);
			}
			break;
		}
		case ECreation.Bow:
		{
			PEBow[] componentsInChildren2 = obj.GetComponentsInChildren<PEBow>(includeInactive: true);
			if (componentsInChildren2 != null && componentsInChildren2.Length > 0)
			{
				PEBow pEBow = componentsInChildren2[0];
				AttackMode attackMode = new AttackMode();
				itemProto.weaponInfo = new ItemProto.WeaponInfo();
				itemProto.weaponInfo.attackModes = new AttackMode[1];
				itemProto.weaponInfo.attackModes[0] = attackMode;
				attackMode.type = pEBow.m_AttackMode[0].type;
				attackMode.minRange = pEBow.m_AttackMode[0].minRange;
				attackMode.maxRange = pEBow.m_AttackMode[0].maxRange;
				attackMode.minSwitchRange = pEBow.m_AttackMode[0].minSwitchRange;
				attackMode.maxSwitchRange = pEBow.m_AttackMode[0].maxSwitchRange;
				attackMode.minAngle = pEBow.m_AttackMode[0].minAngle;
				attackMode.maxAngle = pEBow.m_AttackMode[0].maxAngle;
				attackMode.frequency = pEBow.m_AttackMode[0].frequency;
				attackMode.damage = itemProto.propertyList.GetProperty(AttribType.Atk);
				itemProto.weaponInfo.costItem = pEBow.m_CostItemID[0];
			}
			break;
		}
		case ECreation.HandGun:
		case ECreation.Rifle:
		{
			PEGun[] componentsInChildren = obj.GetComponentsInChildren<PEGun>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				PEGun pEGun = componentsInChildren[0];
				AttackMode attackMode = new AttackMode();
				itemProto.weaponInfo = new ItemProto.WeaponInfo();
				itemProto.weaponInfo.attackModes = new AttackMode[1];
				itemProto.weaponInfo.attackModes[0] = attackMode;
				attackMode.type = pEGun.m_AttackMode[0].type;
				attackMode.minRange = pEGun.m_AttackMode[0].minRange;
				attackMode.maxRange = pEGun.m_AttackMode[0].maxRange;
				attackMode.minSwitchRange = pEGun.m_AttackMode[0].minSwitchRange;
				attackMode.maxSwitchRange = pEGun.m_AttackMode[0].maxSwitchRange;
				attackMode.minAngle = pEGun.m_AttackMode[0].minAngle;
				attackMode.maxAngle = pEGun.m_AttackMode[0].maxAngle;
				attackMode.frequency = pEGun.m_AttackMode[0].frequency;
				attackMode.damage = itemProto.propertyList.GetProperty(AttribType.Atk);
				itemProto.weaponInfo.costItem = ((pEGun.m_AmmoItemIDList.Length > 0) ? pEGun.m_AmmoItemIDList[0] : 0);
				itemProto.weaponInfo.useEnergry = pEGun.m_AmmoType == AmmoType.Energy;
				itemProto.weaponInfo.costPerShoot = ((!itemProto.weaponInfo.useEnergry) ? 1 : ((int)pEGun.m_EnergyPerShoot));
			}
			break;
		}
		}
	}

	public static void UnregisterAll()
	{
		if (EffSkill.s_tblEffSkills == null || EffSkillBuff.s_tblEffSkillBuffs == null)
		{
			return;
		}
		PeSingleton<ItemProto.Mgr>.Instance.ClearCreation();
		for (int num = EffSkill.s_tblEffSkills.Count - 1; num >= 0; num--)
		{
			if (EffSkill.s_tblEffSkills[num].m_id >= s_SubSkillStartID)
			{
				EffSkill.s_tblEffSkills.RemoveAt(num);
			}
		}
		for (int num2 = EffSkillBuff.s_tblEffSkillBuffs.Count - 1; num2 >= 0; num2--)
		{
			if (EffSkillBuff.s_tblEffSkillBuffs[num2].m_id >= s_BuffStartID)
			{
				EffSkillBuff.s_tblEffSkillBuffs.RemoveAt(num2);
			}
		}
	}
}
