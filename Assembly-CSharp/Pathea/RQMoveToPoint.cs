using System.IO;
using UnityEngine;

namespace Pathea;

public class RQMoveToPoint : Request
{
	private const int Version_001 = 0;

	private const int Version_Current = 0;

	public Vector3 position = Vector3.zero;

	public float stopRadius;

	public bool isForce;

	public SpeedState speedState;

	public override EReqType Type => EReqType.MoveToPoint;

	public RQMoveToPoint()
	{
	}

	public RQMoveToPoint(params object[] objs)
	{
		position = (Vector3)objs[0];
		stopRadius = (float)objs[1];
		isForce = (bool)objs[2];
		speedState = (SpeedState)(int)objs[3];
	}

	public void ReachPoint(PeEntity npc)
	{
		if (StroyManager.Instance != null)
		{
			StroyManager.Instance.EntityReach(npc, trigger: true);
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(0);
		w.Write(position.x);
		w.Write(position.y);
		w.Write(position.z);
		w.Write(stopRadius);
		w.Write(isForce);
		w.Write((int)speedState);
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
		position.x = r.ReadSingle();
		position.y = r.ReadSingle();
		position.z = r.ReadSingle();
		stopRadius = r.ReadSingle();
		isForce = r.ReadBoolean();
		speedState = (SpeedState)r.ReadInt32();
	}
}
