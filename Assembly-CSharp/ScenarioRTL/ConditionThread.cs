namespace ScenarioRTL;

public class ConditionThread
{
	private Trigger trigger;

	private ConditionThread()
	{
	}

	public static ConditionThread Create(Trigger trigger)
	{
		ConditionThread conditionThread = new ConditionThread();
		conditionThread.trigger = trigger;
		bool? flag = conditionThread.Check();
		if (flag == true)
		{
			conditionThread.Pass();
		}
		else if (flag == false)
		{
			conditionThread.Fail();
		}
		else
		{
			conditionThread.Register();
		}
		return conditionThread;
	}

	private void Register()
	{
		trigger.mission.scenario.AddConditionThread(this);
	}

	public bool? Check()
	{
		for (int i = 0; i < trigger.m_Conditions.Count; i++)
		{
			bool? flag = _checkConditions(trigger.m_Conditions[i]);
			if (flag == true)
			{
				return true;
			}
			if (!flag.HasValue)
			{
				return null;
			}
		}
		return false;
	}

	private bool? _checkConditions(Condition[] conditions)
	{
		for (int i = 0; i < conditions.Length; i++)
		{
			if (conditions[i] == null)
			{
				return false;
			}
			bool? flag = conditions[i].Check();
			if (!flag.HasValue)
			{
				return null;
			}
			if (flag == false)
			{
				return false;
			}
		}
		return true;
	}

	public void Pass()
	{
		if (trigger.isAlive && (trigger.threadCounter == 0 || trigger.multiThreaded))
		{
			trigger.StartProcessAction();
		}
		trigger = null;
	}

	public void Fail()
	{
		trigger = null;
	}
}
