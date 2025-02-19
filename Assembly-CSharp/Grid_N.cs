using System;
using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using Pathea;
using PeUIEffect;
using SkillSystem;
using UnityEngine;

public class Grid_N : MonoBehaviour
{
	public delegate void GridDelegate(Grid_N grid);

	public delegate void GridsExchangeDel(Grid_N grid, ItemObject item);

	public static Grid_N mActiveGrid;

	public UITexture mItemTex;

	public UISprite mItemspr;

	public UISprite mDragMark;

	public UISprite mNewMark;

	public UISprite mForbiden;

	public UILabel mNumCount;

	public UISprite mScript;

	public UISprite mScriptIco;

	public UIFilledSprite mSkillCooldown;

	public UIFilledSprite mSkillCd;

	public UISprite mDurability;

	public UISprite mDurabilitySpecial;

	public UILabel mPowerPercent;

	public bool MustNot;

	public UISprite mMustNotSpr;

	private ItemSample mItemGrid;

	private static List<int> m_CommonAttributeIDs = new List<int> { 8000806, 8000804, 8000805 };

	public bool mShowNum = true;

	private ItemPlaceType mItemPlace;

	private int mItemIndex;

	private GridMask mGridMask = GridMask.GM_Any;

	private int mSkillID;

	private string mSkillDes;

	private bool mIsSkill;

	private SkEntity mSkenity;

	public bool mSampleMode = true;

	public GridDelegate onDropItem;

	public GridDelegate onRemoveOriginItem;

	public GridDelegate onLeftMouseClicked;

	public GridDelegate onRightMouseClicked;

	public GridsExchangeDel onGridsExchangeItem;

	public GridDelegate onDragItem;

	[SerializeField]
	private GameObject UIGridEffectPrefab;

	private UIGridEffect effect;

	private bool moveIn;

	private float moveInTime;

	private bool isOnce;

	public ItemSample Item => mItemGrid;

	public ItemObject ItemObj => mItemGrid as ItemObject;

	public ItemPlaceType ItemPlace => mItemPlace;

	public int ItemIndex
	{
		get
		{
			return mItemIndex;
		}
		set
		{
			mItemIndex = value;
		}
	}

	public GridMask ItemMask => mGridMask;

	public int SkillID => mSkillID;

	public static void SetActiveGrid(Grid_N activeG)
	{
		if (null != mActiveGrid)
		{
			if (null != mActiveGrid.mSkillCooldown)
			{
				mActiveGrid.mSkillCooldown.fillAmount = 0f;
			}
			mActiveGrid = null;
		}
		if (null != activeG)
		{
			mActiveGrid = activeG;
			if (null != mActiveGrid.mSkillCooldown)
			{
				mActiveGrid.mSkillCooldown.fillAmount = 1f;
			}
		}
	}

	private void Awake()
	{
		mNumCount.enabled = mShowNum;
	}

