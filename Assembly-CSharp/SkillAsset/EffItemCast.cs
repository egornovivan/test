using System;
using ItemAsset;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

namespace SkillAsset;

public class EffItemCast
{
	internal int m_itemId;

	internal int m_skillId;

	internal int m_effId;

	internal int m_impactEffId;

	internal int m_trajectoryId;

	internal float m_speed;

	internal string m_castPosName;

	internal float m_dampRange;

	internal float m_dampValueMin;

	internal int m_castSkillId;

	internal static EffItemCast Create(SqliteDataReader reader, int id)
	{
		string @string = reader.GetString(reader.GetOrdinal("_dummy"));
		string[] array = @string.Split(',');
		if (array.Length < 9)
		{
			return null;
		}
		EffItemCast effItemCast = new EffItemCast();
		effItemCast.m_castSkillId = id;
		effItemCast.m_itemId = Convert.ToInt32(array[0]);
		effItemCast.m_skillId = Convert.ToInt32(array[1]);
		effItemCast.m_effId = Convert.ToInt32(array[2]);
		effItemCast.m_impactEffId = Convert.ToInt32(array[3]);
		effItemCast.m_trajectoryId = Convert.ToInt32(array[4]);
		effItemCast.m_speed = Convert.ToSingle(array[5]);
		effItemCast.m_dampRange = Convert.ToSingle(array[6]);
		effItemCast.m_dampValueMin = Convert.ToSingle(array[7]);
		effItemCast.m_castPosName = array[8];
		return effItemCast;
	}

	internal void Cast(SkillRunner caster, ISkillTarget target)
	{
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(m_itemId);
		if (itemProto == null)
		{
			return;
		}
		SkillRunner emitRunner = caster;
		Transform transform = ((!m_castPosName.Equals("0")) ? AiUtil.GetChild(caster.transform, m_castPosName) : caster.GetCastTransform(this));
		if (transform == null)
		{
			return;
		}
		float value = UnityEngine.Random.value;
		for (int i = 0; i < m_effId; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load(itemProto.resourcePath), transform.position, transform.rotation) as GameObject;
			Projectile component = gameObject.GetComponent<Projectile>();
			if (component != null && target != null)
			{
				AudioManager.instance.Create(transform.position, m_trajectoryId);
				Projectile projectile = caster as Projectile;
				if (projectile != null)
				{
					emitRunner = projectile;
				}
				if (m_skillId <= 0)
				{
					component.Init((byte)(i + 1), emitRunner, target, transform, value);
				}
				else
				{
					component.Init((byte)(i + 1), emitRunner, target, transform, m_skillId, value);
				}
			}
		}
	}
}
