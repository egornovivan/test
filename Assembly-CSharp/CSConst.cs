public class CSConst
{
	public enum ObjectType
	{
		Assembly = 1,
		Storage = 2,
		Engineer = 3,
		Enhance = 4,
		Repair = 5,
		Recyle = 6,
		Farm = 7,
		Factory = 8,
		Processing = 9,
		Dwelling = 21,
		PowerPlant = 32,
		PowerPlant_Coal = 33,
		PowerPlant_Solar = 34,
		PowerPlant_Fusion = 35,
		Trade = 10,
		Train = 11,
		Check = 12,
		Treat = 13,
		Tent = 14,
		None = 12121
	}

	public enum PowerPlantType
	{
		Coal = 33,
		Solar,
		Fusion
	}

	public enum CreatorType
	{
		Managed,
		NoManaged
	}

	public enum EMonsterType
	{
		Land,
		Sky,
		Water
	}

	public const int NONELECTICS = 20;

	public const int dtDefault = 0;

	public const int dtAssembly = 1;

	public const int dtStorage = 2;

	public const int dtEngineer = 3;

	public const int dtEnhance = 4;

	public const int dtRepair = 5;

	public const int dtRecyle = 6;

	public const int dtFarm = 7;

	public const int dtFactory = 8;

	public const int dtProcessing = 9;

	public const int dtTrade = 10;

	public const int dtTrain = 11;

	public const int dtCheck = 12;

	public const int dtTreat = 13;

	public const int dtTent = 14;

	public const int dtDwelling = 21;

	public const int dtPowerPlant = 32;

	public const int dtppCoal = 33;

	public const int dtppSolar = 34;

	public const int dtppFusion = 35;

	public const int dtPersonnel = 50;

	public const int etAssembly = 1;

	public const int etStorage = 2;

	public const int etEngineer = 3;

	public const int etEnhance = 4;

	public const int etRepair = 5;

	public const int etRecyle = 6;

	public const int etFarm = 7;

	public const int etFactory = 8;

	public const int etProcessing = 9;

	public const int etTrade = 10;

	public const int etTrain = 11;

	public const int etCheck = 12;

	public const int etTreat = 13;

	public const int etTent = 14;

	public const int etDwelling = 21;

	public const int etPowerPlant = 32;

	public const int etppCoal = 33;

	public const int etppSolar = 34;

	public const int etppFusion = 35;

	public const int etUnknow = 12121;

	public const int etID_Farm = -100;

	public const int rrtNoAssembly = 0;

	public const int rrtHasAssembly = 1;

	public const int rrtOutOfRadius = 2;

	public const int rrtOutOfRange = 3;

	public const int rrtSucceed = 4;

	public const int rrtUnkown = 5;

	public const int rrtTooCloseToNativeCamp = 6;

	public const int rrtTooCloseToNativeCamp1 = 7;

	public const int rrtTooCloseToNativeCamp2 = 8;

	public const int rrtAreaUnavailable = 9;

	public const int pstUnknown = 0;

	public const int pstPrepare = 1;

	public const int pstIdle = 2;

	public const int pstRest = 3;

	public const int pstWork = 4;

	public const int pstFollow = 5;

	public const int pstDead = 6;

	public const int pstAtk = 7;

	public const int pstPatrol = 8;

	public const int pstPlant = 9;

	public const int pstWatering = 10;

	public const int pstWeeding = 11;

	public const int pstGain = 12;

	public const int potUnknown = -1;

	public const int potDweller = 0;

	public const int potWorker = 1;

	public const int potSoldier = 2;

	public const int potFarmer = 3;

	public const int potFollower = 4;

	public const int potProcessor = 5;

	public const int potDoctor = 6;

	public const int potTrainer = 7;

	public const int pwtUnknown = -1;

	public const int pwtNoWork = 0;

	public const int pwtNormalWork = 1;

	public const int pwtWorkWhenNeed = 2;

	public const int pwtWorkaholic = 3;

	public const int pwtFarmForMag = 4;

	public const int pwtFarmForHarvest = 5;

	public const int pwtFarmForPlant = 6;

	public const int pwtPatrol = 7;

	public const int pwtGuard = 8;

	public const int pwtStandby = 9;

	public const int pwtAssigned = 10;

	public const int eetDestroy = 1;

	public const int eetHurt = 2;

	public const int ehtHurt = 2;

	public const int ehtRestore = 8;

	public const int eetAssembly_AddBuilding = 2001;

	public const int eetAssembly_RemoveBuilding = 2002;

	public const int eetAssembly_Upgraded = 2003;

	public const int eetStorage_HistoryDequeue = 3001;

	public const int eetStorage_HistoryEnqueue = 3002;

	public const int eetStorage_PackageRemoveItem = 3003;

	public const int eetDwellings_ChangeState = 4001;

	public const int eetCommon_ChangeAssembly = 5001;

	public const int eetFarm_OnPlant = 6001;

	public const int egtUrgent = 0;

	public const int egtHigh = 1;

	public const int egtMedium = 2;

	public const int egtLow = 3;

	public const int egtTotal = 4;

	public const int ciDefMgCamp = 0;

	public const int ciDefNoMgCamp = 10000;

	public const int cetAddEntity = 1001;

	public const int cetRemoveEntity = 1002;

	public const int cetAddPersonnel = 1003;

	public const int cetRemovePersonnel = 1004;
}
