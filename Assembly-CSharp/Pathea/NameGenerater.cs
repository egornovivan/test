using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Pathea;

public class NameGenerater
{
	private class LastName
	{
		public string mText;

		public int mRace;

		public int mLabel;
	}

	private class FirstName
	{
		public int mSex;

		public string mText;

		public int mLabel;
	}

	private List<CharacterName> mList = new List<CharacterName>(10);

	private static List<LastName> sLastNamePool;

	private static List<FirstName> sFirstNamePool;

	public NameGenerater()
	{
		LoadFirstName();
		LoadLastName();
	}

	private static List<LastName> GetLastName(int race, int label)
	{
		List<LastName> list = new List<LastName>(10);
		foreach (LastName item in sLastNamePool)
		{
			if (item.mRace == race && item.mLabel == label)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private static List<FirstName> GetFirstName(PeSex sex)
	{
		List<FirstName> list = new List<FirstName>(10);
		foreach (FirstName item in sFirstNamePool)
		{
			if (PeGender.Convert(item.mSex + 1) == sex)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private static void Shuffle<T>(List<T> myList)
	{
		for (int i = 0; i < myList.Count; i++)
		{
			int num = UnityEngine.Random.Range(0, myList.Count);
			if (num != i)
			{
				T value = myList[i];
				myList[i] = myList[num];
				myList[num] = value;
			}
		}
	}

	private static CharacterName GetRandomNpcName(PeSex sex, int race, List<CharacterName> exclude)
	{
		List<FirstName> firstName = GetFirstName(sex);
		if (firstName.Count <= 0)
		{
			Debug.LogError("random first name exhausted for sex:" + sex);
			return null;
		}
		Shuffle(firstName);
		foreach (FirstName item in firstName)
		{
			List<LastName> lastName = GetLastName(race, item.mLabel);
			if (lastName.Count <= 0)
			{
				Debug.LogWarning("no last name can be used by race[" + race + "], which matchs the first name [" + item.mText + "], label:" + item.mLabel);
				continue;
			}
			Shuffle(lastName);
			foreach (LastName item2 in lastName)
			{
				CharacterName characterName = new CharacterName(item.mText, item2.mText);
				if (!exclude.Exists((CharacterName name) => name.Equals(characterName) ? true : false))
				{
					return characterName;
				}
			}
		}
		Debug.LogError(string.Concat("random name exhausted for sex:", sex, " race:", race));
		return null;
	}

	private static void LoadLastName()
	{
		if (sLastNamePool == null)
		{
			sLastNamePool = new List<LastName>(20);
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Randname1");
			while (sqliteDataReader.Read())
			{
				LastName lastName = new LastName();
				lastName.mText = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Fname"));
				lastName.mRace = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("humantype")));
				lastName.mLabel = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("recognize")));
				sLastNamePool.Add(lastName);
			}
		}
	}

	private static void LoadFirstName()
	{
		if (sFirstNamePool == null)
		{
			sFirstNamePool = new List<FirstName>(20);
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("Randname2");
			while (sqliteDataReader.Read())
			{
				FirstName firstName = new FirstName();
				firstName.mSex = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sex")));
				firstName.mText = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("Rname"));
				firstName.mLabel = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("recognize")));
				sFirstNamePool.Add(firstName);
			}
		}
	}

	public CharacterName Fetch(PeSex sex, int race)
	{
		CharacterName randomNpcName = GetRandomNpcName(sex, race, mList);
		if (randomNpcName == null)
		{
			Debug.LogWarning("no random name, return default:" + CharacterName.Default.ToString());
			return CharacterName.Default;
		}
		mList.Add(randomNpcName);
		return randomNpcName;
	}

	public byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(mList.Count);
			foreach (CharacterName m in mList)
			{
				Serialize.WriteBytes(m.Export(), w);
			}
		});
	}

	public void Import(byte[] buffer)
	{
		Serialize.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				byte[] data = Serialize.ReadBytes(r);
				CharacterName characterName = new CharacterName();
				characterName.Import(data);
				mList.Add(characterName);
			}
		});
	}
}
