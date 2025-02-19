using UnityEngine;

public class CSUI_MyNpcItem : MonoBehaviour
{
	public Texture linshiyong;

	[HideInInspector]
	public CSUIMyNpc m_Npc;

	[SerializeField]
	private UISlicedSprite m_EmptyIcon;

	[SerializeField]
	private UITexture m_IconTex;

	[SerializeField]
	private UISlicedSprite m_IconSprite;

	[SerializeField]
	private UIButton m_DeleteButton;

	public bool m_UseDeletebutton = true;

	private void OnActivate(bool active)
	{
		if (m_UseDeletebutton && m_Npc != null)
		{
			m_DeleteButton.gameObject.SetActive(active);
		}
		else
		{
			m_DeleteButton.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (m_Npc != null)
		{
			m_EmptyIcon.enabled = false;
			if (m_Npc.RandomNpcFace != null)
			{
				m_IconTex.mainTexture = m_Npc.RandomNpcFace;
				m_IconSprite.enabled = false;
				m_IconTex.enabled = true;
			}
			if (m_Npc.RandomNpcFace == null)
			{
				m_IconTex.mainTexture = linshiyong;
				m_IconSprite.enabled = false;
				m_IconTex.enabled = true;
			}
		}
	}
}
