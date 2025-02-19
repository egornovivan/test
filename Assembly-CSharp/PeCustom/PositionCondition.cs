using System.IO;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("POSITION")]
public class PositionCondition : Condition
{
	private OBJECT obj;

	private RANGE range;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
		range = Utility.ToRange(base.missionVars, base.parameters["range"]);
	}

	public override bool? Check()
	{
		Transform objectTransform = PeScenarioUtility.GetObjectTransform(obj);
		if (objectTransform == null)
		{
			return range.type == RANGE.RANGETYPE.Anywhere;
		}
		return range.Contains(objectTransform.position);
	}

	protected override void SendReq()
	{
		byte[] array = BufferHelper.Export(delegate(BinaryWriter w)
		{
			w.Write(reqId);
			BufferHelper.Serialize(w, obj);
			BufferHelper.Serialize(w, range);
		});
		PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckPos, array);
	}
}
