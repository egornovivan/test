using System;
using System.Collections;
using Pathea;
using UnityEngine;

public class UIBuildTutorialCtrl : MonoBehaviour
{
	public enum BuildOpType
	{
		MatAndShape,
		VoxelAndBlock,
		Brush,
		Select,
		Menu
	}

	[SerializeField]
	private BuildTutorialItem_N m_MatAndShapeTutorial;

	[SerializeField]
	private BuildTutorialItem_N m_VoxelAndBlockTutorial;

	[SerializeField]
	private BuildTutorialItem_N m_BrushTutorial;

	[SerializeField]
	private BuildTutorialItem_N m_SelectTutorial;

	[SerializeField]
	private BuildTutorialItem_N m_MenuTutorial;

	[SerializeField]
	private BoxCollider[] m_MatAndShapeColliders;

	[SerializeField]
	private BoxCollider[] m_VoxelAndBlockColliders;

	[SerializeField]
	private BoxCollider[] m_BrushColliders;

	[SerializeField]
	private BoxCollider[] m_SelectColliders;

	[SerializeField]
	private BoxCollider[] m_MenuColliders;

	[SerializeField]
	private float m_TweenShowTime = 2f;

	private bool m_ShowAllBuildTutorial;

	private bool m_StartShowAll;

	private BuildTutorialItem_N m_BuildTutorialItemBackup;

