using UnityEngine;

public class CounterScript : MonoBehaviour
{
	public delegate void NoParamDel();

	public delegate void TimeTickDel(float deltaTime);

	public int runCount = 1;

	public string m_Description;

	private float m_FinalCounter;

	private float m_CurCounter;

	public NoParamDel OnTimeUp;

	public TimeTickDel OnTimeTick;

	private bool m_First = true;

	private UTimer m_Timer = new UTimer();

	public float FinalCounter => m_FinalCounter;

	public float CurCounter => m_CurCounter;

	public void Init(float curCounter, float finalCounter)
	{
		m_FinalCounter = finalCounter;
		m_CurCounter = curCounter;
		m_Timer.Second = m_FinalCounter - m_CurCounter;
		m_FinalCounter = finalCounter;
		m_First = true;
	}

	public void SetFinalCounter(float finalCounter)
	{
		Init(m_CurCounter, finalCounter);
	}

	public void SetCurCounter(float curCounter)
	{
		Init(curCounter, m_FinalCounter);
	}

	public void SetRunCount(int count)
	{
		runCount = count;
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!m_First)
		{
			return;
		}
		m_Timer.ElapseSpeed = -1f;
		m_Timer.Update(Time.deltaTime);
		m_CurCounter = m_FinalCounter - (float)m_Timer.Second;
		if (m_Timer.Tick <= 0)
		{
			if (OnTimeUp != null)
			{
				OnTimeUp();
			}
			if (runCount <= 1)
			{
				Object.DestroyImmediate(this);
			}
			else
			{
				runCount--;
			}
		}
		else if (OnTimeTick != null)
		{
			OnTimeTick(Mathf.Abs(m_Timer.ElapseSpeed * Time.deltaTime));
		}
	}
}
