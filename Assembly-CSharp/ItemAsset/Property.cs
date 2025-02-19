using System.Collections.Generic;
using Pathea;
using SkillSystem;

namespace ItemAsset;

public class Property : Cmpt
{
	private float mFactor = 1f;

	public int buffId => base.protoData.buffId;

	public int skillId => base.protoData.skillId;

	public void SetFactor(float factor)
	{
		mFactor = factor;
	}

	public float GetProperty(AttribType property)
	{
		if (property == AttribType.ResRange)
		{
			GetRawProperty(property);
		}
		return GetRawProperty(property) * mFactor;
	}

	public float GetRawProperty(AttribType property)
	{
		return base.protoData.propertyList.GetProperty(property);
	}

	public SkInst StartSkSkill(SkEntity skEntity)
	{
		List<int> list = new List<int>(10);
		List<float> list2 = new List<float>(10);
		foreach (ItemProto.PropertyList.PropertyValue item in (IEnumerable<ItemProto.PropertyList.PropertyValue>)base.protoData.propertyList)
		{
			list.Add((int)item.type);
			list2.Add(item.value * mFactor);
		}
		return skEntity.StartSkill(skEntity, skillId, new SkUseItemPara(list, list2));
	}

	public SkBuffInst CreateSkBuff(SkEntity skEntity)
	{
		List<int> list = new List<int>(10);
		List<float> list2 = new List<float>(10);
		foreach (ItemProto.PropertyList.PropertyValue item in (IEnumerable<ItemProto.PropertyList.PropertyValue>)base.protoData.propertyList)
		{
			list.Add((int)item.type);
			list2.Add(item.value * mFactor);
		}
		return SkEntity.MountBuff(skEntity, buffId, list, list2);
	}

	public bool DestroySkBuff(SkEntity skEntity)
	{
		SkEntity.UnmountBuff(skEntity, buffId);
		return true;
	}

	public override string ProcessTooltip(string text)
	{
		foreach (ItemProto.PropertyList.PropertyValue item in (IEnumerable<ItemProto.PropertyList.PropertyValue>)base.protoData.propertyList)
		{
			string oldValue = "$" + (int)item.type + "$";
			float num = GetProperty(item.type);
			if (item.type == AttribType.ShieldMeleeProtect)
			{
				num *= 100f;
			}
			text = text.Replace(oldValue, ((int)num).ToString());
		}
		return text;
	}
}
