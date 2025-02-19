using System.IO;
using Pathea;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("DISTANCE 3D")]
public class Distance3DCondition : Condition
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
			float lhs = Vector3.Distance(objectTransform.position, objectTransform2.position);
			return Utility.Compare(lhs, dist, comp);
		}
		if (PeGameMgr.IsMulti)
		{
			bool? flag = ReqCheck();
			if (flag.HasValue)
			{
				Debug.Log("Distance 3D result = " + flag);
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
		PlayerNetwork.RequestServer(EPacketType.PT_Custom_CheckDist3D, array);
	}
}
