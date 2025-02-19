using System.IO;
using PETools;

public class CharacterName
{
	private const string DefaultFamilyName = "FamilyName";

	private const string DefaultGivenName = "GivenName";

	private static CharacterName sDefault;

	private string mFamilyName;

	private string mGivenName;

	private string mFullName;

	private string mConcatFullName;

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

	public string OverHeadName => givenName;

	public string givenName => mGivenName;

	public string familyName => mFamilyName;

	public string fullName
	{
		get
		{
			if (mFullName != null)
			{
				return mFullName;
			}
			if (mConcatFullName == null)
			{
				if (familyName == null)
				{
					mConcatFullName = givenName;
				}
				else if (givenName == null)
				{
					mConcatFullName = familyName;
				}
				else
				{
					mConcatFullName = givenName + " " + familyName;
				}
			}
			return mConcatFullName;
		}
	}

	private CharacterName(string fullName, string givenName, string familyName)
	{
		mFullName = fullName;
		mGivenName = givenName;
		mFamilyName = familyName;
	}

	public CharacterName(string givenName, string familyName)
		: this(null, givenName, familyName)
	{
	}

	public CharacterName(string name)
		: this(name, name, null)
	{
	}

	public CharacterName()
	{
	}

	public CharacterName InitStoryNpcName(string fullName, string showName)
	{
		mFullName = fullName;
		mGivenName = showName;
		return this;
	}

	public void Import(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			mFullName = Serialize.ReadNullableString(r);
			mFamilyName = Serialize.ReadNullableString(r);
			mGivenName = Serialize.ReadNullableString(r);
		});
	}

	public byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			Serialize.WriteNullableString(w, mFullName);
			Serialize.WriteNullableString(w, mFamilyName);
			Serialize.WriteNullableString(w, mGivenName);
		}, 20);
	}

	public override bool Equals(object obj)
	{
		CharacterName characterName = obj as CharacterName;
		if (obj == null)
		{
			return false;
		}
		if (base.Equals(obj))
		{
			return true;
		}
		if (string.Equals(characterName.mFullName, mFullName) && string.Equals(characterName.mGivenName, mGivenName))
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return fullName;
	}
}
