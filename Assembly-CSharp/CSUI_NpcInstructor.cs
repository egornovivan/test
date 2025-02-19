using System;
using UnityEngine;

public class CSUI_NpcInstructor : MonoBehaviour
{
	[SerializeField]
	private UILabel m_InstructorLabel;

	[SerializeField]
	private UILabel m_TraineeLabel;

	private CSPersonnel m_RefNpc;

	private bool m_Active = true;

	public CSPersonnel RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc = value;
		}
	}

	public event Action<CSPersonnel> onInstructorClicked;

	public event Action<CSPersonnel> onTraineeClicked;

	public event Action onLabelChanged;

	public void Init()
	{
	}

	private void OnEnable()
	{
		if (this.onLabelChanged != null)
		{
			this.onLabelChanged();
		}
	}

	public void Activate(bool active)
	{
		if (m_Active != active)
		{
			m_Active = active;
			_activate();
		}
		else
		{
			m_Active = active;
		}
	}

	private void _activate()
	{
		if (!m_Active)
		{
		}
	}

	public void OnInstructorBtn()
	{
		if (this.onInstructorClicked != null && m_RefNpc != null)
		{
			this.onInstructorClicked(m_RefNpc);
		}
		if (this.onLabelChanged != null)
		{
			this.onLabelChanged();
		}
	}

	public void OnTraineeBtn()
	{
		if (this.onTraineeClicked != null && m_RefNpc != null)
		{
			this.onTraineeClicked(m_RefNpc);
		}
		if (this.onLabelChanged != null)
		{
			this.onLabelChanged();
		}
	}

	public void SetCountLabel(int _insCurrentCnt, int _insMaxCnt, int _traCurrentCtn, int _traMaxCnt)
	{
		if (m_InstructorLabel != null)
		{
			m_InstructorLabel.text = "[" + _insCurrentCnt + "/" + _insMaxCnt + "]";
		}
		if (m_TraineeLabel != null)
		{
			m_TraineeLabel.text = "[" + _traCurrentCtn + "/" + _traMaxCnt + "]";
		}
	}
}
