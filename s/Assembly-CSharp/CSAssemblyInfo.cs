using System;
using System.Collections.Generic;

[Serializable]
public class CSAssemblyInfo : CSInfo
{
	[Serializable]
	public class LevelData
	{
		public float radius;

		public int dwellingsCnt;

		public int barracksCnt;

		public int storageCnt;

		public int armoryCnt;

		public int pubCnt;

		public int farmCnt;

		public int factoryCnt;

		public int clinicCnt;

		public int labCnt;

		public int EngineeringCnt;

		public int RepairMachineCnt;

		public int EnhanceMachineCnt;

		public int RecycleMachineCnt;

		public int coalPlantCnt;

		public int processingCnt;

		public int tradePostCnt = 1;

		public int trainCenterCnt = 1;

		public int medicalCheckCnt = 1;

		public int medicalTreatCnt = 1;

		public int medicalTentCnt = 1;

		public int fusionPlantCnt = 1;

		public List<int> itemIDList = new List<int>();

		public List<int> itemCnt = new List<int>();

		public float upgradeTime;
	}

	public static CSAssemblyInfo _Self = new CSAssemblyInfo();

	public static List<LevelData> m_Levels = new List<LevelData>();
}
