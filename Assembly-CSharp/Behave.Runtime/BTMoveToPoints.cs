using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTMoveToPoints), "MoveToPoints")]
public class BTMoveToPoints : BTNormal
{
	private class Data
	{
		[Behave]
		public int speed;

		[Behave]
		public string pathData;

		public Vector3[] Path;

		public bool isLoop;

		private bool mInit;

		public void Init()
		{
			if (!mInit)
			{
				List<Vector3> list = new List<Vector3>();
				string[] array = PEUtil.ToArrayString(pathData, ';');
				for (int i = 0; i < array.Length; i++)
				{
					float[] array2 = PEUtil.ToArraySingle(array[i], ',');
					list.Add(new Vector3(array2[0], array2[1], array2[2]));
				}
				Path = list.ToArray();
				mInit = true;
			}
		}

		public bool IsReached(Vector3 pos, Vector3 targetPos, float radiu = 1f)
		{
			float num = PEUtil.SqrMagnitudeH(pos, targetPos);
			return num < radiu * radiu;
		}

		public int GetClosetPointIndex(Vector3 position)
		{
			int result = -1;
			float num = float.PositiveInfinity;
			for (int i = 0; i < Path.Length; i++)
			{
				float num2 = PEUtil.SqrMagnitudeH(Path[i], position);
				if (num2 < num)
				{
					num = num2;
					result = i;
				}
			}
			return result;
		}
	}

	private Data m_Data;

	private int mIndex;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.Init();
		mIndex = m_Data.GetClosetPointIndex(base.position);
		if (mIndex < 0 || mIndex >= m_Data.Path.Length)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (m_Data.IsReached(base.position, m_Data.Path[mIndex]))
		{
			mIndex++;
			if (mIndex == m_Data.Path.Length)
			{
				return BehaveResult.Success;
			}
		}
		MoveToPosition(m_Data.Path[mIndex], (SpeedState)m_Data.speed);
		return BehaveResult.Running;
	}
}
