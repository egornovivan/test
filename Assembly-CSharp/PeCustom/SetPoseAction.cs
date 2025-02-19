using Pathea;
using PETools;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("SET POSE", true)]
public class SetPoseAction : Action
{
	private const int radius = 2;

	private OBJECT obj;

	private EFunc func_pos;

	private Vector3 pos;

	private EFunc func_rot;

	private Vector3 rot;

	protected override void OnCreate()
	{
		obj = Utility.ToObject(base.parameters["object"]);
		func_pos = Utility.ToFunc(base.parameters["funcp"]);
		pos = Utility.ToVector(base.missionVars, base.parameters["point"]);
		func_rot = Utility.ToFunc(base.parameters["funcr"]);
		rot = Utility.ToVector(base.missionVars, base.parameters["euler"]);
	}

	public override bool Logic()
	{
		PeEntity entity = PeScenarioUtility.GetEntity(obj);
		if (entity != null && entity.peTrans != null)
		{
			if (pos != Vector3.zero)
			{
				if (func_pos == EFunc.Plus || func_pos == EFunc.Minus)
				{
					entity.peTrans.position = LocalToWorld(entity.peTrans, pos, func_pos);
				}
				else
				{
					entity.peTrans.position = LocalToWorld(entity.peTrans, pos, EFunc.SetTo);
				}
			}
			if (rot != Vector3.zero)
			{
				Quaternion identity = Quaternion.identity;
				identity.eulerAngles = Calculate(entity.peTrans.rotation.eulerAngles, rot, func_rot);
				entity.peTrans.rotation = identity;
			}
		}
		return true;
	}

	private Vector3 LocalToWorld(PeTrans trans, Vector3 local, EFunc func)
	{
		Vector3 vector = Vector3.Cross(Vector3.up, trans.forward) * pos.x + Vector3.up * pos.y + trans.forward * pos.z;
		switch (func)
		{
		case EFunc.Plus:
			vector = PEUtil.GetRandomPositionOnGround(trans.position + vector, 0f, 2f);
			break;
		case EFunc.Minus:
			vector = PEUtil.GetRandomPositionOnGround(trans.position - vector, 0f, 2f);
			break;
		case EFunc.SetTo:
			vector = local;
			break;
		}
		return vector;
	}

	private Vector3 Calculate(Vector3 lhs, Vector3 rhs, EFunc func)
	{
		return func switch
		{
			EFunc.Plus => lhs + rhs, 
			EFunc.Minus => lhs - rhs, 
			_ => rhs, 
		};
	}
}
