using System;
using Mono.Data.SqliteClient;
using Pathea;

public class CollisionEffectMgr
{
	private static int[,] _particleEeffects;

	private static int[,] _soundEeffects;

	private static float[,] _damageScale;

	public static void LoadData()
	{
		_particleEeffects = new int[Enum.GetValues(typeof(AttackMaterial)).Length, Enum.GetValues(typeof(DefenceMaterial)).Length];
		_soundEeffects = new int[Enum.GetValues(typeof(AttackMaterial)).Length, Enum.GetValues(typeof(DefenceMaterial)).Length];
		_damageScale = new float[Enum.GetValues(typeof(AttackForm)).Length, Enum.GetValues(typeof(DefenceType)).Length];
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("CollisionParticle");
		int num = 0;
		int num2 = 0;
		while (sqliteDataReader.Read())
		{
			for (int i = num2; i < sqliteDataReader.FieldCount; i++)
			{
				_particleEeffects[num, i - num2] = Convert.ToInt32(sqliteDataReader.GetString(i));
			}
			num++;
		}
		sqliteDataReader.Close();
		sqliteDataReader = LocalDatabase.Instance.ReadFullTable("CollisionSound");
		num = 0;
		num2 = 0;
		while (sqliteDataReader.Read())
		{
			for (int j = num2; j < sqliteDataReader.FieldCount; j++)
			{
				_soundEeffects[num, j - num2] = Convert.ToInt32(sqliteDataReader.GetString(j));
			}
			num++;
		}
		sqliteDataReader.Close();
		sqliteDataReader = LocalDatabase.Instance.ReadFullTable("DmgDefScale");
		num = 0;
		num2 = 2;
		while (sqliteDataReader.Read())
		{
			for (int k = num2; k < sqliteDataReader.FieldCount; k++)
			{
				_damageScale[num, k - num2] = Convert.ToSingle(sqliteDataReader.GetString(k));
			}
			num++;
		}
		sqliteDataReader.Close();
	}

	private static int GetParticleEffectID(int casterMaterial, int hitMaterial)
	{
		return _particleEeffects[casterMaterial, hitMaterial];
	}

	public static int GetParticleEffectID(AttackMaterial casterMaterial, DefenceMaterial hitMaterial)
	{
		return GetParticleEffectID((int)casterMaterial, (int)hitMaterial);
	}

	private static int GetSoundEffectID(int casterMaterial, int hitMaterial)
	{
		return _soundEeffects[casterMaterial, hitMaterial];
	}

	public static int GetSoundEffectID(AttackMaterial casterMaterial, DefenceMaterial hitMaterial)
	{
		return GetSoundEffectID((int)casterMaterial, (int)hitMaterial);
	}

	private static float GetDamageScale(int attackForm, int defenceType)
	{
		return _damageScale[attackForm, defenceType];
	}

	public float GetDamageScale(AttackForm attackForm, DefenceType defenceType)
	{
		return GetDamageScale((int)attackForm, (int)defenceType);
	}
}
