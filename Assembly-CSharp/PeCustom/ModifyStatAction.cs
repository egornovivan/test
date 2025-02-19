using System.IO;
using Pathea;
using PETools;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("MODIFY STAT", true)]
public class ModifyStatAction : Action
{
	private OBJECT obj;

	private AttribType stat;

	private EFunc func;

	private float amt;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
		stat = (AttribType)Utility.ToEnumInt(base.parameters["stat"]);
		func = Utility.ToFunc(base.parameters["func"]);
		amt = Utility.ToSingle(base.missionVars, base.parameters["amount"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				BufferHelper.Serialize(w, obj);
				w.Write(amt);
				w.Write((byte)stat);
				w.Write((byte)func);
			});
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_ModifyStat, array);
		}
		else
		{
			PeEntity entity = PeScenarioUtility.GetEntity(obj);
			if (entity != null && entity.skEntity != null)
			{
				float attribute = entity.skEntity.GetAttribute((int)stat);
				float attrValue = Utility.Function(attribute, amt, func);
				float hPPercent = PeSingleton<PeCreature>.Instance.mainPlayer.HPPercent;
				entity.SetAttribute(stat, attrValue);
				float hPPercent2 = PeSingleton<PeCreature>.Instance.mainPlayer.HPPercent;
				float num = Mathf.Clamp01(hPPercent - hPPercent2) * PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.HpMax);
				if (num > 0f)
				{
					PeCameraImageEffect.PlayHitEffect(num);
				}
			}
		}
		return true;
	}
}
