using System.IO;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("LOOK AT 2D")]
public class LookAt2DCondition : Condition
{
	private EAxis axis;

	private OBJECT obj1;

	private OBJECT obj2;

	private ECompare comp;

	private float amt;

	protected override void OnCreate()
	{
		axis = (EAxis)Utility.ToEnumInt(base.parameters["axis"]);
		obj1 = Utility.ToObject(base.parameters["object1"]);
		obj2 = Utility.ToObject(base.parameters["object2"]);
		comp = Utility.ToCompare(base.parameters["compare"]);
		amt = Utility.ToSingle(base.missionVars, base.parameters["amount"]);
	}

	public override bool? Check()
	{
		Transform objectTransform = PeScenarioUtility.GetObjectTransform(obj1);
		Transform objectTransform2 = PeScenarioUtility.GetObjectTransform(obj2);
		if (objectTransform == null || objectTransform2 == null)
		{
			return false;
		}
		Vector3 to = objectTransform2.position - objectTransform.position;
		Vector3 forward = objectTransform.forward;
		Vector3 right = objectTransform.right;
		to.y = (forward.y = (right.y = 0f));
		return axis switch
		{
			EAxis.Left => Utility.Compare(Vector3.Angle(-right, to), amt, comp), 
			EAxis.Right => Utility.Compare(Vector3.Angle(right, to), amt, comp), 
			EAxis.Down => true, 
			EAxis.Up => true, 
			EAxis.Backward => Utility.Compare(Vector3.Angle(-forward, to), amt, comp), 
			EAxis.Forward => Utility.Compare(Vector3.Angle(forward, to), amt, comp), 
			_ => false, 
		};
	}

	protected override void SendReq()
	{
		byte[] array = BufferHelper.Export(delegate(BinaryWriter w)
		{
			w.Write(reqId);
			BufferHelper.Serialize(w, obj1);
			BufferHelper.Serialize(w, obj2);
			w.Write(amt);
			w.Write((byte)axis);
			w.Write((byte)comp);
		});
		PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckLookAt2D, array);
	}
}
