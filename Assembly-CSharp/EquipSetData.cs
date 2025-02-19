using System.Collections.Generic;
using ItemAsset;
using Mono.Data.SqliteClient;
using PETools;

public class EquipSetData
{
	public float attack;

	public float defence;

	public float maxHp;

	public float hpRecovery;

	public float maxStamina;

	public float staminaRecovery;

	public float maxHunger;

	public float hungerDownRate;

	public float digPower;

	public float chopPower;

	public float maxComfort;

	public float comfortSpendingRate;

	public string desStr;

	public int[] buffIDs;

	public static Dictionary<int, EquipSetData> g_EquipSetDatas;

	public static void LoadData()
	{
		g_EquipSetDatas = new Dictionary<int, EquipSetData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("SingleSet");
		while (sqliteDataReader.Read())
		{
			int @int = Db.GetInt(sqliteDataReader, "id");
			EquipSetData equipSetData = new EquipSetData();
			equipSetData.attack = Db.GetFloat(sqliteDataReader, "Attack");
			equipSetData.defence = Db.GetFloat(sqliteDataReader, "Defence");
			equipSetData.maxHp = Db.GetFloat(sqliteDataReader, "MaxHp");
			equipSetData.hpRecovery = Db.GetFloat(sqliteDataReader, "HpRecovery");
			equipSetData.maxStamina = Db.GetFloat(sqliteDataReader, "MaxStamina");
			equipSetData.staminaRecovery = Db.GetFloat(sqliteDataReader, "StaminaRecovery");
			equipSetData.maxHunger = Db.GetFloat(sqliteDataReader, "MaxHunger");
			equipSetData.hungerDownRate = Db.GetFloat(sqliteDataReader, "HungerDownRate");
			equipSetData.digPower = Db.GetFloat(sqliteDataReader, "DigPower");
			equipSetData.chopPower = Db.GetFloat(sqliteDataReader, "ChopPower");
			equipSetData.maxComfort = Db.GetFloat(sqliteDataReader, "MaxComfort");
			equipSetData.comfortSpendingRate = Db.GetFloat(sqliteDataReader, "ComfortSpendingRate");
			equipSetData.buffIDs = Db.GetIntArray(sqliteDataReader, "SkBuffId");
			equipSetData.ProductDes();
			g_EquipSetDatas[@int] = equipSetData;
		}
	}

	public static EquipSetData GetData(ItemObject itemObj)
	{
		if (itemObj == null)
		{
			return null;
		}
		if (g_EquipSetDatas.ContainsKey(itemObj.protoId))
		{
			return g_EquipSetDatas[itemObj.protoId];
		}
		return null;
	}

	public static void GetSuitSetEffect(List<ItemObject> equipList, ref List<int> buffList)
	{
		if (buffList == null || equipList == null)
		{
			return;
		}
		for (int i = 0; i < equipList.Count; i++)
		{
			if (equipList[i] != null && g_EquipSetDatas.ContainsKey(equipList[i].protoId) && g_EquipSetDatas[equipList[i].protoId].buffIDs != null)
			{
				buffList.AddRange(g_EquipSetDatas[equipList[i].protoId].buffIDs);
			}
		}
	}

	private void ProductDes()
	{
		desStr = string.Empty;
		string empty = string.Empty;
		if (attack != 0f)
		{
			empty = attack * 100f + "%";
			if (attack > 0f)
			{
				empty = "+" + empty;
			}
			desStr += string.Format(PELocalization.GetString(8000814), empty);
			desStr = string.Format((!(attack > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (defence != 0f)
		{
			empty = defence * 100f + "%";
			if (defence > 0f)
			{
				empty = "+" + empty;
			}
			desStr += string.Format(PELocalization.GetString(8000815), empty);
			desStr = string.Format((!(defence > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (maxHp != 0f)
		{
			empty = ((!(maxHp > 0f)) ? maxHp.ToString() : ("+" + maxHp));
			desStr += string.Format(PELocalization.GetString(8000816), empty);
			desStr = string.Format((!(maxHp > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (hpRecovery != 0f)
		{
			empty = ((!(hpRecovery > 0f)) ? hpRecovery.ToString() : ("+" + hpRecovery));
			desStr += string.Format(PELocalization.GetString(8000817), empty);
			desStr = string.Format((!(hpRecovery > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (maxStamina != 0f)
		{
			empty = ((!(maxStamina > 0f)) ? maxStamina.ToString() : ("+" + maxStamina));
			desStr += string.Format(PELocalization.GetString(8000818), empty);
			desStr = string.Format((!(maxStamina > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (staminaRecovery != 0f)
		{
			empty = staminaRecovery * 100f + "%";
			if (staminaRecovery > 0f)
			{
				empty = "+" + empty;
			}
			desStr += string.Format(PELocalization.GetString(8000819), empty);
			desStr = string.Format((!(staminaRecovery > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (maxHunger != 0f)
		{
			empty = ((!(maxHunger > 0f)) ? maxHunger.ToString() : ("+" + maxHunger));
			desStr += string.Format(PELocalization.GetString(8000820), empty);
			desStr = string.Format((!(maxHunger > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (hungerDownRate != 0f)
		{
			empty = hungerDownRate * 100f + "%";
			if (hungerDownRate > 0f)
			{
				empty = "+" + empty;
			}
			desStr += string.Format(PELocalization.GetString(8000821), empty);
			desStr = string.Format((!(hungerDownRate < 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (digPower != 0f)
		{
			empty = digPower * 100f + "%";
			if (digPower > 0f)
			{
				empty = "+" + empty;
			}
			desStr += string.Format(PELocalization.GetString(8000822), empty);
			desStr = string.Format((!(digPower > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (chopPower != 0f)
		{
			empty = chopPower * 100f + "%";
			if (chopPower > 0f)
			{
				empty = "+" + empty;
			}
			desStr += string.Format(PELocalization.GetString(8000823), empty);
			desStr = string.Format((!(chopPower > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (maxComfort != 0f)
		{
			empty = ((!(maxComfort > 0f)) ? maxComfort.ToString() : ("+" + maxComfort));
			desStr += string.Format(PELocalization.GetString(8000824), empty);
			desStr = string.Format((!(maxComfort > 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
		if (comfortSpendingRate != 0f)
		{
			empty = comfortSpendingRate * 100f + "%";
			if (comfortSpendingRate > 0f)
			{
				empty = "+" + empty;
			}
			desStr += string.Format(PELocalization.GetString(8000825), empty);
			desStr = string.Format((!(comfortSpendingRate < 0f)) ? "[FF1D05]{0}[-]" : "[B7EF54]{0}[-]", desStr) + "\n";
		}
	}
}
