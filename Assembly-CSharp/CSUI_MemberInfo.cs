using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_MemberInfo : MonoBehaviour
{
	[SerializeField]
	private UILabel mLbName;

	[SerializeField]
	private UISprite mSprSex;

	[SerializeField]
	private UILabel mLbHealth;

	[SerializeField]
	private UISlider mSdHealth;

	[SerializeField]
	private UILabel mLbStamina;

	[SerializeField]
	private UISlider mSdStamina;

	[SerializeField]
	private UILabel mLbHunger;

	[SerializeField]
	private UISlider mSdHunger;

	[SerializeField]
	private UILabel mLbComfort;

	[SerializeField]
	private UISlider mSdComfort;

	[SerializeField]
	private UILabel mLbOxygen;

	[SerializeField]
	private UISlider mSdOxygen;

	[SerializeField]
	private UILabel mLbShield;

	[SerializeField]
	private UISlider mSdShield;

	[SerializeField]
	private UILabel mLbEnergy;

	[SerializeField]
	private UISlider mSdEnergy;

	[SerializeField]
	private UILabel mLbAttack;

	[SerializeField]
	private UILabel mLbDefense;

	[SerializeField]
	private UIGrid m_ItemRoot;

	[SerializeField]
	private UIGrid m_SkillRoot;

	[SerializeField]
	private GameObject m_InfoPage;

	[SerializeField]
	private GameObject m_InventoryPage;

	public CSUI_Grid m_GridPrefab;

	private CSPersonnel m_RefNpc;

	private List<CSUI_Grid> m_ItemGrids = new List<CSUI_Grid>();

	private List<CSUI_Grid> m_SkillGrids = new List<CSUI_Grid>();

	public CSPersonnel RefNpc
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc = value;
			UpdateItemGrid();
			UpdateSkills();
		}
	}

	private void Start()
	{
		for (int i = 0; i < 10; i++)
		{
			CSUI_Grid cSUI_Grid = UnityEngine.Object.Instantiate(m_GridPrefab);
			cSUI_Grid.transform.parent = m_ItemRoot.transform;
			cSUI_Grid.transform.localPosition = Vector3.zero;
			cSUI_Grid.transform.localRotation = Quaternion.identity;
			cSUI_Grid.transform.localScale = Vector3.one;
			cSUI_Grid.m_Active = true;
			cSUI_Grid.m_UseDefaultExchangeDel = false;
			cSUI_Grid.m_Index = i;
			m_ItemGrids.Add(cSUI_Grid);
			cSUI_Grid.onCheckItem = OnCheckItemGrid;
			cSUI_Grid.OnItemChanged = OnItemGridChanged;
			cSUI_Grid.m_Grid.onGridsExchangeItem = OnGridsExchangeItem;
		}
		m_ItemRoot.repositionNow = true;
		for (int j = 0; j < 5; j++)
		{
			CSUI_Grid cSUI_Grid2 = UnityEngine.Object.Instantiate(m_GridPrefab);
			cSUI_Grid2.transform.parent = m_SkillRoot.transform;
			cSUI_Grid2.transform.localPosition = Vector3.zero;
			cSUI_Grid2.transform.localRotation = Quaternion.identity;
			cSUI_Grid2.transform.localScale = Vector3.one;
			cSUI_Grid2.m_Active = false;
			cSUI_Grid2.m_Index = j;
			m_SkillGrids.Add(cSUI_Grid2);
		}
		m_SkillRoot.repositionNow = true;
	}

	private void Update()
	{
		if (m_RefNpc == null || m_RefNpc.NPC == null)
		{
			SetServantInfo("--", PeSex.Male, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		}
		else
		{
			SetServantInfo(m_RefNpc.FullName, m_RefNpc.Sex, (int)m_RefNpc.GetAttribute(AttribType.Hp), (int)m_RefNpc.GetAttribute(AttribType.HpMax), (int)m_RefNpc.Stamina, (int)m_RefNpc.MaxStamina, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (int)m_RefNpc.GetAttribute(AttribType.CutDamage), (int)m_RefNpc.GetAttribute(AttribType.Def));
		}
	}

	private void UpdateItemGrid()
	{
		if (m_ItemGrids.Count != 0)
		{
			for (int i = 0; i < 10; i++)
			{
				m_ItemGrids[i].m_Grid.SetItem(null);
			}
			if (m_RefNpc == null)
			{
			}
		}
	}

	private void UpdateSkills()
	{
		if (m_RefNpc == null || m_RefNpc.NPC == null || !m_RefNpc.IsRandomNpc || m_RefNpc.SkAlive == null)
		{
			return;
		}
		if (m_RefNpc.Npcabliys != null)
		{
			int num = 0;
			{
				foreach (NpcAbility npcabliy in m_RefNpc.Npcabliys)
				{
					if (npcabliy != null)
					{
						m_SkillGrids[num].m_Grid.SetSkill(npcabliy.skillId, num, m_RefNpc.SkAlive, npcabliy.icon, npcabliy.desc);
						num++;
					}
				}
				return;
			}
		}
		for (int i = 0; i < m_SkillGrids.Count; i++)
		{
			m_SkillGrids[i].m_Grid.SetSkill(0, i, null, string.Empty);
		}
	}

	private void SetServantInfo(string name, PeSex sex, int health, int healthMax, int stamina, int stamina_max, int hunger, int hunger_max, int comfort, int comfort_max, int oxygen, int oxygen_max, int shield, int shield_max, int energy, int energy_max, int attack, int defense)
	{
		mLbName.text = name;
		mSprSex.spriteName = ((sex != PeSex.Male) ? "woman" : "man");
		mLbHealth.text = Convert.ToString(health) + "/" + Convert.ToString(healthMax);
		mSdHealth.sliderValue = ((healthMax <= 0) ? 0f : (Convert.ToSingle(health) / (float)healthMax));
		mLbStamina.text = Convert.ToString(stamina) + "/" + Convert.ToString(stamina_max);
		mSdStamina.sliderValue = ((stamina_max <= 0) ? 0f : (Convert.ToSingle(stamina) / (float)stamina_max));
		mLbHunger.text = Convert.ToString(hunger) + "/" + Convert.ToString(hunger_max);
		mSdHunger.sliderValue = ((hunger_max <= 0) ? 0f : (Convert.ToSingle(hunger) / (float)hunger_max));
		mLbComfort.text = Convert.ToString(comfort) + "/" + Convert.ToString(comfort_max);
		mSdComfort.sliderValue = ((comfort_max <= 0) ? 0f : (Convert.ToSingle(comfort) / (float)comfort_max));
		mLbOxygen.text = Convert.ToString(oxygen) + "/" + Convert.ToString(oxygen_max);
		mSdOxygen.sliderValue = ((oxygen_max <= 0) ? 0f : (Convert.ToSingle(oxygen) / (float)oxygen_max));
		mLbShield.text = Convert.ToString(shield) + "/" + Convert.ToString(shield_max);
		mSdShield.sliderValue = ((shield_max <= 0) ? 0f : (Convert.ToSingle(shield) / (float)shield_max));
		mLbEnergy.text = Convert.ToString(energy) + "/" + Convert.ToString(energy_max);
		mSdEnergy.sliderValue = ((energy_max <= 0) ? 0f : (Convert.ToSingle(energy) / (float)energy_max));
		mLbAttack.text = Convert.ToString(attack);
		mLbDefense.text = Convert.ToString(defense);
	}

	private bool OnCheckItemGrid(ItemObject item, CSUI_Grid.ECheckItemType check_type)
	{
		if (m_RefNpc == null)
		{
			return false;
		}
		return true;
	}

	private void OnItemGridChanged(ItemObject item, ItemObject oldItem, int index)
	{
		if (oldItem != null)
		{
			if (item == null)
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayItemFromNpc.GetString(), oldItem.protoData.GetName(), m_RefNpc.FullName));
			}
			else if (item == oldItem)
			{
				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNotEnoughGrid.GetString(), Color.red);
			}
			else
			{
				CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutItemToNpc.GetString(), item.protoData.GetName(), m_RefNpc.FullName));
			}
		}
		else if (item != null)
		{
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutItemToNpc.GetString(), item.protoData.GetName(), m_RefNpc.FullName));
		}
	}

	private void OnGridsExchangeItem(Grid_N grid, ItemObject item)
	{
		grid.SetItem(item);
	}

	private void PageInfoOnActive(bool active)
	{
		m_InfoPage.SetActive(active);
	}

	private void PageInvetoryOnActive(bool active)
	{
		m_InventoryPage.SetActive(active);
	}
}
