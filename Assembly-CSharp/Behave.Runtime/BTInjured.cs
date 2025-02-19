namespace Behave.Runtime;

[BehaveAction(typeof(BTInjured), "Injured")]
public class BTInjured : BTNormal
{
	private class Data
	{
		[Behave]
		public float cancelHpPercent;

		[Behave]
		public string anim = string.Empty;
	}

	private Data m_Data;

	private bool m_End;

	private bool m_Run;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.entity.IsSeriousInjury)
		{
			return BehaveResult.Failure;
		}
		m_Run = true;
		m_End = false;
		SetBool(m_Data.anim, value: true);
		StartSkill(base.entity, 30100326);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!m_End)
		{
			if (base.HpPercent > m_Data.cancelHpPercent)
			{
				m_End = true;
				base.entity.IsSeriousInjury = false;
				SetBool(m_Data.anim, value: false);
				return BehaveResult.Running;
			}
		}
		else if (!GetBool("BehaveWaiting"))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Run)
		{
			StopSkill(30100326);
			m_Run = false;
		}
	}
}
