using UnityEngine;

namespace Pathea;

public class ChatTeamDb
{
	public Vector3 TRMovePos;

	public Vector3 CenterPos;

	public void Set(object[] objs)
	{
		TRMovePos = (Vector3)objs[0];
		CenterPos = (Vector3)objs[1];
	}
}
