public class CSStorageHistoryAttr
{
	public enum EType
	{
		NpcAddSth,
		NpcUseSth
	}

	public string m_TimeStr;

	public string m_TimeStrColor = "[00FFFF]";

	public string m_NpcName;

	public string m_NameColorStr = "[FFFF00]";

	public string m_ItemStr;

	public int m_Day;

	public EType m_Type;
}
