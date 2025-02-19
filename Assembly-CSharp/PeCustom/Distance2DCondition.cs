using System.IO;
using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("DISTANCE 2D")]
public class Distance2DCondition : Condition
{
	private OBJECT obj1;

	private OBJECT obj2;

	private ECompare comp;

	private float dist;

	protected override void OnCreate()
	{
		obj1 = Utility.ToObject(base.parameters["object1"]);
		obj2 = Utility.ToObject(base.parameters["object2"]);
		comp = Utility.ToCompare(base.parameters["compare"]);
		dist = Utility.ToSingle(base.missionVars, base.parameters["distance"]);
	}

	public override bool? Check()
	{
		if (PeGameMgr.IsSingle)
		{
			Transform objectTransform = PeScenarioUtility.GetObjectTransform(obj1);
			Transform objectTransform2 = PeScenarioUtility.GetObjectTransform(obj2);
			if (objectTransform == null || objectTransform2 == null)
			{
				return false;
			}
			Vector3 position = objectTransform.position;
			Vector3 position2 = objectTransform2.position;
			position.y = (position2.y = 0f);
			float lhs = Vector3.Distance(position, position2);
			return Utility.Compare(lhs, dist, comp);
		}
		if (PeGameMgr.IsMulti)
		{
			bool? flag = ReqCheck();
			if (flag.HasValue)
			{
				Debug.Log("Distance 2D result = " + flag);
			}
			return flag;
		}
		return false;
	}

	protected override void SendReq()
	{
		byte[] array = BufferHelper.Export(delegate(BinaryWriter w)
		{
			w.Write(reqId);
			BufferHelper.Serialize(w, obj1);
			BufferHelper.Serialize(w, obj2);
			w.Write((byte)comp);
			w.Write(dist);
		});
		PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckDist2D, array);
	}
}
