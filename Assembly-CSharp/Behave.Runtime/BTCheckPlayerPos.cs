using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCheckPlayerPos), "CheckPlayerPos")]
public class BTCheckPlayerPos : BTNormal
{
	private class Data
	{
		[Behave]
		public float Radius;
	}

	private Data m_Data;

	private bool InRadiu(Vector3 self, Vector3 target, float radiu)
	{
		float num = PEUtil.SqrMagnitudeH(self, target);
		return num < radiu * radiu;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null && InRadiu(base.position, PeSingleton<PeCreature>.Instance.mainPlayer.position, m_Data.Radius))
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
