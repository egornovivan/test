using System;
using System.Collections.Generic;
using HelpExtension;
using Pathea;
using UnityEngine;

public class UISkillWndCtrl : UIBaseWnd
{
	private enum TableState
	{
		tb_None,
		tb_Work,
		tb_Live,
		tb_Fight
	}

	public delegate void DNotify(UISkillWndCtrl uiSkill);

	[SerializeField]
	private GameObject mSkillTypePrefab;

	[SerializeField]
	private Transform mContent;

	[SerializeField]
	private UIScrollBar mScrollBar;

	[SerializeField]
	private UILabel mLbExp;

	[SerializeField]
	private UISlider mSliderExp;

	[SerializeField]
	private UISkillTypeCtrl mSkillTypeCtrl;

	[SerializeField]
	private UILabel mInfo;

	private List<UISkillType> mSkillTypeList = new List<UISkillType>();

	private List<UISkillType.SkillTypeData> mSkillTypeDatas = new List<UISkillType.SkillTypeData>();

	private UISkillType mActiveSkillType;

	private TableState mTableState;

	private SkillTreeUnitMgr skillMgr;

	private SkAliveEntity skEntiyt;

	private float _TipCooldown = 3f;

	private float _curTipCooldown = 1f;

	private UISkillItem _prevLernItem;

	public SkillTreeUnitMgr _SkillMgr
	{
		get
		{
			if (skillMgr == null)
			{
				skillMgr = ((PeSingleton<PeCreature>.Instance != null && !(null == PeSingleton<PeCreature>.Instance.mainPlayer)) ? PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<SkillTreeUnitMgr>() : null);
			}
			return skillMgr;
		}
	}

	private int _SkillExp
	{
		get
		{
			if (skEntiyt == null)
			{
				return 0;
			}
			return (int)skEntiyt.GetAttribute(AttribType.Exp);
		}
	}

	private int _SkillExpMax
	{
		get
		{
			if (skEntiyt == null)
			{
				return 0;
			}
			return (int)skEntiyt.GetAttribute(AttribType.ExpMax);
		}
	}

	private int pageIndex => (int)mTableState;

	public event DNotify onRefreshTypeData;

	protected override void InitWindow()
	{
		base.InitWindow();
		skillMgr = GameUI.Instance.mMainPlayer.GetCmpt<SkillTreeUnitMgr>();
		skEntiyt = GameUI.Instance.mMainPlayer.GetCmpt<SkAliveEntity>();
		Refresh(TableState.tb_Work);
		SkillTreeInfo.SetUICallBack(RefreshTypeData);
		Invoke("ResetScrollValue", 0.2f);
		mSkillTypeCtrl.onSetBtnActive += OnSkillTypeBtnActive;
		mSkillTypeCtrl.SetActiveBtn(0);
	}

	private void Refresh(TableState state)
	{
		if (mTableState != state)
		{
			mTableState = state;
			if (SkillTreeInfo.SkillMainTypeInfo.ContainsKey(pageIndex))
			{
				mSkillTypeDatas.Clear();
				for (int i = 0; i < SkillTreeInfo.SkillMainTypeInfo[pageIndex].Count; i++)
				{
					int main_type = SkillTreeInfo.SkillMainTypeInfo[pageIndex][i]._mainType;
					SkillMainType skillMainType = SkillTreeInfo.SkillMainTypeInfo[pageIndex].Find((SkillMainType itr) => itr._mainType == main_type);
					UISkillType.SkillTypeData item = new UISkillType.SkillTypeData(SkillTreeInfo.GetUIShowList(skillMainType._mainType, skillMgr), skillMainType);
					mSkillTypeDatas.Add(item);
				}
				mSkillTypeCtrl.SetContent(SkillTreeInfo.SkillMainTypeInfo[pageIndex].Count, OnSetSkillTypeBtnContent);
				mSkillTypeCtrl.SetActiveBtn(0);
			}
		}
		UpdateSkillTypePos();
	}

	private void OnSetSkillTypeBtnContent(UISkillTypeBtn btn)
	{
		btn.spriteName = mSkillTypeDatas[btn.index].info._icon[0];
	}

	private void RefreshTypeData(int maintype)
	{
		int num = mSkillTypeDatas.FindIndex((UISkillType.SkillTypeData itr) => itr.info._mainType == maintype);
		if (num != -1)
		{
			mSkillTypeDatas[num].data = SkillTreeInfo.GetUIShowList(maintype, skillMgr);
		}
		if (mActiveSkillType != null)
		{
			SkillMainType skillMainType = SkillTreeInfo.SkillMainTypeInfo[pageIndex].Find((SkillMainType itr) => itr._mainType == mActiveSkillType.data.info._mainType);
			if (skillMainType != null)
			{
				UISkillType.SkillTypeData data = new UISkillType.SkillTypeData(SkillTreeInfo.GetUIShowList(skillMainType._mainType, skillMgr), skillMainType);
				mActiveSkillType.data = data;
			}
		}
		if (this.onRefreshTypeData != null)
		{
			this.onRefreshTypeData(this);
		}
	}

