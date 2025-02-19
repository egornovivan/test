using System;
using System.Collections;
using UnityEngine;

public class UIWndTutorialTip_N : MonoBehaviour
{
	[SerializeField]
	private TweenAlpha m_HideTween;

	[SerializeField]
	private UISprite m_ContentBg;

	[SerializeField]
	private UILabel m_ContentLb;

	[SerializeField]
	private int m_ContentID = -1;

	[SerializeField]
	private float m_WaitHideTime = 5f;

	public Action DeleteEvent;

	private void Start()
	{
		m_ContentLb.text = PELocalization.GetString(m_ContentID);
		m_ContentLb.MakePixelPerfect();
		Vector3 localScale = m_ContentBg.transform.localScale;
		localScale.y = m_ContentLb.relativeSize.y * (float)m_ContentLb.font.size + 26f;
		m_ContentBg.transform.localScale = localScale;
		base.gameObject.SetActive(value: true);
		StartCoroutine("HideTutorialTipIterator");
	}

	private IEnumerator HideTutorialTipIterator()
	{
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < m_WaitHideTime)
		{
			yield return null;
		}
		m_HideTween.Play(forward: true);
		startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < m_HideTween.duration)
		{
			yield return null;
		}
		if (DeleteEvent != null)
		{
			DeleteEvent();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
