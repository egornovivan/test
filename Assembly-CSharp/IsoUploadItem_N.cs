using System;
using System.Collections;
using UnityEngine;

public class IsoUploadItem_N : MonoBehaviour
{
	[SerializeField]
	private UILabel m_IsoNameLb;

	[SerializeField]
	private UISlider m_UploadProgres;

	[SerializeField]
	private UISprite m_ProgresForeSprite;

	[SerializeField]
	private UIButton m_DelBtn;

	[SerializeField]
	private BoxCollider m_ShowTipCollider;

	[SerializeField]
	private Color m_NormalCol;

	[SerializeField]
	private Color m_FailedCol;

	[SerializeField]
	private Color m_ComplatedCol;

	private float m_DeleteWaitTime = 5f;

	private ShowToolTipItem_N m_ShowToolTipItem;

	private bool m_ToolTipIsHover;

	private IsoUploadInfoWndCtrl.UploadState m_Step;

	public Action<int> DelEvent;

	public int ID { get; private set; }

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		UIEventListener.Get(m_DelBtn.gameObject).onClick = DelBtnClick;
		m_ShowToolTipItem = m_ShowTipCollider.gameObject.AddComponent<ShowToolTipItem_N>();
		UIEventListener.Get(m_ShowTipCollider.gameObject).onHover = ShowTipColliderOnHover;
	}

	private void DelBtnClick(GameObject go)
	{
		if (DelEvent != null)
		{
			DelEvent(ID);
			if (m_ToolTipIsHover)
			{
				UITooltip.ShowText(null);
			}
		}
	}

	private void ShowTipColliderOnHover(GameObject go, bool isHover)
	{
		m_ToolTipIsHover = isHover;
	}

	private void UpdateIsoName(string isoName)
	{
		m_IsoNameLb.text = isoName.ToString();
		m_IsoNameLb.MakePixelPerfect();
	}

	private int GetTipIDByStep(IsoUploadInfoWndCtrl.UploadState step)
	{
		return step switch
		{
			IsoUploadInfoWndCtrl.UploadState.UploadPreViewFile => 8000963, 
			IsoUploadInfoWndCtrl.UploadState.UploadIsoFile => 8000964, 
			IsoUploadInfoWndCtrl.UploadState.SharingIso => 8000965, 
			IsoUploadInfoWndCtrl.UploadState.OthePlayerDownload => 8000966, 
			IsoUploadInfoWndCtrl.UploadState.ExportComplated => 8000967, 
			IsoUploadInfoWndCtrl.UploadState.ExportFailed => 8000968, 
			IsoUploadInfoWndCtrl.UploadState.NotEnoughMaterials => 821000001, 
			_ => -1, 
		};
	}

	private void UpdateToolTip(IsoUploadInfoWndCtrl.UploadState step)
	{
		int tipIDByStep = GetTipIDByStep(step);
		if (tipIDByStep != -1)
		{
			m_ShowToolTipItem.mStrID = tipIDByStep;
			if (m_ToolTipIsHover)
			{
				UITooltip.ShowText(PELocalization.GetString(tipIDByStep));
			}
		}
	}

	private void UpdateProgres(IsoUploadInfoWndCtrl.UploadState step)
	{
		switch (step)
		{
		case IsoUploadInfoWndCtrl.UploadState.ExportFailed:
		case IsoUploadInfoWndCtrl.UploadState.NotEnoughMaterials:
			m_ProgresForeSprite.color = m_FailedCol;
			m_UploadProgres.sliderValue = 1f;
			break;
		case IsoUploadInfoWndCtrl.UploadState.ExportComplated:
			m_ProgresForeSprite.color = m_ComplatedCol;
			m_UploadProgres.sliderValue = 1f;
			break;
		case IsoUploadInfoWndCtrl.UploadState.None:
			m_ProgresForeSprite.color = m_NormalCol;
			m_UploadProgres.sliderValue = 0f;
			break;
		default:
			m_ProgresForeSprite.color = m_NormalCol;
			m_UploadProgres.sliderValue = (float)step / 5f;
			break;
		}
	}

	private void UpdateDelBtnState(IsoUploadInfoWndCtrl.UploadState step)
	{
		m_DelBtn.gameObject.SetActive(step == IsoUploadInfoWndCtrl.UploadState.ExportFailed);
	}

	private IEnumerator AutoDeleteIterator()
	{
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < m_DeleteWaitTime)
		{
			yield return null;
		}
		DelBtnClick(base.gameObject);
	}

	public void UpdateInfo(int id, string isoName, IsoUploadInfoWndCtrl.UploadState step)
	{
		ID = id;
		UpdateIsoName(isoName);
		UpdateStep(step);
	}

	public void UpdateStep(IsoUploadInfoWndCtrl.UploadState step)
	{
		m_Step = step;
		UpdateProgres(m_Step);
		UpdateToolTip(m_Step);
		if (step == IsoUploadInfoWndCtrl.UploadState.ExportComplated)
		{
			StartCoroutine(AutoDeleteIterator());
		}
	}
}