	private void CreateSkillTypes()
	{
		if (SkillTreeInfo.SkillMainTypeInfo.ContainsKey(pageIndex))
		{
			ClearSkillTypes();
			for (int i = 0; i < SkillTreeInfo.SkillMainTypeInfo[pageIndex].Count; i++)
			{
				UISkillType uISkillType = InstantiateSkillType();
				uISkillType.mainType = SkillTreeInfo.SkillMainTypeInfo[pageIndex][i]._mainType;
				mSkillTypeList.Add(uISkillType);
			}
			ResetScrollValue();
		}
	}

	private void ResetScrollValue()
	{
		mScrollBar.scrollValue = 0f;
	}

	private void UpdateSkillTypePos()
	{
		if (mActiveSkillType != null)
		{
			Vector3 localPosition = mActiveSkillType.transform.localPosition;
			localPosition.y = (0f - mActiveSkillType.height) / 2f;
			mActiveSkillType.transform.localPosition = localPosition;
		}
	}

	private void ClearSkillTypes()
	{
		foreach (UISkillType mSkillType in mSkillTypeList)
		{
			UnityEngine.Object.Destroy(mSkillType.gameObject);
			mSkillType.transform.parent = null;
		}
		mSkillTypeList.Clear();
	}

	private UISkillType InstantiateSkillType()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(mSkillTypePrefab);
		gameObject.transform.parent = mContent;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		UISkillType component = gameObject.GetComponent<UISkillType>();
		component.onSkillItemLernBtn += OnSkillItemLernClick;
		return component;
	}

	private void RefreshData()
	{
		if (!SkillTreeInfo.SkillMainTypeInfo.ContainsKey(pageIndex) || skillMgr == null)
		{
			return;
		}
		UISkillType uiType;
		foreach (UISkillType mSkillType in mSkillTypeList)
		{
			uiType = mSkillType;
			SkillMainType skillMainType = SkillTreeInfo.SkillMainTypeInfo[pageIndex].Find((SkillMainType itr) => itr._mainType == uiType.mainType);
			if (skillMainType != null)
			{
				UISkillType.SkillTypeData data = new UISkillType.SkillTypeData(SkillTreeInfo.GetUIShowList(skillMainType._mainType, skillMgr), skillMainType);
				uiType.data = data;
			}
		}
	}

	private void BtnLiveOnClick()
	{
		Refresh(TableState.tb_Live);
	}

	private void BtnFightOnClick()
	{
		Refresh(TableState.tb_Fight);
	}

	private void BtnWorkOnClick()
	{
		Refresh(TableState.tb_Work);
	}

	private void OnSkillItemLernClick(UISkillItem item)
	{
		SKTLearnResult sKTLearnResult = _SkillMgr.SKTLearn(item.data._skillType);
		if (sKTLearnResult == SKTLearnResult.SKTLearnResult_DontHaveEnoughExp && _curTipCooldown >= _TipCooldown)
		{
			new PeTipMsg(PELocalization.GetString(8000159), PeTipMsg.EMsgLevel.Warning);
			_curTipCooldown = 0f;
		}
	}

	private void OnSkillItemSelectChanged(UISkillItem item)
	{
		if (item != null)
		{
			try
			{
				string desc = item.data._desc.Substring(item.data._desc.LastIndexOf("\\n") + 2) + "\r\n" + PELocalization.GetString(8000160) + "[5CB0FF]" + GameUI.Instance.mSkillWndCtrl._SkillMgr.GetNextExpBySkillType(item.data._skillType) + "[-]";
				mSkillTypeCtrl.desc = desc;
				return;
			}
			catch
			{
				mSkillTypeCtrl.desc = string.Empty;
				return;
			}
		}
		mSkillTypeCtrl.desc = string.Empty;
	}

	private void OnSkillTypeBtnActive(UISkillTypeBtn btn)
	{
		if (mActiveSkillType == null)
		{
			GameObject gameObject = mSkillTypePrefab.CreateNew(mContent);
			UISkillType component = gameObject.GetComponent<UISkillType>();
			component.onSkillItemLernBtn += OnSkillItemLernClick;
			component.onSelectItemChanged += OnSkillItemSelectChanged;
			mActiveSkillType = component;
		}
		mActiveSkillType.data = mSkillTypeDatas[btn.index];
		mSkillTypeCtrl.desc = mActiveSkillType.data.info._desc;
		UpdateSkillTypePos();
		if (mActiveSkillType != null)
		{
			mActiveSkillType.selectItem = null;
		}
	}

	private void Update()
	{
		mLbExp.text = _SkillExp + "/" + _SkillExpMax;
		mSliderExp.sliderValue = ((_SkillExpMax != 0) ? (Convert.ToSingle(_SkillExp) / Convert.ToSingle(_SkillExpMax)) : 0f);
		if (_curTipCooldown < _TipCooldown)
		{
			_curTipCooldown += Time.deltaTime;
		}
	}

	private void OnGUI()
	{
		if (Application.isEditor && GUI.Button(new Rect(150f, 50f, 80f, 50f), "Max Exp"))
		{
			skEntiyt.SetAttribute(AttribType.Exp, _SkillExpMax);
		}
	}

	private void OnHelpBtnClick()
	{
		if (null != GameUI.Instance && null != GameUI.Instance.mPhoneWnd)
		{
			GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Help);
			GameUI.Instance.mPhoneWnd.mUIHelp.ChangeSelect(TutorialData.SkillIDs[0]);
		}
	}
}
