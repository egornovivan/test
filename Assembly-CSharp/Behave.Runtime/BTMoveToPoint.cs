using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTMoveToPoint), "MoveToPoint")]
public class BTMoveToPoint : BTNormal
{
	private class Data
	{
		[Behave]
		public int speed;

		[Behave]
		public string point;

		public Vector3 vector;

		private bool mInit;

		public void Init()
		{
			if (!mInit)
			{
				float[] array = PEUtil.ToArraySingle(point, ',');
				vector = new Vector3(array[0], array[1], array[2]);
				mInit = true;
			}
		}

		public bool IsReached(Vector3 pos, Vector3 targetPos, float radiu = 1f)
		{
			float num = PEUtil.SqrMagnitudeH(pos, targetPos);
			return num < radiu * radiu;
		}
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.Init();
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (m_Data.IsReached(base.position, m_Data.vector))
		{
			return BehaveResult.Success;
		}
		MoveToPosition(m_Data.vector, (SpeedState)m_Data.speed);
		return BehaveResult.Running;
	}
}
