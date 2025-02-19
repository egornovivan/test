using UnityEngine;

public class BuildTutorialItem_N : MonoBehaviour
{
	[SerializeField]
	private TweenScale m_Tween;

	[SerializeField]
	private UISprite m_BgSprite;

	[SerializeField]
	private UILabel m_ContentLbl;

	[SerializeField]
	private int m_ContentID = -1;

	private bool m_Forword;

	public bool IsShow;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		UpdateContent();
		if (null != m_Tween)
		{
			m_Tween.onFinished = TweenFinish;
		}
	}

	private void UpdateContent()
	{
		if (m_ContentID != -1)
		{
			m_ContentLbl.text = PELocalization.GetString(m_ContentID);
			m_ContentLbl.MakePixelPerfect();
			AdjustContentBg();
		}
	}

	private void AdjustContentBg()
	{
		Vector3 localScale = m_BgSprite.transform.localScale;
		localScale.y = m_ContentLbl.relativeSize.y * (float)m_ContentLbl.font.size + 26f;
		m_BgSprite.transform.localScale = localScale;
	}

	private void TweenFinish(UITweener tween)
	{
		IsShow = m_Forword;
		base.gameObject.SetActive(IsShow);
	}

	public void ShowTween(bool show)
	{
		if (null != m_Tween)
		{
			base.gameObject.SetActive(value: true);
			m_Tween.Play(show);
			m_Forword = show;
		}
	}

	public float GetTweenTime()
	{
		return m_Tween.duration;
	}
}
