using System;
using PETools;
using UnityEngine;

public class MissileLockerUI : MonoBehaviour
{
	public Transform m_TargetObject;

	public Transform m_TAGroup;

	public UISprite m_TA0;

	public UISprite m_TA90;

	public UISprite m_TA180;

	public UISprite m_TA270;

	public UISprite m_Deco0;

	public UISprite m_Deco1;

	public UISprite m_ProgressFSpriteBG;

	public UIFilledSprite m_ProgressFSprite;

	public UILabel m_LockProgressText;

	public AudioClip m_LockingSound0;

	public AudioClip m_LockingSound1;

	public AudioClip m_LockedSound;

	public float m_TASpan = 20f;

	public float m_TAWidth = 16f;

	public float m_TAHeight = 16f;

	public float m_TARotSpeed = 90f;

	public float m_Deco0RotSpeed = 59f;

	public float m_Deco1RotSpeed = -83f;

	public float m_Alpha = 0.6f;

	public Color m_UnlockColor;

	public Color m_LockedColor;

	private float m_LockProgress;

	private float m_LastProgress;

	private float m_LockCompleteEffectTime;

	private bool m_LockComplete;

	private float m_Fade;

	private float m_FadeDir = 1f;

	private float ta_rot;

	public float LockProgress
	{
		get
		{
			return m_LockProgress;
		}
		set
		{
			m_LockProgress = value;
			m_ProgressFSprite.fillAmount = value;
		}
	}

	public bool Alive => m_FadeDir >= 0f;

	private void LockComplete()
	{
		m_LockComplete = true;
		m_LockCompleteEffectTime = 0f;
		NGUITools.PlaySound(m_LockedSound, 0.5f, 1.4f);
	}

	public void FadeIn()
	{
		m_FadeDir = 1f;
	}

	public void FadeOut()
	{
		m_FadeDir = -1f;
	}

	private void Start()
	{
		FadeIn();
	}

