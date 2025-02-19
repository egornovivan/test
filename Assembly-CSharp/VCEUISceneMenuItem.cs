using UnityEngine;

public class VCEUISceneMenuItem : MonoBehaviour
{
	public VCEUISceneMenuItem m_ParentMenuItem;

	public VCESceneSetting m_SceneSetting;

	public UILabel m_Label;

	public GameObject m_PopupMenuBg;

	public UIGrid m_PopupMenuItemGroup;

	public GameObject m_PopupMenuSpriteTriangle;

	public string m_PopupMenuItemPrefabRes = "GUI/Prefabs/Scene popup menu item";

	public int m_ChildMenuCount;

	public UIButtonTween m_PopupTween;

	private bool bShouldExpand;

	private int ExpandState;

	private float ParentExpandFactor;

	private float notExpandTime;

	private float ExpandingTime;

	public static VCESceneSetting s_SceneToCreate;

	private void Start()
	{
		m_Label.text = m_SceneSetting.m_Name.ToLocalizationString();
		foreach (VCESceneSetting s_EditorScene in VCConfig.s_EditorScenes)
		{
			if (s_EditorScene.m_ParentId == m_SceneSetting.m_Id)
			{
				GameObject gameObject = Object.Instantiate(Resources.Load(m_PopupMenuItemPrefabRes) as GameObject);
				Vector3 localScale = gameObject.transform.localScale;
				gameObject.transform.parent = m_PopupMenuItemGroup.transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localScale = localScale;
				gameObject.name = "Scene " + s_EditorScene.m_Id.ToString("00");
				VCEUISceneMenuItem component = gameObject.GetComponent<VCEUISceneMenuItem>();
				component.m_SceneSetting = s_EditorScene;
				component.m_ParentMenuItem = this;
				gameObject.SetActive(value: true);
				m_ChildMenuCount++;
			}
		}
		if (m_PopupMenuBg != null)
		{
			Vector3 localScale2 = m_PopupMenuBg.transform.localScale;
			localScale2.y = m_PopupMenuItemGroup.cellHeight * (float)m_ChildMenuCount + 7f;
			m_PopupMenuBg.transform.localScale = localScale2;
		}
		m_PopupMenuItemGroup.Reposition();
		if (m_ChildMenuCount > 0)
		{
			EnablePopupMenu();
		}
		else
		{
			DisablePopupMenu();
		}
	}

	private void Update()
	{
		if (m_ParentMenuItem != null)
		{
			ParentExpandFactor = (m_ParentMenuItem.m_PopupTween.tweenTarget.transform.localScale - m_ParentMenuItem.m_PopupTween.tweenTarget.GetComponent<TweenScale>().from).magnitude;
		}
		else
		{
			ParentExpandFactor = 1f;
		}
		if (m_PopupTween.tweenTarget.transform.localScale == m_PopupTween.tweenTarget.GetComponent<TweenScale>().from)
		{
			ExpandState = 0;
		}
		else if (m_PopupTween.tweenTarget.transform.localScale == m_PopupTween.tweenTarget.GetComponent<TweenScale>().to)
		{
			ExpandState = 1;
		}
		else
		{
			ExpandState = -1;
		}
		if (ExpandingTime < 0.1f)
		{
			ExpandState = -1;
		}
		notExpandTime += Time.deltaTime;
		ExpandingTime += Time.deltaTime;
		if (UICamera.selectedObject == null)
		{
			bShouldExpand = false;
		}
		else if (notExpandTime < 0.3f)
		{
			bShouldExpand = false;
			UICamera.selectedObject = null;
		}
		else if (ExpandState != -1)
		{
			if (m_PopupTween.gameObject == UICamera.selectedObject)
			{
				bShouldExpand = true;
			}
			else
			{
				bShouldExpand = false;
			}
			VCEUISceneMenuItem[] componentsInChildren = GetComponentsInChildren<VCEUISceneMenuItem>(includeInactive: true);
			VCEUISceneMenuItem[] array = componentsInChildren;
			foreach (VCEUISceneMenuItem vCEUISceneMenuItem in array)
			{
				if (vCEUISceneMenuItem.m_PopupTween.gameObject == UICamera.selectedObject)
				{
					bShouldExpand = true;
				}
			}
		}
		if (bShouldExpand && ExpandState == 0)
		{
			m_PopupTween.Play(forward: true);
			ExpandingTime = 0f;
		}
		if (!bShouldExpand && ExpandState == 1)
		{
			m_PopupTween.Play(forward: false);
			ExpandingTime = 0f;
		}
		if (m_PopupMenuSpriteTriangle != null)
		{
			if (bShouldExpand)
			{
				m_PopupMenuSpriteTriangle.GetComponent<UISprite>().color = Color.yellow;
				m_Label.color = new Color(1f, 1f, 0f, ParentExpandFactor);
			}
			else
			{
				m_PopupMenuSpriteTriangle.GetComponent<UISprite>().color = Color.white;
				m_Label.color = new Color(1f, 1f, 1f, ParentExpandFactor);
			}
		}
	}

	public void OnBtnHover()
	{
		if (notExpandTime > 0.3f)
		{
			UICamera.selectedObject = m_PopupTween.gameObject;
		}
	}

	public void EnablePopupMenu()
	{
		if (m_PopupMenuBg != null)
		{
			m_PopupMenuBg.SetActive(value: true);
		}
		if (m_PopupMenuItemGroup != null)
		{
			m_PopupMenuItemGroup.gameObject.SetActive(value: true);
		}
		if (m_PopupMenuSpriteTriangle != null)
		{
			m_PopupMenuSpriteTriangle.SetActive(value: true);
		}
	}

	public void DisablePopupMenu()
	{
		if (m_PopupMenuBg != null)
		{
			m_PopupMenuBg.SetActive(value: false);
		}
		if (m_PopupMenuItemGroup != null)
		{
			m_PopupMenuItemGroup.gameObject.SetActive(value: false);
		}
		if (m_PopupMenuSpriteTriangle != null)
		{
			m_PopupMenuSpriteTriangle.SetActive(value: false);
		}
	}

	public static void DoCreateSceneFromMsgBox()
	{
		if (s_SceneToCreate == null)
		{
			s_SceneToCreate = VCConfig.FirstSceneSetting;
		}
		VCEditor.NewScene(s_SceneToCreate);
	}

	public void OnBtnClick()
	{
		if (m_SceneSetting.m_Category != 0)
		{
			UICamera.selectedObject = null;
			notExpandTime = 0f;
			if (VCEHistory.s_Modified)
			{
				s_SceneToCreate = m_SceneSetting;
				VCEMsgBox.Show(VCEMsgBoxType.SWITCH_QUERY);
			}
			else
			{
				VCEditor.NewScene(m_SceneSetting);
			}
		}
	}
}
