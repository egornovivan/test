public class CharacterName
{
	public const string DefaultFamilyName = "FamilyName";

	public const string DefaultGivenName = "GivenName";

	private string mFamilyName;

	private string mGivenName;

	private string mFullName;

	private static CharacterName sDefault;

	public static CharacterName Default
	{
		get
		{
			if (sDefault == null)
			{
				sDefault = new CharacterName("GivenName", "FamilyName");
			}
			return sDefault;
		}
	}

	public string GivenName => mGivenName;

	public string FamilyName => mFamilyName;

	public string FullName
	{
		get
		{
			if (string.IsNullOrEmpty(mFullName))
			{
				mFullName = GivenName + " " + FamilyName;
			}
			return mFullName;
		}
	}

	public CharacterName()
	{
	}

	public CharacterName(string fullName, string givenName, string familyName)
	{
		mFullName = fullName;
		mGivenName = givenName;
		mFamilyName = familyName;
	}

	public CharacterName(string givenName, string familyName)
		: this(string.Empty, givenName, familyName)
	{
	}

	public CharacterName(string name)
		: this(name, name, string.Empty)
	{
	}
}