	private void Update()
	{
		if (mMustNotSpr != null)
		{
			mMustNotSpr.enabled = MustNot;
		}
		if (mItemGrid != null && mItemGrid.GetCount() > 1)
		{
			mNumCount.text = mItemGrid.GetCount().ToString();
		}
		else
		{
			mNumCount.text = string.Empty;
		}
		if (null != mSkillCooldown && !mSampleMode && PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			UseItemCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<UseItemCmpt>();
			if (cmpt == null || mItemGrid == null)
			{
				return;
			}
			if (cmpt.GetCdByItemProtoId(mItemGrid.protoId) > 0f)
			{
				mSkillCooldown.fillAmount = cmpt.GetCdByItemProtoId(mItemGrid.protoId);
			}
			else
			{
				mSkillCooldown.fillAmount = 0f;
			}
		}
		if (mIsSkill && mSkillID > 0 && PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			if (mSkenity == null)
			{
				return;
			}
			UseItemCmpt cmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<UseItemCmpt>();
			if (cmpt2 == null)
			{
				return;
			}
			if (cmpt2.GetNpcSkillCd(mSkenity, mSkillID) > 0f)
			{
				mSkillCooldown.fillAmount = cmpt2.GetNpcSkillCd(mSkenity, mSkillID);
			}
			else
			{
				mSkillCooldown.fillAmount = 0f;
			}
		}
		if (ItemObj != null)
		{
			if (null != mDurability)
			{
				Durability cmpt3 = ItemObj.GetCmpt<Durability>();
				if (cmpt3 != null && cmpt3.value != null)
				{
					mDurability.alpha = 1f - cmpt3.value.current / cmpt3.valueMax;
				}
				else
				{
					mDurability.alpha = 0f;
				}
			}
		}
		else if (null != mDurability)
		{
			mDurability.alpha = 0f;
		}
		if (ItemObj != null)
		{
			if (mPowerPercent != null)
			{
				Energy cmpt4 = ItemObj.GetCmpt<Energy>();
				if (cmpt4 != null && cmpt4.energy != null && ItemObj.protoId == 228)
				{
					mPowerPercent.text = (int)(cmpt4.energy.percent * 100f) + "%";
				}
				else
				{
					mPowerPercent.text = string.Empty;
				}
			}
		}
		else if (mPowerPercent != null)
		{
			mPowerPercent.text = string.Empty;
		}
		UpdateEffect();
	}

