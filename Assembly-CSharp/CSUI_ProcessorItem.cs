using System;
using UnityEngine;

public class CSUI_ProcessorItem : MonoBehaviour
{
	public delegate void JionEvent(object sender, int index);

	public delegate void DoubleClickEvent(GameObject go, int index);

	[SerializeField]
	private UILabel mTimeLb;

	[SerializeField]
	private GameObject mSelcet;

	[SerializeField]
	private UILabel mProcessorNumLb;

	private ProcessorInfo m_info;

	private int m_ProcessorNum;

	private UICheckbox m_CheckBox;

	public ProcessorInfo Info
	{
		set
		{
			m_info = value;
			if (m_info != null)
			{
				InitWnd();
			}
		}
	}

	public int ProcessorNum
	{
		get
		{
			return m_ProcessorNum;
		}
		set
		{
			m_ProcessorNum = value;
		}
	}

	public bool isChecked
	{
		get
		{
			if (m_CheckBox == null)
			{
				m_CheckBox = GetComponent<UICheckbox>();
			}
			return m_CheckBox.isChecked;
		}
		set
		{
			if (m_CheckBox == null)
			{
				m_CheckBox = GetComponent<UICheckbox>();
			}
			m_CheckBox.isChecked = value;
		}
	}

	public event JionEvent e_JionEvent;

	public event DoubleClickEvent e_DoubleClickEvent;

	private void Awake()
	{
		m_ProcessorNum = Convert.ToInt32(mProcessorNumLb.text);
		UIEventListener.Get(base.gameObject).onDoubleClick = delegate(GameObject go)
		{
			OnDoubleClickEvent(go);
		};
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void InitWnd()
	{
		SetTime(m_info.Times);
		JionProcrssor(m_info.Jioned);
	}

	public void SetTime(float mTimes)
	{
		int num = (int)mTimes;
		int num2 = num / 60 / 60 % 24;
		int num3 = num / 60 % 60;
		int num4 = num % 60;
		mTimeLb.gameObject.SetActive(value: true);
		mTimeLb.text = num2 + ":" + num3 + ":" + num4;
	}

	public void JionProcrssor(bool jion)
	{
		mSelcet.gameObject.SetActive(jion);
	}

	public void SetProcessorNum(int index)
	{
		mProcessorNumLb.gameObject.SetActive(value: true);
		mProcessorNumLb.text = index.ToString();
		m_ProcessorNum = index;
	}

	private void OnJionActivate(bool active)
	{
		if (active && this.e_JionEvent != null)
		{
			this.e_JionEvent(this, m_ProcessorNum);
		}
	}

	private void OnDoubleClickEvent(GameObject go)
	{
		if (this.e_DoubleClickEvent != null)
		{
			this.e_DoubleClickEvent(go, m_ProcessorNum);
		}
	}
}
