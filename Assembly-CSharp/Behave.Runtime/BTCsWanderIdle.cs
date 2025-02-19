using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCsWanderIdle), "CsWanderIdle")]
public class BTCsWanderIdle : BTNormal
{
	private class Data
	{
		[Behave]
		public float WanderTime;

		public Vector3 mWanderWalkPos;

		public float startWanderTime;

		public Vector3 mDirPos;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Running;
	}
}