	private void Update()
	{
		UpdatePos();
		if (m_ProgressFSprite.fillAmount < 0.999f && m_ProgressFSprite.fillAmount > 0.001f)
		{
			ta_rot += Time.deltaTime * m_TARotSpeed * (3f + m_ProgressFSprite.fillAmount * 8f);
		}
		else
		{
			ta_rot += Time.deltaTime * m_TARotSpeed;
		}
		m_TAGroup.localEulerAngles = new Vector3(0f, 0f, ta_rot);
		m_Deco0.transform.localEulerAngles = new Vector3(0f, 0f, Time.time * m_Deco0RotSpeed);
		m_Deco1.transform.localEulerAngles = new Vector3(0f, 0f, Time.time * m_Deco1RotSpeed);
		m_TA0.transform.localPosition = Vector3.up * m_TASpan;
		m_TA90.transform.localPosition = Vector3.left * m_TASpan;
		m_TA180.transform.localPosition = Vector3.down * m_TASpan;
		m_TA270.transform.localPosition = Vector3.right * m_TASpan;
		Transform obj = m_TA0.transform;
		Vector3 vector = new Vector3(m_TAWidth, m_TAHeight, 1f);
		m_TA270.transform.localScale = vector;
		vector = vector;
		m_TA180.transform.localScale = vector;
		vector = vector;
		m_TA90.transform.localScale = vector;
		obj.localScale = vector;
		if (m_LockComplete)
		{
			m_LockCompleteEffectTime += Time.deltaTime;
		}
		m_Fade += m_FadeDir * Time.deltaTime * 6f;
		m_Fade = Mathf.Clamp01(m_Fade);
		if (m_Fade == 0f && m_FadeDir < 0f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (m_LastProgress <= 0.999f && m_ProgressFSprite.fillAmount > 0.999f)
		{
			LockComplete();
		}
		for (float num = 0f; num < 0.75f; num += 0.125f)
		{
			if (m_LastProgress <= num && m_ProgressFSprite.fillAmount > num)
			{
				NGUITools.PlaySound(m_LockingSound0, num * 0.2f + 0.1f, 1f + num * 0.15f);
			}
		}
		float num2 = 0.15f;
		for (float num3 = 0f; num3 < 0.98f; num3 += num2)
		{
			if (m_LastProgress <= num3 && m_ProgressFSprite.fillAmount > num3)
			{
				NGUITools.PlaySound(m_LockingSound1, 0.5f, 1f + num3 * 0.15f);
			}
			num2 *= 0.87f;
			if (num2 < 0.03f)
			{
				num2 = 0.03f;
			}
		}
		m_LastProgress = m_ProgressFSprite.fillAmount;
		float num4 = Mathf.Lerp(20f, 1f, Mathf.Pow(m_Fade, 0.4f));
		float num5 = m_Alpha;
		if (m_Fade < 1f)
		{
			num5 = Mathf.Lerp(0f, 1f, Mathf.Pow(m_Fade, 3f)) * m_Alpha * 0.5f;
		}
		base.transform.localScale = new Vector3(num4, num4, 1f);
		Color color;
		if (m_ProgressFSprite.fillAmount < 0.999f)
		{
			color = m_UnlockColor;
		}
		else
		{
			float num6 = 0.75f;
			if (m_LockCompleteEffectTime < num6)
			{
				float num7 = num6 / 5f;
				float f = m_LockCompleteEffectTime / num7 * (float)Math.PI;
				color = Color.Lerp(m_LockedColor, m_UnlockColor, Mathf.Cos(f) * 0.5f + 0.5f);
			}
			else
			{
				color = m_LockedColor;
			}
		}
		float num8 = ((!(m_ProgressFSprite.fillAmount < 0.999f)) ? Mathf.Lerp(40f, 90f, Mathf.Pow(m_LockCompleteEffectTime * 5f, 2f)) : 40f);
		float num9 = ((!(m_ProgressFSprite.fillAmount < 0.999f)) ? Mathf.Lerp(0.6f, 0f, Mathf.Pow(m_LockCompleteEffectTime * 5f, 0.4f)) : 1f);
		if (m_ProgressFSprite.fillAmount > 0.001f && m_ProgressFSprite.fillAmount < 0.999f)
		{
			m_LockProgressText.text = (m_ProgressFSprite.fillAmount * 100f).ToString("0") + " %";
		}
		else if (m_ProgressFSprite.fillAmount < 0.002f)
		{
			m_LockProgressText.text = "'X'-" + "Lockon".ToLocalizationString();
		}
		else if (m_ProgressFSprite.fillAmount > 0.998f)
		{
			if (m_LockCompleteEffectTime < 1f)
			{
				m_LockProgressText.text = "Locked on".ToLocalizationString();
			}
			else
			{
				m_LockProgressText.text = "'C'-" + "Fire".ToLocalizationString();
			}
		}
		m_TA0.color = color;
		m_TA90.color = color;
		m_TA180.color = color;
		m_TA270.color = color;
		m_Deco0.color = color;
		m_Deco1.color = color;
		m_ProgressFSpriteBG.color = color;
		m_TA0.alpha = num5;
		m_TA90.alpha = num5;
		m_TA180.alpha = num5;
		m_TA270.alpha = num5;
		m_ProgressFSpriteBG.alpha = num5 * 0.3f;
		m_ProgressFSprite.alpha = num5 * num9;
		m_Deco0.alpha = num5;
		m_Deco1.alpha = num5;
		m_LockProgressText.alpha = num5;
		m_ProgressFSprite.transform.localScale = new Vector3(num8, num8, 1f);
	}

	public void UpdatePos()
	{
		if (m_TargetObject != null && PEUtil.MainCamTransform != null)
		{
			Vector3 vector = m_TargetObject.transform.position + Vector3.up;
			if (m_TargetObject.GetComponent<Rigidbody>() != null)
			{
				vector = m_TargetObject.GetComponent<Rigidbody>().worldCenterOfMass;
			}
			if (Vector3.Dot(PEUtil.MainCamTransform.forward, (vector - PEUtil.MainCamTransform.position).normalized) > 0.2f)
			{
				base.transform.localPosition = Camera.main.WorldToScreenPoint(vector);
			}
			else
			{
				base.transform.localPosition = new Vector3(-300f, -300f, 0f);
			}
			NGUITools.MakePixelPerfect(base.transform);
		}
	}
}
