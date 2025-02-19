using System;
using UnityEngine;

public class ClickBtnInGameAidCmpt : MonoBehaviour
{
	[SerializeField]
	private int m_BtnID;

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (m_BtnID != 0)
			{
				InGameAidData.CheckClickBtn(m_BtnID);
			}
		});
	}

	public void SetBtnID(int id)
	{
		m_BtnID = id;
	}
}
