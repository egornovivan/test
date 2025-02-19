using System.Collections.Generic;
using UnityEngine;

public class OptionUIHintFadeCtrl : MonoBehaviour
{
	private enum EPhase
	{
		none,
		keep,
		fadeIn,
		fadeOut,
		Clear
	}

	private static OptionUIHintFadeCtrl mInstance;

	[SerializeField]
	private OptionUIHintCtrl mHintCtrl;

	[SerializeField]
	private List<UISprite> spriteList;

	private EPhase m_Phase;

	private EPhase m_FadeInNextPhase;

	private float m_CurTime;

	private float m_CurKeepOutTime;

	public float fadeTime = 2f;

	public float bgDefaultAlpha = 0.5f;

	public float keepOutTime = 10f;

	public AnimationCurve curve;

	private bool m_AwalysOn;

	public static OptionUIHintFadeCtrl Instance => mInstance;

	public void AddOneHint(string _content)
	{
		mHintCtrl.AddOneHint(_content);
	}

	private void SetAlpha(float t)
	{
		float alpha = curve.Evaluate(t);
		foreach (OptionUIHintItem msg in mHintCtrl.MsgList)
		{
			msg.mLabel.alpha = alpha;
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

	private void Awake()
	{
		mInstance = this;
		if (!m_AwalysOn)
		{
			m_Phase = EPhase.fadeOut;
		}
		SetAlpha(bgDefaultAlpha);
		mHintCtrl.onAddShowingMsg += OnTipListAddMsg;
	}

	private void OnDestroy()
	{
		mHintCtrl.onAddShowingMsg -= OnTipListAddMsg;
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
				mHintCtrl.ClearMsg();
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
}
