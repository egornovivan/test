public interface ISkillTree
{
	bool CheckEquipEnable(int type, int level);

	bool CheckDriveEnable(int type, int level);

	bool CheckMinerGetRare();

	bool CheckCutterGetRare();

	bool CheckHunterGetRare();

	bool CheckUnlockColony(int colonytype);

	bool CheckUnlockProductItemType(int unlocktype);
}
