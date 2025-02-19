public class UITextValue
{
	public int mStrId;

	public string mDefaultValue;

	public UITextValue(int _mStrId, string _mDefaultValue)
	{
		mStrId = _mStrId;
		mDefaultValue = _mDefaultValue;
	}

	public string GetString()
	{
		string @string = PELocalization.GetString(mStrId);
		return (!(@string == string.Empty)) ? @string : mDefaultValue;
	}
}
