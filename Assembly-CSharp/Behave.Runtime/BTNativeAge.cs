using System;
using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNativeAge), "NativeAge")]
public class BTNativeAge : BTNormal
{
	private class Data
	{
		[Behave]
		public string age = string.Empty;

		private NativeAge m_Type;

		public NativeAge type
		{
			get
			{
				if (m_Type == NativeAge.None)
				{
					try
					{
						m_Type = (NativeAge)(int)Enum.Parse(typeof(NativeAge), age);
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
		if (base.nativeAge == m_Data.type)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
