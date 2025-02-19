namespace AiAsset;

public class AiData
{
	public static void LoadData()
	{
		AiHatredData.LoadData();
		AiHarmData.LoadData();
		AiDataBlock.LoadData();
		AiDamageTypeData.LoadData();
		AISpawnDataRepository.LoadData();
	}
}
