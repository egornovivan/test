public struct MsgBoxValue
{
	public int mStrId;

	public string mDefaultValue;

	public MsgBoxValue(int _mStrId, string _mDefaultValue)
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
