public class Istat
{
	public int m_StatValue;

	public int m_Goal;

	public virtual bool IsAccomplish()
	{
		return false;
	}

	public virtual void IncreaseStat()
	{
	}

	public virtual void DropStat()
	{
	}

	public virtual void SetGoal(object obj)
	{
	}

	public virtual void ResetValue()
	{
	}
}
