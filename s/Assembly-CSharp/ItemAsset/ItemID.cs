namespace ItemAsset;

public class ItemID
{
	public const int REVIVE_SHOT = 937;

	public const int MUTILPLE_STARTING_ADVENTURE = 1290;

	public const int MUTILPLE_STARTING_STORY = 1358;

	public const int MULTIPLE_STARTING_BUILD = 1292;

	public const int MULTIPLE_STARTING_ADVENTUREVS = 1300;

	public const int MULTIPLE_STARTING_ADVENTUREFREEMODE = 1301;

	public const int MULTIPLE_STARTING_SURVIVAL = 1305;

	public const int WATERID = 1003;

	public const int HERBAL_JUICE = 916;

	public const int PUREE = 387;

	public const int NUTS = 388;

	public const int RICE = 401;

	public const int COMFORT_INJECTION_1 = 1479;

	public const int COAL = 983;

	public const int ASSEMBLY_CORE = 1127;

	public const int POWER_PLANT_COAL = 1128;

	public const int STORAGE = 1129;

	public const int REPAIR_MACHINE = 1130;

	public const int DWELLING_BED = 1131;

	public const int ENHANCE_MACHINE = 1132;

	public const int RECYCLE_MACHINE = 1133;

	public const int FARM = 1134;

	public const int FACTORY_REPLICATOR = 1135;

	public const int PROCESSING = 1356;

	public const int TRADE_POST = 1357;

	public const int TRAINING_CENTER = 1423;

	public const int MEDICAL_CHECK = 1424;

	public const int MEDICAL_TREAT = 1422;

	public const int MEDICAL_TENT = 1421;

	public const int POWER_PLANT_FUSION = 1558;

	public const int FERTILIZER_SPRAYER = 68;

	public const int MANURE_SPRAYER = 69;

	public const int FERTILIZER_PALLET = 51;

	public const int MANURE_PALLET = 53;

	public const int ISSUED_WEAPON = 33;

	public const int F_ISSUED_OVERALLS = 95;

	public const int F_ISSUED_PANTS = 131;

	public const int F_ISSUED_SHOES = 192;

	public const int M_ISSUED_OVERALLS = 113;

	public const int M_ISSUED_PANTS = 149;

	public const int M_ISSUED_SHOES = 210;

	public const int BATTERY = 228;

	public const int MEAT = 229;

	public const int ARROW = 49;

	public const int BULLET = 50;

	public const int REPAIR_KIT = 1030;

	public const int ENERGY_PACKET = 1029;

	public const int WATER = 1003;

	public const int INSECTICIDE = 1002;

	public const int HEAT_PACK = 1582;

	public const int CreationItemID = 100000000;

	public static bool IsCreation(int id)
	{
		return id >= 100000000;
	}
}
