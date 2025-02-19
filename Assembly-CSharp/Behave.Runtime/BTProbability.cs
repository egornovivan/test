using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTProbability), "Probability")]
public class BTProbability : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Random.value <= m_Data.prob)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
