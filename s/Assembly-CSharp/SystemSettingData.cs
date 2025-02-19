public class SystemSettingData
{
	private static SystemSettingData mInstance;

	public bool mHasData;

	public string mVersion = "0.795";

	public string mLanguage = "english";

	public static SystemSettingData Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new SystemSettingData();
			}
			return mInstance;
		}
	}
}