	private void PlayGridEffect()
	{
		if ((mItemTex.enabled && mItemTex.mainTexture != null) || (mItemspr.enabled && mItemspr.spriteName != "Null"))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(UIGridEffectPrefab);
			gameObject.transform.parent = base.transform.parent;
			gameObject.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -5f);
			gameObject.transform.localScale = new Vector3(48f, 48f, 1f);
			effect = gameObject.GetComponentInChildren<UIGridEffect>();
			if (effect != null)
			{
				effect.e_OnEnd += EffectEnd;
			}
		}
	}

	private void EffectEnd(UIEffect _effect)
	{
		effect = null;
	}

	private void OnMouseMoveOut()
	{
		moveIn = false;
	}

	private void OnMouseMoveOver()
	{
		moveIn = true;
		moveInTime = 0f;
		isOnce = true;
	}

	private void UpdateEffect()
	{
		if (moveIn)
		{
			moveInTime += Time.deltaTime;
			if (moveInTime > 0.4f && isOnce)
			{
				PlayGridEffect();
				isOnce = false;
			}
			if (moveInTime > 5.2f && !isOnce)
			{
				PlayGridEffect();
				moveInTime = 0.4f;
			}
		}
	}

	private void UpdateSillcool()
	{
		if (mItemGrid != null && SelectItem_N.Instance.ItemSkillmar.IsInSkillCD(mItemGrid.protoId) && SkData.s_SkillTbl.ContainsKey(mItemGrid.protoData.skillId) && mItemGrid.protoData.skillId != 0)
		{
			float coolingTime = SkData.s_SkillTbl[mItemGrid.protoData.skillId]._coolingTime;
			mItemGrid.ClickedTime += Time.deltaTime;
			if (mItemGrid.ClickedTime <= coolingTime)
			{
				SelectItem_N.Instance.ItemSkillmar.Fillcount = 1f - mItemGrid.ClickedTime / coolingTime;
				return;
			}
			mItemGrid.ClickedTime = 0f;
			SelectItem_N.Instance.ItemSkillmar.Fillcount = 0f;
			SelectItem_N.Instance.ItemSkillmar.DelateSkillCD(mItemGrid.protoId);
			mItemGrid.Click = false;
		}
	}

	public void SetItem(int itemObjID)
	{
		SetItem(PeSingleton<ItemMgr>.Instance.Get(itemObjID));
	}

	public virtual void SetItem(ItemSample itemGrid, bool showNew = false)
	{
		mItemGrid = itemGrid;
		mSampleMode = mItemGrid == null;
		if (mItemTex == null)
		{
			return;
		}
		if (mItemGrid == null)
		{
			mItemTex.enabled = false;
			mItemTex.mainTexture = null;
			mItemspr.enabled = false;
			mNewMark.enabled = false;
			mNumCount.text = string.Empty;
			mDragMark.spriteName = "Null";
			mScript.spriteName = "Null";
			if (null != mSkillCooldown)
			{
				mSkillCooldown.fillAmount = 0f;
			}
			return;
		}
		mNewMark.enabled = showNew;
		if (ItemObj != null && null != ItemObj.iconTex)
		{
			mItemTex.enabled = true;
			mItemTex.mainTexture = ItemObj.iconTex;
			mItemspr.enabled = false;
		}
		else if (mItemGrid.iconString0 == "0")
		{
			mItemTex.enabled = true;
			mItemTex.mainTexture = mItemGrid.iconTex;
			mItemspr.enabled = false;
		}
		else
		{
			mItemTex.enabled = false;
			mItemTex.mainTexture = null;
			mItemspr.enabled = true;
			mItemspr.spriteName = mItemGrid.iconString0;
			mItemspr.MakePixelPerfect();
		}
		if (ItemObj != null && ItemObj.CanDrag())
		{
			mDragMark.spriteName = ItemObj.iconString1;
		}
		else
		{
			mDragMark.spriteName = "Null";
		}
		mDragMark.MakePixelPerfect();
		if (mItemGrid.protoData != null)
		{
			if (mItemGrid.protoData.maxStackNum == 1)
			{
				mNumCount.text = string.Empty;
			}
			if (mScript != null)
			{
				mScript.spriteName = mItemGrid.iconString2;
				mScript.MakePixelPerfect();
			}
		}
		if ((bool)mSkillCooldown)
		{
			mSkillCooldown.fillAmount = SelectItem_N.Instance.ItemSkillmar.GetFillcount(mItemGrid.protoId);
		}
	}

	public void SetItemPlace(ItemPlaceType itemPlace, int index)
	{
		mItemPlace = itemPlace;
		mItemIndex = index;
	}

	public void SetGridMask(GridMask mask)
	{
		mGridMask = mask;
	}

	public void SetSkill(int skillId, int index, SkEntity skenity = null, string icon = "", int decsId = 0)
	{
		if (icon != string.Empty)
		{
			string text = string.Empty;
			if (decsId > 0)
			{
				text = PELocalization.GetString(decsId);
			}
			mItemspr.spriteName = icon;
			mItemspr.MakePixelPerfect();
			mSkillID = skillId;
			mGridMask = GridMask.GM_Skill;
			mItemIndex = index;
			mItemspr.enabled = true;
			mIsSkill = true;
			mSkenity = skenity;
			mSkillDes = text;
		}
		else
		{
			mItemspr.spriteName = "Null";
			mItemspr.MakePixelPerfect();
			mItemspr.enabled = false;
			mIsSkill = false;
			mSkenity = null;
			if (null != mSkillCooldown)
			{
				mSkillCooldown.fillAmount = 0f;
			}
		}
	}

	public bool IsForbiden()
	{
		return mForbiden.gameObject.activeSelf;
	}

	public void SetGridForbiden(bool forbiden)
	{
		GetComponent<Collider>().enabled = !forbiden;
		mForbiden.gameObject.SetActive(forbiden);
	}

	public void SetDurabilityBg(ItemObject obj)
	{
		if (mDurabilitySpecial == null)
		{
			return;
		}
		if (obj == null)
		{
			mDurabilitySpecial.alpha = 0f;
			return;
		}
		Durability cmpt = obj.GetCmpt<Durability>();
		if (cmpt != null)
		{
			mDurabilitySpecial.alpha = 1f - cmpt.value.current / cmpt.valueMax;
		}
		else
		{
			mDurabilitySpecial.alpha = 0f;
		}
	}

	public void SetClick()
	{
		if (mItemGrid != null)
		{
			mItemGrid.Click = true;
			if (!(SelectItem_N.Instance == null) && SelectItem_N.Instance.ItemSkillmar != null)
			{
				SelectItem_N.Instance.ItemSkillmar.AddSkillCD(mItemGrid);
				mItemGrid.ClickedTime = 0f;
			}
		}
	}

	private void OnClick()
	{
		if (MustNot || mGridMask == GridMask.GM_Skill)
		{
			return;
		}
		if (ItemObj != null)
		{
		}
		if (Input.GetMouseButtonUp(0))
		{
			if (null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem())
			{
				OnDrop(null);
			}
		}
		else if (Input.GetMouseButtonUp(1))
		{
			if (null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem())
			{
				SelectItem_N.Instance.SetItem(null);
			}
			else if (mItemGrid != null && onRightMouseClicked != null)
			{
				onRightMouseClicked(this);
			}
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (!MustNot && onDragItem != null)
		{
			onDragItem(this);
		}
	}

	private void OnPress(bool press)
	{
		if (!MustNot && press && Input.GetMouseButtonDown(0) && null != SelectItem_N.Instance && !SelectItem_N.Instance.HaveOpItem() && mItemGrid != null)
		{
			if (ItemObj != null)
			{
			}
			if (onLeftMouseClicked != null)
			{
				onLeftMouseClicked(this);
			}
			SetActiveGrid(this);
		}
	}

	private void i(int itemid)
	{
	}

	private void OnDrop(GameObject go)
	{
		if (!MustNot && Input.GetMouseButtonUp(0) && null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem() && onDropItem != null)
		{
			onDropItem(this);
		}
	}

	private string SuitAttribute(string text)
	{
		if (text == null)
		{
			return string.Empty;
		}
		if (ItemObj == null)
		{
			return text;
		}
		string format = "[B7EF54]{0}[-]";
		string format2 = "[808080]{0}[-]";
		string format3 = "{0} ( {1}/{2} )";
		char c = '\n';
		string text2 = string.Empty;
		string text3 = string.Empty;
		string empty = string.Empty;
		string text4 = string.Empty;
		string text5 = string.Empty;
		List<string> list = new List<string>();
		for (int i = 0; i < m_CommonAttributeIDs.Count; i++)
		{
			list.Add(PELocalization.GetString(m_CommonAttributeIDs[i]));
		}
		if (list.Count > 0)
		{
			string[] array = text.Split(new string[1] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
			if (array != null && array.Length > 0)
			{
				string empty2 = string.Empty;
				bool flag = false;
				for (int j = 0; j < array.Length; j++)
				{
					flag = false;
					empty2 = array[j];
					for (int k = 0; k < list.Count; k++)
					{
						if (empty2.Contains(list[k]))
						{
							text5 = text5 + empty2 + c;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						text2 = text2 + empty2 + c;
					}
				}
			}
			if (text2.Length > 1)
			{
				text2 = text2.Substring(0, text2.Length - 1);
			}
			if (text5.Length > 1)
			{
				text5 = text5.Substring(0, text5.Length - 1);
			}
		}
		else
		{
			text2 = text;
		}
		EquipSetData data = EquipSetData.GetData(ItemObj);
		if (data != null)
		{
			text3 = data.desStr;
			if (text3.Length > 1)
			{
				text3 = text3.Substring(0, text3.Length - 1);
			}
		}
		EquipmentCmpt equipmentByCurItemPlace = GetEquipmentByCurItemPlace();
		List<SuitSetData.MatchData> list2 = new List<SuitSetData.MatchData>();
		if (null != equipmentByCurItemPlace && equipmentByCurItemPlace.matchDatas.Count > 0)
		{
			list2 = equipmentByCurItemPlace.matchDatas;
		}
		else
		{
			SuitSetData data2 = SuitSetData.GetData(ItemObj.protoId);
			if (data2 != null)
			{
				SuitSetData.MatchData item = default(SuitSetData.MatchData);
				item.name = data2.suitSetName;
				item.itemProtoList = data2.itemProtoList;
				item.itemNames = data2.itemNames;
				item.tips = data2.tips;
				item.activeTipsIndex = -1;
				list2.Add(item);
			}
		}
		empty = string.Empty;
		if (list2 != null && list2.Count > 0)
		{
			SuitSetData.MatchData matchData = default(SuitSetData.MatchData);
			string empty3 = string.Empty;
			for (int l = 0; l < list2.Count; l++)
			{
				if (list2[l].itemProtoList != null && list2[l].itemProtoList.Contains(ItemObj.protoId))
				{
					matchData = list2[l];
					break;
				}
			}
			if (matchData.itemNames != null && matchData.itemNames.Count > 0)
			{
				int num = ((matchData.activeIndex != null) ? matchData.activeIndex.Count((bool a) => a) : 0);
				empty = string.Format(format3, matchData.name, num, matchData.itemNames.Count);
				empty = string.Format(format, empty);
				empty += c;
				for (int m = 0; m < matchData.itemNames.Count; m++)
				{
					empty3 = matchData.itemNames[m];
					if (!string.IsNullOrEmpty(empty3))
					{
						empty3 = ((matchData.activeIndex == null || m >= matchData.activeIndex.Count || !matchData.activeIndex[m]) ? string.Format(format2, empty3) : string.Format(format, empty3));
						empty = empty + empty3 + c;
					}
				}
			}
			if (matchData.tips != null && matchData.tips.Length > 0)
			{
				for (int n = 0; n < matchData.tips.Length; n++)
				{
					if (matchData.tips[n] != 0)
					{
						empty3 = PELocalization.GetString(matchData.tips[n]);
						if (!string.IsNullOrEmpty(empty3))
						{
							empty3 = ((n > matchData.activeTipsIndex) ? string.Format(format2, empty3) : string.Format(format, empty3));
							text4 = text4 + empty3 + c;
						}
					}
				}
			}
		}
		if (empty.Length > 1)
		{
			empty = empty.Substring(0, empty.Length - 1);
		}
		if (text4.Length > 1)
		{
			text4 = text4.Substring(0, text4.Length - 1);
		}
		string text6 = string.Empty;
		if (string.IsNullOrEmpty(text3) && string.IsNullOrEmpty(empty) && string.IsNullOrEmpty(text4))
		{
			text6 = text;
		}
		else
		{
			if (!string.IsNullOrEmpty(text2))
			{
				string text7 = text6;
				text6 = text7 + text2 + c + c;
			}
			if (!string.IsNullOrEmpty(text3))
			{
				string text7 = text6;
				text6 = text7 + text3 + c + c;
			}
			if (!string.IsNullOrEmpty(empty))
			{
				string text7 = text6;
				text6 = text7 + empty + c + c;
			}
			if (!string.IsNullOrEmpty(text4))
			{
				string text7 = text6;
				text6 = text7 + text4 + c + c;
			}
			if (!string.IsNullOrEmpty(text5))
			{
				text6 += text5;
			}
			int num2 = text6.Length - 1;
			while (num2 >= 0 && text6[num2] == c)
			{
				text6 = text6.Substring(0, num2);
				num2--;
			}
		}
		return text6;
	}

	private EquipmentCmpt GetEquipmentByCurItemPlace()
	{
		return ItemPlace switch
		{
			ItemPlaceType.IPT_Equipment => (!(null == PeSingleton<MainPlayer>.Instance.entity)) ? PeSingleton<MainPlayer>.Instance.entity.equipmentCmpt : null, 
			ItemPlaceType.IPT_ServantEqu => (!(GameUI.Instance == null)) ? GameUI.Instance.mServantWndCtrl.GetCurServantEquipCmpt() : null, 
			ItemPlaceType.IPT_ConolyServantEquPersonel => (!(GameUI.Instance == null)) ? GameUI.Instance.mCSUI_MainWndCtrl.PersonnelUI.m_NPCEquipUI.NpcEquipment : null, 
			_ => null, 
		};
	}

	private void OnTooltip(bool show)
	{
		if (!show)
		{
			ToolTipsMgr.ShowText(null);
			return;
		}
		if (mIsSkill)
		{
			ToolTipsMgr.ShowText(mSkillDes);
			return;
		}
		if (mItemGrid == null)
		{
			ToolTipsMgr.ShowText(null);
			return;
		}
		string content = string.Empty;
		if (Item != null)
		{
			content = Item.GetTooltip();
			content = SuitAttribute(content);
		}
		ToolTipsMgr.ShowText(content);
	}
}
