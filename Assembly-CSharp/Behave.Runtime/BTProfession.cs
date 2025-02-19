using System;
using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTProfession), "Profession")]
public class BTProfession : BTNormal
{
	private class Data
	{
		[Behave]
		public string profession = string.Empty;

		private NativeProfession m_Type;

		public NativeProfession type
		{
			get
			{
				if (m_Type == NativeProfession.None)
				{
					try
					{
						m_Type = (NativeProfession)(int)Enum.Parse(typeof(NativeProfession), profession);
					}
					catch (Exception message)
					{
						Debug.LogWarning(message);
					}
				}
				return m_Type;
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
		if (base.nativeProfession == m_Data.type)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
