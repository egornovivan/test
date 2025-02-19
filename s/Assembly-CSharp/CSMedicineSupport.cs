using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class CSMedicineSupport
{
	private static List<MedicineSupply> medicineData = new List<MedicineSupply>();

	public static List<MedicineSupply> AllMedicine => medicineData;

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AbnormalDruggery");
		while (sqliteDataReader.Read())
		{
			MedicineSupply medicineSupply = new MedicineSupply();
			medicineSupply.id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			medicineSupply.protoId = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("prototypeitem_id")));
			medicineSupply.count = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("count")));
			medicineSupply.rounds = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("rounds")));
			medicineData.Add(medicineSupply);
		}
	}
}
