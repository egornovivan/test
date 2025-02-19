using System;
using UnityEngine;

public class VCEUICreationTypeItem : MonoBehaviour
{
	public int usage;

	[HideInInspector]
	public VCESceneSetting m_Scene;

	public VCEUIFastCreateWnd m_ParentWnd;

	public UIEventListener m_Button;

	public UILabel m_NameLabel;

	public UISprite m_Icon;

	private Vector3 offset;

	private float fade;

	private float fadeTarget = 1f;

	private void Start()
	{
		if (usage == 0)
		{
			m_NameLabel.text = m_Scene.m_Name.ToLocalizationString();
			m_Icon.spriteName = "scenes/" + m_Scene.m_Id;
		}
		else if (usage == 1)
		{
			m_Icon.spriteName = "scenes/new";
		}
		else if (usage == 2)
		{
			m_Icon.spriteName = "scenes/load";
		}
		else if (usage == 3)
		{
			m_Icon.spriteName = "scenes/template";
		}
		else if (usage == 4)
		{
			m_Icon.spriteName = "scenes/empty";
		}
		offset = base.transform.localPosition - new Vector3(-100f, 100f, 0f);
	}

	public void FadeOut()
	{
		fadeTarget = 0f;
		UIEventListener button = m_Button;
		button.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(button.onClick, new UIEventListener.VoidDelegate(m_ParentWnd.OnItemClick));
	}

	public void FadeIn()
	{
		UIEventListener button = m_Button;
		button.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(button.onClick, new UIEventListener.VoidDelegate(m_ParentWnd.OnItemClick));
		fadeTarget = 1f;
	}

	private void LateUpdate()
	{
		fade = Mathf.Lerp(fade, fadeTarget, 0.15f);
		if (fadeTarget == 0f && fade < 0.005f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (fade > 0.995f && fade != 1f)
		{
			fade = 1f;
			m_NameLabel.alpha = fade * fade;
			m_Icon.alpha = fade * fade;
		}
		if (fade != 1f)
		{
			if (fadeTarget == 0f)
			{
				m_Button.GetComponent<UISlicedSprite>().alpha = 0f;
			}
			m_NameLabel.alpha = fade * fade;
			m_Icon.alpha = fade * fade;
			if (fadeTarget == 0f)
			{
				base.transform.localPosition = new Vector3(-100f, 100f, 0f) + offset * (1.5f - fade * 0.5f);
			}
		}
	}
}
