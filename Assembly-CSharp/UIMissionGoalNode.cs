using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMissionGoalNode : MonoBehaviour
{
	public delegate void DNotitfy(UIMissionGoalNode node);

	public delegate void DBoolParamNotity(bool active, UIMissionGoalNode node);

	[SerializeField]
	private UILabel titleLb;

	[SerializeField]
	private UISprite stateSprite;

	[SerializeField]
	private UISprite selectedSprite;

	[SerializeField]
	private UIButton deleteBtn;

	[SerializeField]
	private UICheckbox trackCB;

	[SerializeField]
	private UITable chlidNodeTb;

	[SerializeField]
	private UITweener tweener;

	[SerializeField]
	private List<GameObject> m_ChildNodes = new List<GameObject>(5);

	[SerializeField]
	private bool expanded;

	[HideInInspector]
	public UIMissionGoalNode parentNode;

	public int index = -1;

	public int value0 = -1;

	public int value1 = -1;

	public int value2 = -1;

	public Action<int, GameObject> onSetChildNodeContent;

	public Color titleColor
	{
		get
		{
			return titleLb.color;
		}
		set
		{
			titleLb.color = value;
		}
	}

	public string title
	{
		get
		{
			return titleLb.text;
		}
		set
		{
			titleLb.text = value;
		}
	}

	public bool isTracked
	{
		get
		{
			return trackCB.isChecked;
		}
		set
		{
			trackCB.isChecked = value;
		}
	}

	public bool IsSelected
	{
		get
		{
			return selectedSprite.enabled;
		}
		set
		{
			selectedSprite.enabled = value;
		}
	}

	public List<GameObject> childNode => m_ChildNodes;

	public event DNotitfy onTitleClick;

	public event DNotitfy onDeleteBtnClick;

	public event DBoolParamNotity onTrackBoxActive;

	public void PlayTween(bool foward)
	{
		tweener.Play(foward);
		expanded = foward;
		CheckState();
		DetermineStateSprite();
	}

	public void SetContent(string _title, bool _canAbort, bool _canTrack)
	{
		title = _title;
		if (trackCB != null)
		{
			trackCB.gameObject.SetActive(_canTrack);
		}
		if (deleteBtn != null)
		{
			deleteBtn.gameObject.SetActive(_canAbort);
		}
	}

	public void UpdateChildNode(int count, GameObject prefab)
	{
		UIUtility.UpdateListGos(m_ChildNodes, prefab, chlidNodeTb.transform, count, OnSetChildNodeContent, null);
		CheckState();
		chlidNodeTb.repositionNow = true;
	}

	public void RemoveChildeNode(int index)
	{
		if (index >= -1 || index <= m_ChildNodes.Count)
		{
			UnityEngine.Object.Destroy(m_ChildNodes[index].gameObject);
			m_ChildNodes[index].transform.parent = null;
			m_ChildNodes.RemoveAt(index);
			CheckState();
			chlidNodeTb.repositionNow = true;
		}
	}

	public GameObject GetChildNode(int index)
	{
		return m_ChildNodes[index];
	}

	private void OnSetChildNodeContent(int index, GameObject go)
	{
		if (onSetChildNodeContent != null)
		{
			onSetChildNodeContent(index, go);
		}
	}

	private void CheckState()
	{
		stateSprite.gameObject.SetActive(m_ChildNodes.Count != 0);
	}

	private void DetermineStateSprite()
	{
		if (expanded)
		{
			stateSprite.spriteName = "mission_open";
		}
		else
		{
			stateSprite.spriteName = "mission_closed";
		}
	}

	private void Awake()
	{
		CheckState();
		DetermineStateSprite();
	}

	private void OnTitileClick()
	{
		expanded = !expanded;
		DetermineStateSprite();
		if (this.onTitleClick != null)
		{
			this.onTitleClick(this);
		}
	}

	private void OnDeleteBtnClick()
	{
		if (this.onDeleteBtnClick != null)
		{
			this.onDeleteBtnClick(this);
		}
	}

	private void OnStateClick()
	{
		expanded = !expanded;
		if (expanded)
		{
			stateSprite.spriteName = "mission_open";
		}
		else
		{
			stateSprite.spriteName = "mission_closed";
		}
		expanded = !expanded;
	}

	private void OnTrackBoxActive(bool active)
	{
		if (this.onTrackBoxActive != null)
		{
			this.onTrackBoxActive(active, this);
		}
	}
}