	private void Awake()
	{
		if (PeGameMgr.IsTutorial)
		{
			InitEvent();
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		if (m_MatAndShapeTutorial.IsShow)
		{
			m_MatAndShapeTutorial.ShowTween(show: false);
		}
		if (m_VoxelAndBlockTutorial.IsShow)
		{
			m_VoxelAndBlockTutorial.ShowTween(show: false);
		}
		if (m_BrushTutorial.IsShow)
		{
			m_BrushTutorial.ShowTween(show: false);
		}
		if (m_SelectTutorial.IsShow)
		{
			m_SelectTutorial.ShowTween(show: false);
		}
		if (m_MenuTutorial.IsShow)
		{
			m_MenuTutorial.ShowTween(show: false);
		}
		if (m_StartShowAll && !m_ShowAllBuildTutorial)
		{
			m_ShowAllBuildTutorial = true;
		}
	}

	private IEnumerator ShowAllBuildTutorialInterator()
	{
		m_ShowAllBuildTutorial = false;
		m_StartShowAll = true;
		float waitTime = 0f;
		waitTime = m_MatAndShapeTutorial.GetTweenTime() + m_TweenShowTime;
		m_MatAndShapeTutorial.ShowTween(show: true);
		yield return new WaitForSeconds(waitTime);
		m_MatAndShapeTutorial.ShowTween(show: false);
		waitTime = m_VoxelAndBlockTutorial.GetTweenTime() + m_TweenShowTime;
		m_VoxelAndBlockTutorial.ShowTween(show: true);
		yield return new WaitForSeconds(waitTime);
		m_VoxelAndBlockTutorial.ShowTween(show: false);
		waitTime = m_BrushTutorial.GetTweenTime() + m_TweenShowTime;
		m_BrushTutorial.ShowTween(show: true);
		yield return new WaitForSeconds(waitTime);
		m_BrushTutorial.ShowTween(show: false);
		waitTime = m_SelectTutorial.GetTweenTime() + m_TweenShowTime;
		m_SelectTutorial.ShowTween(show: true);
		yield return new WaitForSeconds(waitTime);
		m_SelectTutorial.ShowTween(show: false);
		waitTime = m_MenuTutorial.GetTweenTime() + m_TweenShowTime;
		m_MenuTutorial.ShowTween(show: true);
		yield return new WaitForSeconds(waitTime);
		m_MenuTutorial.ShowTween(show: false);
		yield return new WaitForSeconds(m_MenuTutorial.GetTweenTime());
		m_ShowAllBuildTutorial = true;
	}

	private BuildTutorialItem_N GetBuildTutorialItemByType(BuildOpType type)
	{
		return type switch
		{
			BuildOpType.MatAndShape => m_MatAndShapeTutorial, 
			BuildOpType.VoxelAndBlock => m_VoxelAndBlockTutorial, 
			BuildOpType.Brush => m_BrushTutorial, 
			BuildOpType.Select => m_SelectTutorial, 
			BuildOpType.Menu => m_MenuTutorial, 
			_ => null, 
		};
	}

	private void PlayerTweenByType(BuildOpType type, bool show)
	{
		if (!PeGameMgr.IsTutorial)
		{
			return;
		}
		BuildTutorialItem_N buildTutorialItemByType = GetBuildTutorialItemByType(type);
		if (!(null != buildTutorialItemByType))
		{
			return;
		}
		if (show)
		{
			if (null != m_BuildTutorialItemBackup && m_BuildTutorialItemBackup.IsShow)
			{
				m_BuildTutorialItemBackup.ShowTween(show: false);
			}
			m_BuildTutorialItemBackup = buildTutorialItemByType;
		}
		buildTutorialItemByType.ShowTween(show);
	}

	private void MatAndShapeHoverEvent(GameObject go, bool isHover)
	{
		if (m_ShowAllBuildTutorial)
		{
			PlayerTweenByType(BuildOpType.MatAndShape, isHover);
		}
	}

	private void VoxelAndBlockHoverEvent(GameObject go, bool isHover)
	{
		if (m_ShowAllBuildTutorial)
		{
			PlayerTweenByType(BuildOpType.VoxelAndBlock, isHover);
		}
	}

	private void BrushHoverEvent(GameObject go, bool isHover)
	{
		if (m_ShowAllBuildTutorial)
		{
			PlayerTweenByType(BuildOpType.Brush, isHover);
		}
	}

	private void SelectHoverEvent(GameObject go, bool isHover)
	{
		if (m_ShowAllBuildTutorial)
		{
			PlayerTweenByType(BuildOpType.Select, isHover);
		}
	}

	private void MenuHoverEvent(GameObject go, bool isHover)
	{
		if (m_ShowAllBuildTutorial)
		{
			PlayerTweenByType(BuildOpType.Menu, isHover);
		}
	}

	private void InitEvent()
	{
		if (m_MatAndShapeColliders != null && m_MatAndShapeColliders.Length > 0)
		{
			for (int i = 0; i < m_MatAndShapeColliders.Length; i++)
			{
				UIEventListener uIEventListener = UIEventListener.Get(m_MatAndShapeColliders[i].gameObject);
				uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(MatAndShapeHoverEvent));
			}
		}
		if (m_VoxelAndBlockColliders != null && m_VoxelAndBlockColliders.Length > 0)
		{
			for (int j = 0; j < m_VoxelAndBlockColliders.Length; j++)
			{
				UIEventListener uIEventListener2 = UIEventListener.Get(m_VoxelAndBlockColliders[j].gameObject);
				uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(VoxelAndBlockHoverEvent));
			}
		}
		if (m_BrushColliders != null && m_BrushColliders.Length > 0)
		{
			for (int k = 0; k < m_BrushColliders.Length; k++)
			{
				UIEventListener uIEventListener3 = UIEventListener.Get(m_BrushColliders[k].gameObject);
				uIEventListener3.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener3.onHover, new UIEventListener.BoolDelegate(BrushHoverEvent));
			}
		}
		if (m_SelectColliders != null && m_SelectColliders.Length > 0)
		{
			for (int l = 0; l < m_SelectColliders.Length; l++)
			{
				UIEventListener uIEventListener4 = UIEventListener.Get(m_SelectColliders[l].gameObject);
				uIEventListener4.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener4.onHover, new UIEventListener.BoolDelegate(SelectHoverEvent));
			}
		}
		if (m_MenuColliders != null && m_MenuColliders.Length > 0)
		{
			for (int m = 0; m < m_MenuColliders.Length; m++)
			{
				UIEventListener uIEventListener5 = UIEventListener.Get(m_MenuColliders[m].gameObject);
				uIEventListener5.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener5.onHover, new UIEventListener.BoolDelegate(MenuHoverEvent));
			}
		}
	}

	public void ShowAllBuildTutorial()
	{
		if (!m_ShowAllBuildTutorial)
		{
			StopCoroutine("ShowAllBuildTutorialInterator");
			StartCoroutine("ShowAllBuildTutorialInterator");
		}
	}
}
