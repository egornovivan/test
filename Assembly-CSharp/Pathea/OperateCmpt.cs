using Pathea.Operate;

namespace Pathea;

public class OperateCmpt : PeCmpt, IOperator
{
	private IOperation m_Operate;

	private MotionMgrCmpt m_Motion;

	public bool HasOperate => m_Operate != null && !m_Operate.Equals(null);

	public IOperation Operate
	{
		get
		{
			return m_Operate;
		}
		set
		{
			m_Operate = value;
		}
	}

	public override void Start()
	{
		base.Start();
		m_Motion = GetComponent<MotionMgrCmpt>();
	}

	public bool IsActionRunning(PEActionType type)
	{
		return m_Motion != null && m_Motion.IsActionRunning(type);
	}

	public bool DoAction(PEActionType type, PEActionParam para)
	{
		if (m_Motion != null)
		{
			return m_Motion.DoAction(type, para);
		}
		return false;
	}

	public void EndAction(PEActionType type)
	{
		if (m_Motion != null)
		{
			m_Motion.EndAction(type);
		}
	}
}
