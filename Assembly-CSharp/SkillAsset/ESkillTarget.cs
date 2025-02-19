using UnityEngine;

namespace SkillAsset;

public class ESkillTarget
{
	public const int TAR_Enemy = 1;

	public const int TAR_EnemyBuilding = 2;

	public const int TAR_Self = 4;

	public const int TAR_Partner = 8;

	public const int TAR_PartnerBuilding = 16;

	public const int TAR_aa = 32;

	public const int TAR_Mud = 64;

	public const int TAR_Mine = 128;

	public const int TAR_Wood = 256;

	public const int TAR_Herb = 512;

	public const int TAR_Fish = 1024;

	public const int TAR_VitualMeat = 2048;

	public const int TAR_VitualGrass = 4096;

	public const int TAR_VitualWater = 8192;

	public static int Type2Mask(ESkillTargetType type)
	{
		switch (type)
		{
		case ESkillTargetType.TYPE_SkillRunner:
			return 13;
		case ESkillTargetType.TYPE_Building:
			return 18;
		case ESkillTargetType.TYPE_Mud:
		case ESkillTargetType.TYPE_Mine:
		case ESkillTargetType.TYPE_Wood:
		case ESkillTargetType.TYPE_Herb:
		case ESkillTargetType.TYPE_Fish:
		case ESkillTargetType.TYPE_VitualMeat:
		case ESkillTargetType.TYPE_VitualGrass:
		case ESkillTargetType.TYPE_VitualWater:
			return 1 << (int)(type - 1);
		default:
			Debug.Log("Undefined Type2Mask behavior");
			return 0;
		}
	}
}
