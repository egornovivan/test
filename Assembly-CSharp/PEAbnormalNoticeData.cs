using System.Collections.Generic;
using Mono.Data.SqliteClient;
using PETools;

public class PEAbnormalNoticeData
{
	public float trigger_TimeInterval;

	public int[] trigger_AbnormalHit;

	public float[] hit_AreaTime;

	public float hit_HitRate;

	public AbnormalData.HitAttr[] hit_Attr;

	public int eff_HumanAudio;

	public int[] eff_Contents;

	private static PEAbnormalNoticeData[] g_Datas;

	public static PEAbnormalNoticeData[] datas => g_Datas;

	public static void LoadData()
	{
		List<PEAbnormalNoticeData> list = new List<PEAbnormalNoticeData>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("AbnormalNotice");
		while (sqliteDataReader.Read())
		{
			PEAbnormalNoticeData pEAbnormalNoticeData = new PEAbnormalNoticeData();
			pEAbnormalNoticeData.trigger_TimeInterval = Db.GetFloat(sqliteDataReader, "Trigger_Time");
			pEAbnormalNoticeData.trigger_AbnormalHit = Db.GetIntArray(sqliteDataReader, "Trigger_Abnormal");
			pEAbnormalNoticeData.hit_Attr = AbnormalData.HitAttr.GetHitAttrArray(sqliteDataReader, "Hit_Attr");
			pEAbnormalNoticeData.hit_AreaTime = Db.GetFloatArray(sqliteDataReader, "Hit_AreaTime");
			pEAbnormalNoticeData.hit_HitRate = Db.GetFloat(sqliteDataReader, "Hit_Rate");
			pEAbnormalNoticeData.eff_HumanAudio = Db.GetInt(sqliteDataReader, "Eff_HumanAudio");
			pEAbnormalNoticeData.eff_Contents = Db.GetIntArray(sqliteDataReader, "Eff_Content");
			list.Add(pEAbnormalNoticeData);
		}
		g_Datas = list.ToArray();
	}
}
