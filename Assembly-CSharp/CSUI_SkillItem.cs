using System;
using Pathea;
using UnityEngine;

public class CSUI_SkillItem : MonoBehaviour
{
	public delegate void SkillGridEvent(CSUI_SkillItem skillGrid);

	[SerializeField]
	private UISlicedSprite skillIcon;

	[SerializeField]
	private GameObject deleteBtn;

	[HideInInspector]
	public int m_Index = -1;

	[HideInInspector]
	public bool _ableToClick = true;

	private bool _active;

	private NpcAbility _localSkill;

	public SkillGridEvent OnDestroySelf;

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
			if (value)
			{
				deleteBtn.SetActive(value);
			}
		}
	}

	public event Action<NpcAbility> onLeftMouseClicked;

	public void SetSkill(string _icon, NpcAbility _skill = null)
	{
		_localSkill = _skill;
		skillIcon.spriteName = _icon;
	}

	public void SetIcon(string _name)
	{
		skillIcon.spriteName = _name;
	}

	public void DeleteIcon(NpcAbility _skill = null)
	{
		_localSkill = _skill;
		skillIcon.spriteName = "Null";
	}

	private void OnDeleteBtn()
	{
		if (OnDestroySelf != null)
		{
			OnDestroySelf(this);
		}
		OnHideBtn();
	}

	public void OnHideBtn()
	{
		deleteBtn.SetActive(value: false);
	}

	private void OnShowBtn()
	{
		CSUI_TrainMgr.Instance.HideAllDeleteBtn();
		if (skillIcon.spriteName != "Null" && _ableToClick)
		{
			deleteBtn.SetActive(value: true);
		}
	}

	private void Awake()
	{
		skillIcon.spriteName = "Null";
		deleteBtn.SetActive(value: false);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnClick()
	{
		if (Input.GetMouseButtonUp(0) && this.onLeftMouseClicked != null)
		{
			this.onLeftMouseClicked(_localSkill);
		}
	}

	private void OnTooltip(bool show)
	{
		if (show && skillIcon.spriteName != "Null")
		{
			string @string = PELocalization.GetString(_localSkill.desc);
			ToolTipsMgr.ShowText(@string);
		}
		else
		{
			ToolTipsMgr.ShowText(null);
		}
	}
}
