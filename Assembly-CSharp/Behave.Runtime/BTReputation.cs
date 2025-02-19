using System;
using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTReputation), "Reputation")]
public class BTReputation : BTNormal
{
	private class Data
	{
		[Behave]
		public string type = string.Empty;

		[Behave]
		public string minReputation = string.Empty;

		[Behave]
		public string maxReputation = string.Empty;

		public ReputationSystem.ReputationLevel minType;

		public ReputationSystem.ReputationLevel maxType;

		private bool m_Init;

		public void Init()
		{
			if (!m_Init)
			{
				m_Init = true;
				minType = (ReputationSystem.ReputationLevel)(int)Enum.Parse(typeof(ReputationSystem.ReputationLevel), minReputation);
				maxType = (ReputationSystem.ReputationLevel)(int)Enum.Parse(typeof(ReputationSystem.ReputationLevel), maxReputation);
			}
		}
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		PeEntity reputation = base.entity.GetReputation(m_Data.minType, m_Data.maxType);
		if (reputation != null)
		{
			if (m_Data.type == "afraid")
			{
				base.entity.Afraid = reputation;
			}
			else if (m_Data.type == "doubt")
			{
				base.entity.Doubt = reputation;
			}
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
