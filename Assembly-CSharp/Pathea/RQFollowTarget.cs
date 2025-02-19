using System.IO;
using UnityEngine;

namespace Pathea;

public class RQFollowTarget : Request
{
	private const int Version_001 = 0;

	private const int Version_002 = 1;

	private const int Version_Current = 1;

	public int id;

	public float tgtRadiu;

	public int dirTargetID;

	public Vector3 targetPos;

	public override EReqType Type => EReqType.FollowTarget;

	public RQFollowTarget()
	{
	}

	public RQFollowTarget(params object[] objs)
	{
		id = (int)objs[0];
		targetPos = (Vector3)objs[1];
		dirTargetID = (int)objs[2];
		tgtRadiu = (float)objs[3];
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(1);
		w.Write(id);
		w.Write(targetPos.x);
		w.Write(targetPos.y);
		w.Write(targetPos.z);
		w.Write(dirTargetID);
		w.Write(tgtRadiu);
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
		id = r.ReadInt32();
		if (num >= 1)
		{
			targetPos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			dirTargetID = r.ReadInt32();
			tgtRadiu = r.ReadSingle();
		}
	}
}
