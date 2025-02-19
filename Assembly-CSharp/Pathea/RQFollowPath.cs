using System.IO;
using UnityEngine;

namespace Pathea;

public class RQFollowPath : Request
{
	private const int Version_001 = 0;

	private const int Version_002 = 1;

	private const int Version_Current = 1;

	private Vector3[] verifPos = new Vector3[2];

	public Vector3[] path = new Vector3[0];

	public bool isLoop;

	public SpeedState speedState = SpeedState.Run;

	public Vector3[] VerifPos => verifPos;

	public override EReqType Type => EReqType.FollowPath;

	public RQFollowPath()
	{
	}

	public RQFollowPath(params object[] objs)
	{
		path = (Vector3[])objs[0];
		isLoop = (bool)objs[1];
		speedState = (SpeedState)(int)objs[2];
		if (path.Length != 0)
		{
			ref Vector3 reference = ref verifPos[0];
			reference = path[0];
			ref Vector3 reference2 = ref verifPos[1];
			reference2 = path[path.Length - 1];
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		w.Write(1);
		w.Write(path.Length);
		for (int i = 0; i < path.Length; i++)
		{
			w.Write(path[i].x);
			w.Write(path[i].y);
			w.Write(path[i].z);
		}
		w.Write(isLoop);
		w.Write((int)speedState);
	}

	public override void Deserialize(BinaryReader r)
	{
		int num = r.ReadInt32();
		int num2 = r.ReadInt32();
		path = new Vector3[num2];
		for (int i = 0; i < num2; i++)
		{
			path[i].x = r.ReadSingle();
			path[i].y = r.ReadSingle();
			path[i].z = r.ReadSingle();
		}
		isLoop = r.ReadBoolean();
		if (num >= 1)
		{
			speedState = (SpeedState)r.ReadInt32();
		}
	}

	public bool Equal(Vector3[] pos)
	{
		if (pos.Length != 2)
		{
			return false;
		}
		if (Mathf.Abs(pos[0].x - verifPos[0].x) > 2f)
		{
			return false;
		}
		if (Mathf.Abs(pos[0].y - verifPos[0].y) > 2f)
		{
			return false;
		}
		if (Mathf.Abs(pos[0].z - verifPos[0].z) > 2f)
		{
			return false;
		}
		if (Mathf.Abs(pos[1].x - verifPos[1].x) > 2f)
		{
			return false;
		}
		if (Mathf.Abs(pos[1].y - verifPos[1].y) > 2f)
		{
			return false;
		}
		if (Mathf.Abs(pos[1].z - verifPos[1].z) > 2f)
		{
			return false;
		}
		return true;
	}
}
