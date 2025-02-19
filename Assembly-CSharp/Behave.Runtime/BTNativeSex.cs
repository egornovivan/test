using System;
using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNativeSex), "NativeSex")]
public class BTNativeSex : BTNormal
{
	private class Data
	{
		[Behave]
		public string sex = string.Empty;

		private NativeSex m_Type;

		public NativeSex type
		{
			get
			{
				if (m_Type == NativeSex.None)
				{
					try
					{
						m_Type = (NativeSex)(int)Enum.Parse(typeof(NativeSex), sex);
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
		if (base.nativeSex == m_Data.type)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
