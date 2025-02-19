using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTOwnBlood), "OwnBlood")]
public class BTOwnBlood : BTNormal
{
	private class Data
	{
		[Behave]
		public float minHpPercent;
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return BehaveResult.Success;
		}
		float hPPercent = base.entity.HPPercent;
		if (hPPercent < m_Data.minHpPercent)
		{
			Debug.Log("OwnBlood    Success");
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
