using System.IO;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("ROTATION")]
public class RotationCondition : Condition
{
	private EAxis axis;

	private OBJECT obj;

	private DIRRANGE range;

	protected override void OnCreate()
	{
		axis = (EAxis)Utility.ToEnumInt(base.parameters["axis"]);
		obj = Utility.ToObject(base.parameters["object"]);
		range = Utility.ToDirRange(base.missionVars, base.parameters["range"]);
	}

	public override bool? Check()
	{
		Transform objectTransform = PeScenarioUtility.GetObjectTransform(obj);
		if (objectTransform == null)
		{
			return range.type == DIRRANGE.DIRRANGETYPE.Anydirection;
		}
		return axis switch
		{
			EAxis.Left => range.Contains(-objectTransform.right), 
			EAxis.Right => range.Contains(objectTransform.right), 
			EAxis.Down => range.Contains(-objectTransform.up), 
			EAxis.Up => range.Contains(objectTransform.up), 
			EAxis.Backward => range.Contains(-objectTransform.forward), 
			EAxis.Forward => range.Contains(objectTransform.forward), 
			_ => false, 
		};
	}

	protected override void SendReq()
	{
		byte[] array = BufferHelper.Export(delegate(BinaryWriter w)
		{
			w.Write(reqId);
			BufferHelper.Serialize(w, obj);
			BufferHelper.Serialize(w, range);
			w.Write((byte)axis);
		});
		PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckRot, array);
	}
}
