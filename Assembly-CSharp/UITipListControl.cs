using System.Collections.Generic;
using UnityEngine;

public class UITipListControl : MonoBehaviour
{
	private enum EPhase
	{
		none,
		keep,
		fadeIn,
		fadeOut,
		Clear
	}

	[SerializeField]
	private UISlicedSprite trumptNormSprite;

	[SerializeField]
	private UISlicedSprite trumptSelectSprite;

	[SerializeField]
	private List<UISprite> spriteList;

	[SerializeField]
	private UITipList tipList;

	public float bgDefaultAlpha = 0.5f;

	public float keepOutTime = 10f;

	public float fadeTime = 2f;

	public AnimationCurve curve;

	private float m_CurTime;

	private float m_CurKeepOutTime;

	private EPhase m_Phase;

	private EPhase m_FadeInNextPhase;

	private float m_AddMsgInterval;

	private bool m_AddMsg;

	private bool m_AwalysOn;

	public UITipList TipList => tipList;

	public void AddMsg(PeTipMsg tipMsg)
	{
		tipList.AddMsg(tipMsg);
	}

	private void Awake()
	{
		if (m_AwalysOn)
		{
			trumptNormSprite.gameObject.SetActive(value: false);
			trumptSelectSprite.gameObject.SetActive(value: true);
		}
		else
		{
			trumptNormSprite.gameObject.SetActive(value: true);
			trumptSelectSprite.gameObject.SetActive(value: false);
			m_Phase = EPhase.fadeOut;
		}
		SetAlpha(bgDefaultAlpha);
		tipList.onAddShowingMsg += OnTipListAddMsg;
	}

	private void OnDestroy()
	{
		tipList.onAddShowingMsg -= OnTipListAddMsg;
	}

	private void Update()
	{
		if (m_Phase == EPhase.fadeIn)
		{
			m_CurTime += Time.deltaTime;
			SetAlpha(Mathf.Clamp01(m_CurTime / fadeTime));
			if (m_CurTime > fadeTime)
			{
				m_Phase = m_FadeInNextPhase;
				m_CurTime = 0f;
			}
		}
		else if (m_Phase == EPhase.fadeOut)
		{
			m_CurTime += Time.deltaTime;
			SetAlpha(Mathf.Clamp01(1f - m_CurTime / fadeTime));
			if (m_CurTime > fadeTime)
			{
				m_Phase = EPhase.none;
				m_CurTime = 0f;
				tipList.ClearMsg();
			}
		}
		else if (m_Phase == EPhase.keep)
		{
			m_CurKeepOutTime += Time.deltaTime;
			m_CurTime = fadeTime;
			if (m_CurKeepOutTime > keepOutTime)
			{
				m_CurKeepOutTime = 0f;
				m_CurTime = 0f;
				m_Phase = EPhase.fadeOut;
			}
		}
		else if (m_Phase == EPhase.Clear)
		{
			m_Phase = EPhase.none;
		}
	}

	private void SetAlpha(float t)
	{
		float num = curve.Evaluate(t);
		foreach (UISprite sprite in spriteList)
		{
			sprite.alpha = num * bgDefaultAlpha;
		}
		foreach (UITipMsg msg in tipList.MsgList)
		{
			msg.content.alpha = num;
			msg.icon.alpha = num;
			msg.tex.alpha = num;
		}
	}

	private void OnTipListAddMsg()
	{
		if (!m_AwalysOn)
		{
			if (m_Phase == EPhase.fadeOut)
			{
				m_CurTime = Mathf.Max(0f, fadeTime - m_CurTime);
			}
			m_Phase = EPhase.fadeIn;
			m_FadeInNextPhase = EPhase.keep;
			m_CurKeepOutTime = 0f;
		}
	}

	private void OnTrumpetBtnClick()
	{
		if (GameUI.Instance != null && GameUI.Instance.mTipRecordsMgr != null)
		{
			GameUI.Instance.mTipRecordsMgr.ChangeWindowShowState();
		}
	}
}
