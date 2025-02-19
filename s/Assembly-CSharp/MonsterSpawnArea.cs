using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnArea : MonsterSpawnPoint
{
	public class SocialSpawns
	{
		public List<MonsterSpawnPoint> spawnPoints;

		public bool isSocial;

		public MonsterSpawnPoint centerSP;

		public SocialSpawns(bool social, MonsterSpawnArea center)
		{
			isSocial = social;
			spawnPoints = new List<MonsterSpawnPoint>(10);
			centerSP = center;
		}
	}

	private const float c_Unit = 1f;

	private List<Bounds> mInnerBounds = new List<Bounds>();

	public int SpawnAmount { get; private set; }

	public int AmountPerSocial { get; private set; }

	public bool IsSocial { get; private set; }

	public Bounds[] innerBounds => mInnerBounds.ToArray();

	public MonsterSpawnArea()
	{
	}

	public MonsterSpawnArea(WEMonster mst)
		: base(mst)
	{
		SpawnAmount = mst.SpawnAmount;
		AmountPerSocial = mst.AmountPerSocial;
		IsSocial = mst.IsSocial;
		CalcInnerBounds(mst.SpawnAmount);
	}

	public MonsterSpawnArea(MonsterSpawnArea sa)
		: base(sa)
	{
		SpawnAmount = sa.SpawnAmount;
		AmountPerSocial = sa.AmountPerSocial;
		CalcInnerBounds(sa.SpawnAmount);
	}

	public void SetAmount(int amount)
	{
		SpawnAmount = amount;
		CalcInnerBounds(amount);
	}

	public bool PointIn(Vector3 pos)
	{
		Vector3 vector = Position + Quaternion.Inverse(Rotation) * (pos - Position);
		float num = Position.x - Scale.x * 0.5f;
		float num2 = Position.y - Scale.y * 0.5f;
		float num3 = Position.z - Scale.z * 0.5f;
		float num4 = Position.x + Scale.x * 0.5f;
		float num5 = Position.y + Scale.y * 0.5f;
		float num6 = Position.z + Scale.z * 0.5f;
		if (vector.x > num && vector.x < num4 && vector.y > num2 && vector.y < num5 && vector.z > num3 && vector.z < num6)
		{
			return true;
		}
		return false;
	}

	public List<MonsterSpawnPoint> RandomPoints()
	{
		List<MonsterSpawnPoint> list = new List<MonsterSpawnPoint>();
		List<Bounds> list2 = new List<Bounds>(mInnerBounds);
		for (int i = 0; i < SpawnAmount; i++)
		{
			if (list2.Count == 0)
			{
				break;
			}
			int index = Random.Range(0, list2.Count - 1);
			MonsterSpawnPoint monsterSpawnPoint = new MonsterSpawnPoint(this);
			Vector3 center = list2[index].center;
			Vector3 size = list2[index].size;
			monsterSpawnPoint.Position = new Vector3(center.x + (Random.value * 2f - 1f) * size.x * 0.3f, center.y + (Random.value * 2f - 1f) * size.y * 0.3f, center.z + (Random.value * 2f - 1f) * size.z * 0.3f);
			monsterSpawnPoint.Position = center + Rotation * (monsterSpawnPoint.Position - center);
			monsterSpawnPoint.Rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			monsterSpawnPoint.Scale = Vector3.one;
			monsterSpawnPoint.bound = list2[index];
			list.Add(monsterSpawnPoint);
			list2.RemoveAt(index);
		}
		return list;
	}

	public List<SocialSpawns> RandomPointsForSocials()
	{
		List<SocialSpawns> list = new List<SocialSpawns>();
		List<Bounds> list2 = new List<Bounds>(mInnerBounds);
		for (int num = SpawnAmount; num > 0; num -= AmountPerSocial)
		{
			if (num >= AmountPerSocial)
			{
				SocialSpawns socialSpawns = new SocialSpawns(social: true, this);
				for (int i = 0; i < AmountPerSocial; i++)
				{
					int index = Random.Range(0, list2.Count - 1);
					MonsterSpawnPoint monsterSpawnPoint = new MonsterSpawnPoint(this);
					Vector3 center = list2[index].center;
					Vector3 size = list2[index].size;
					monsterSpawnPoint.Position = new Vector3(center.x + (Random.value * 2f - 1f) * size.x * 0.3f, center.y + (Random.value * 2f - 1f) * size.y * 0.3f, center.z + (Random.value * 2f - 1f) * size.z * 0.3f);
					monsterSpawnPoint.Position = Position + Rotation * (monsterSpawnPoint.Position - Position);
					monsterSpawnPoint.Rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
					monsterSpawnPoint.Scale = Vector3.one;
					monsterSpawnPoint.bound = list2[index];
					socialSpawns.spawnPoints.Add(monsterSpawnPoint);
					list2.RemoveAt(index);
				}
				list.Add(socialSpawns);
			}
			else
			{
				SocialSpawns socialSpawns2 = new SocialSpawns(social: false, null);
				for (int j = 0; j < num; j++)
				{
					int index2 = Random.Range(0, list2.Count - 1);
					MonsterSpawnPoint monsterSpawnPoint2 = new MonsterSpawnPoint(this);
					Vector3 center2 = list2[index2].center;
					Vector3 size2 = list2[index2].size;
					monsterSpawnPoint2.Position = new Vector3(center2.x + (Random.value * 2f - 1f) * size2.x * 0.3f, center2.y + (Random.value * 2f - 1f) * size2.y * 0.3f, center2.z + (Random.value * 2f - 1f) * size2.z * 0.3f);
					monsterSpawnPoint2.Position = Position + Rotation * (monsterSpawnPoint2.Position - Position);
					monsterSpawnPoint2.Rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
					monsterSpawnPoint2.Scale = Vector3.one;
					monsterSpawnPoint2.bound = list2[index2];
					socialSpawns2.spawnPoints.Add(monsterSpawnPoint2);
					list2.RemoveAt(index2);
				}
				list.Add(socialSpawns2);
			}
		}
		return list;
	}

	private void CalcInnerBounds(int amount)
	{
		mInnerBounds.Clear();
		if (Scale.x <= Scale.y && Scale.x <= Scale.z)
		{
			int num = Mathf.Clamp(Mathf.FloorToInt(Scale.y / Mathf.Max(0.01f, Scale.x)), 1, 10);
			int num2 = 1;
			int num3 = Mathf.Clamp(Mathf.FloorToInt(Scale.z / Mathf.Max(0.01f, Scale.x)), 1, 10);
			int num4;
			for (num4 = num2 * num * num3; num4 < amount; num4 = num2 * num * num3)
			{
				num2++;
				num = Mathf.Clamp(Mathf.FloorToInt(Scale.y / Mathf.Max(0.01f, Scale.x)), 1, 10) * num2;
				num3 = Mathf.Clamp(Mathf.FloorToInt(Scale.z / Mathf.Max(0.01f, Scale.x)), 1, 10) * num2;
			}
			bool flag = num < num3;
			while (num4 >= amount * 2)
			{
				if (flag)
				{
					if (num != 1)
					{
						num = Mathf.CeilToInt((float)num * 0.5f);
					}
					else
					{
						if (num3 == 1)
						{
							break;
						}
						num3 = Mathf.CeilToInt((float)num3 * 0.5f);
					}
				}
				else if (num3 != 1)
				{
					num3 = Mathf.CeilToInt((float)num3 * 0.5f);
				}
				else
				{
					if (num == 1)
					{
						break;
					}
					num = Mathf.CeilToInt((float)num * 0.5f);
				}
				flag = !flag;
				num4 = num2 * num * num3;
			}
			_calcInnerBounds(num2, num, num3);
			return;
		}
		if (Scale.y <= Scale.z && Scale.y <= Scale.x)
		{
			int num5 = 1;
			int num6 = Mathf.Clamp(Mathf.FloorToInt(Scale.x / Mathf.Max(0.01f, Scale.y)), 1, 10);
			int num7 = Mathf.Clamp(Mathf.FloorToInt(Scale.z / Mathf.Max(0.01f, Scale.y)), 1, 10);
			int num8;
			for (num8 = num6 * num5 * num7; num8 < amount; num8 = num6 * num5 * num7)
			{
				num5++;
				num6 = Mathf.Clamp(Mathf.FloorToInt(Scale.x / Mathf.Max(0.01f, Scale.y)), 1, 10) * num5;
				num7 = Mathf.Clamp(Mathf.FloorToInt(Scale.z / Mathf.Max(0.01f, Scale.y)), 1, 10) * num5;
			}
			bool flag2 = num6 < num7;
			while (num8 >= amount * 2)
			{
				if (flag2)
				{
					if (num6 != 1)
					{
						num6 = Mathf.CeilToInt((float)num6 * 0.5f);
					}
					else
					{
						if (num7 == 1)
						{
							break;
						}
						num7 = Mathf.CeilToInt((float)num7 * 0.5f);
					}
				}
				else if (num7 != 1)
				{
					num7 = Mathf.CeilToInt((float)num7 * 0.5f);
				}
				else
				{
					if (num6 == 1)
					{
						break;
					}
					num6 = Mathf.CeilToInt((float)num6 * 0.5f);
				}
				flag2 = !flag2;
				num8 = num6 * num5 * num7;
			}
			_calcInnerBounds(num6, num5, num7);
			return;
		}
		int num9 = Mathf.Clamp(Mathf.FloorToInt(Scale.y / Mathf.Max(0.01f, Scale.z)), 1, 10);
		int num10 = Mathf.Clamp(Mathf.FloorToInt(Scale.x / Mathf.Max(0.01f, Scale.z)), 1, 10);
		int num11 = 1;
		int num12;
		for (num12 = num10 * num9 * num11; num12 < amount; num12 = num10 * num9 * num11)
		{
			num11++;
			num9 = Mathf.Clamp(Mathf.FloorToInt(Scale.y / Mathf.Max(0.01f, Scale.z)), 1, 10) * num11;
			num10 = Mathf.Clamp(Mathf.FloorToInt(Scale.x / Mathf.Max(0.01f, Scale.z)), 1, 10) * num11;
		}
		bool flag3 = num10 < num9;
		while (num12 >= amount * 2)
		{
			if (flag3)
			{
				if (num10 != 1)
				{
					num10 = Mathf.CeilToInt((float)num10 * 0.5f);
				}
				else
				{
					if (num9 == 1)
					{
						break;
					}
					num9 = Mathf.CeilToInt((float)num9 * 0.5f);
				}
			}
			else if (num9 != 1)
			{
				num9 = Mathf.CeilToInt((float)num9 * 0.5f);
			}
			else
			{
				if (num10 == 1)
				{
					break;
				}
				num10 = Mathf.CeilToInt((float)num10 * 0.5f);
			}
			flag3 = !flag3;
			num12 = num10 * num9 * num11;
		}
		_calcInnerBounds(num10, num9, num11);
	}

	private void _calcInnerBounds(int xcnt, int ycnt, int zcnt)
	{
		float num = Scale.x / (float)xcnt;
		float num2 = Scale.y / (float)ycnt;
		float num3 = Scale.z / (float)zcnt;
		float num4 = num * 0.5f;
		float num5 = num2 * 0.5f;
		float num6 = num3 * 0.5f;
		float num7 = Position.x - Scale.x * 0.5f;
		float num8 = Position.y - Scale.y * 0.5f;
		float num9 = Position.z - Scale.z * 0.5f;
		for (int i = 0; i < xcnt; i++)
		{
			for (int j = 0; j < ycnt; j++)
			{
				for (int k = 0; k < zcnt; k++)
				{
					Vector3 center = new Vector3(num7 + (float)i * num + num4, num8 + (float)j * num2 + num5, num9 + (float)k * num3 + num6);
					Bounds item = new Bounds(center, new Vector3(num, num2, num3));
					mInnerBounds.Add(item);
				}
			}
		}
	}
}
