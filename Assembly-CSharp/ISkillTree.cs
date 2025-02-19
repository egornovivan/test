public interface ISkillTree
{
	bool CheckEquipEnable(int type, int level);

	bool CheckDriveEnable(int type, int level);

	bool CheckMinerGetRare();

	bool CheckCutterGetRare();

	bool CheckHunterGetRare();

	bool CheckUnlockColony(int colonytype);

	bool CheckUnlockProductItemLevel(int unlocklevel);

	bool CheckUnlockProductItemType(int unlocktype);

	float CheckReduceTime(float srcTime);

	bool CheckBuildShape(int index);

	bool CheckBuildBlockLevel(int level);

	bool CheckUnlockBuildBlockBevel();

	bool CheckUnlockBuildBlockIso();

	bool CheckUnlockBuildBlockVoxel();

	bool CheckUnlockCusProduct(int unlocktype);
}
