using UnityEngine;
using WhiteCat;

public class VCEUIStatisticsPanel : MonoBehaviour
{
	public bool m_IsEditor = true;

	public string m_NonEditorRemark = string.Empty;

	[HideInInspector]
	public string m_NonEditorError = string.Empty;

	[HideInInspector]
	public byte[] m_NonEditorIcon;

	public string m_NonEditorISOName = string.Empty;

	public string m_NonEditorISODesc = string.Empty;

	public string m_NonEditorISOVersion = string.Empty;

	public UITable m_UITable;

	public BoxCollider m_DragContentCollider;

	public UILabel m_ISOName;

	public UILabel m_ISOCategory;

	public UILabel m_ISOSize;

	public UILabel m_ISOStat;

	public UILabel m_ISODesc;

	public UILabel ISOVersionLabel;

	public UITexture m_ISOIcon64;

	public UITexture m_ISOIcon32;

	public Material m_IconMatRes;

	private Material m_IconMat;

	private Texture2D m_IconTex;

	public UICheckbox ckItemTrack;

	public UILabel m_AttrNames;

	public UILabel m_AttrValues;

	public VCEUICostList m_CostList;

	public bool m_RefreshNow;

	private string last_desc = string.Empty;

	private CreationAttr _curAttr;

	public void Init()
	{
		m_IconMat = Object.Instantiate(m_IconMatRes);
		m_ISOIcon64.material = m_IconMat;
		if (m_ISOIcon32 != null)
		{
			m_ISOIcon32.material = m_IconMat;
		}
		m_CostList.Init();
	}

	private void OnDestroy()
	{
		if (m_IconMat != null)
		{
			Object.Destroy(m_IconMat);
			m_IconMat = null;
		}
		if (m_IconTex != null)
		{
			Object.Destroy(m_IconTex);
			m_IconTex = null;
		}
	}

	public void SetIsoIcon()
	{
		if (m_IconTex != null)
		{
			Object.Destroy(m_IconTex);
			m_IconMat.mainTexture = null;
		}
		byte[] array = ((!m_IsEditor) ? m_NonEditorIcon : VCEditor.s_Scene.m_IsoData.m_HeadInfo.IconTex);
		if (array != null)
		{
			m_IconTex = new Texture2D(2, 2);
			m_IconTex.LoadImage(array);
			m_IconMat.mainTexture = m_IconTex;
			m_ISOIcon64.gameObject.SetActive(value: false);
			m_ISOIcon64.gameObject.SetActive(value: true);
			if (m_ISOIcon32 != null)
			{
				m_ISOIcon32.gameObject.SetActive(value: false);
				m_ISOIcon32.gameObject.SetActive(value: true);
			}
		}
	}

	private void Awake()
	{
		if (!m_IsEditor)
		{
			Init();
		}
	}

	private void Update()
	{
		if (m_RefreshNow)
		{
			m_RefreshNow = false;
			OnCreationInfoRefresh();
		}
		if (m_IsEditor)
		{
			VCIsoData isoData = VCEditor.s_Scene.m_IsoData;
			if (isoData.m_HeadInfo.Name.Trim().Length > 0)
			{
				m_ISOName.text = isoData.m_HeadInfo.Name;
				m_ISOName.color = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				m_ISOName.text = "< Untitled >".ToLocalizationString();
				m_ISOName.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
			}
			if (isoData.m_HeadInfo.Desc.Trim().Length > 0)
			{
				m_ISODesc.text = isoData.m_HeadInfo.Desc + "\r\n";
				m_ISODesc.color = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				m_ISODesc.text = "< No description >".ToLocalizationString() + "\r\n";
				m_ISODesc.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
			}
			m_ISOCategory.text = VCConfig.s_Categories[isoData.m_HeadInfo.Category].m_Name.ToLocalizationString();
			m_ISOSize.text = "Width".ToLocalizationString() + ": " + isoData.m_HeadInfo.xSize + "\r\n" + "Depth".ToLocalizationString() + ": " + isoData.m_HeadInfo.zSize + "\r\n" + "Height".ToLocalizationString() + ": " + isoData.m_HeadInfo.ySize;
			m_ISOStat.text = isoData.m_Components.Count.ToString("#,##0") + " " + "component(s)".ToLocalizationString() + "\r\n" + isoData.m_Voxels.Count.ToString("#,##0") + " " + "voxel(s)".ToLocalizationString() + "\r\n" + isoData.MaterialUsedCount() + " " + "material(s)".ToLocalizationString() + "\r\n" + isoData.m_Colors.Count.ToString("#,##0") + " " + "color unit(s)".ToLocalizationString();
			if (m_ISODesc.text != last_desc)
			{
				m_UITable.Reposition();
				last_desc = m_ISODesc.text;
			}
		}
		else
		{
			if (m_NonEditorISOName.Trim().Length > 0)
			{
				m_ISOName.text = m_NonEditorISOName;
				m_ISOName.color = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				m_ISOName.text = "< Untitled >".ToLocalizationString();
				m_ISOName.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
			}
			if (m_NonEditorISODesc.Trim().Length > 0)
			{
				m_ISODesc.text = m_NonEditorISODesc + "\r\n";
				m_ISODesc.color = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				m_ISODesc.text = "< No description >".ToLocalizationString() + "\r\n";
				m_ISODesc.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
			}
			m_ISOCategory.text = string.Empty;
			m_ISOSize.text = string.Empty;
			m_ISOStat.text = string.Empty;
			if (m_ISODesc.text != last_desc)
			{
				m_UITable.Reposition();
				last_desc = m_ISODesc.text;
			}
			if (m_NonEditorISOVersion != null)
			{
				float result = 0f;
				float num = float.Parse(SteamWorkShop.NewVersionTag);
				if (float.TryParse(m_NonEditorISOVersion, out result) && result >= num)
				{
					ISOVersionLabel.text = "ISO " + "Version".ToLocalizationString() + ": [FF0000]" + m_NonEditorISOVersion + "[-]";
				}
				else
				{
					ISOVersionLabel.text = "ISO " + "Version".ToLocalizationString() + ": [00FFFF]<" + SteamWorkShop.NewVersionTag + "[-]";
				}
			}
		}
		Vector3 size = m_DragContentCollider.size;
		Vector3 center = m_DragContentCollider.center;
		size.y = m_UITable.mVariableHeight;
		center.y = (0f - size.y) / 2f;
		m_DragContentCollider.size = size;
		m_DragContentCollider.center = center;
		Vector3 localPosition = m_UITable.transform.localPosition;
		if (Mathf.Abs(localPosition.x) > 2f)
		{
			localPosition.x = 0f;
		}
		m_UITable.transform.localPosition = localPosition;
	}

	public void OnCreationInfoRefresh()
	{
		CreationAttr creationAttr = null;
		if (m_IsEditor)
		{
			CreationData.CalcCreationAttr(VCEditor.s_Scene.m_IsoData, 0f, ref VCEditor.s_Scene.m_CreationAttr);
			creationAttr = VCEditor.s_Scene.m_CreationAttr;
		}
		else
		{
			VCIsoRemark vCIsoRemark = new VCIsoRemark();
			vCIsoRemark.xml = m_NonEditorRemark;
			creationAttr = vCIsoRemark.m_Attribute;
			creationAttr?.CheckCostId();
			m_CostList.m_NonEditorAttr = creationAttr;
			if (vCIsoRemark.m_Error != null && vCIsoRemark.m_Error.Length > 1)
			{
				m_NonEditorError = "ISO version is obsolete".ToLocalizationString();
			}
			else
			{
				m_NonEditorError = string.Empty;
			}
		}
		UpdateItemsTrackState(creationAttr);
		if (creationAttr != null)
		{
			if (creationAttr.m_Type == ECreation.Sword || creationAttr.m_Type == ECreation.SwordLarge || creationAttr.m_Type == ECreation.SwordDouble || creationAttr.m_Type == ECreation.Axe)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Attack".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = ((creationAttr.m_Type != ECreation.Sword && creationAttr.m_Type != ECreation.SwordLarge && creationAttr.m_Type != ECreation.SwordDouble) ? ("Axe".ToLocalizationString() + "\r\n") : ("Sword".ToLocalizationString() + "\r\n")) + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Attack.ToString("0.0") + "\r\n" + Mathf.CeilToInt(creationAttr.m_Durability * PEVCConfig.equipDurabilityShowScale).ToString("0.0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.Bow)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Attack".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = "Bow".ToLocalizationString() + "\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Attack.ToString("0.0") + "\r\n" + Mathf.CeilToInt(creationAttr.m_Durability * PEVCConfig.equipDurabilityShowScale).ToString("0.0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.Shield)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Defense".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = "Shield".ToLocalizationString() + "\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Defense.ToString("0.0") + "\r\n" + Mathf.CeilToInt(creationAttr.m_Durability * PEVCConfig.equipDurabilityShowScale).ToString("0.0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.HandGun || creationAttr.m_Type == ECreation.Rifle)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Increase".ToLocalizationString() + ":\r\n" + "Final Attack".ToLocalizationString() + ":\r\n" + "Firing Rate".ToLocalizationString() + ":\r\n" + "Accuracy".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = ((creationAttr.m_Type != ECreation.HandGun) ? ("Rifle".ToLocalizationString() + "\r\n") : ("Hand Gun".ToLocalizationString() + "\r\n")) + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + ((creationAttr.m_MuzzleAtkInc != 0f) ? (creationAttr.m_MuzzleAtkInc * 100f - 100f).ToString("0.0") : "-") + " %\r\n" + creationAttr.m_Attack.ToString("0.0") + "\r\n" + creationAttr.m_FireSpeed.ToString("0.0") + "\r\n" + (1f / creationAttr.m_Accuracy * 100f).ToString("0.0") + " %\r\n" + Mathf.CeilToInt(creationAttr.m_Durability * PEVCConfig.equipDurabilityShowScale).ToString("0.0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.Vehicle)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Attack".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n" + "Fuel".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = "Vehicle".ToLocalizationString() + "\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Attack.ToString("#,##0") + " /s\r\n" + creationAttr.m_Durability.ToString("#,##0") + "\r\n" + creationAttr.m_MaxFuel.ToString("#,##0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.Aircraft)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Attack".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n" + "Fuel".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = "Aircraft".ToLocalizationString() + "\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Attack.ToString("#,##0") + " /s\r\n" + creationAttr.m_Durability.ToString("#,##0") + "\r\n" + creationAttr.m_MaxFuel.ToString("#,##0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.Boat)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Attack".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n" + "Fuel".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = "Boat".ToLocalizationString() + "\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Attack.ToString("#,##0") + " /s\r\n" + creationAttr.m_Durability.ToString("#,##0") + "\r\n" + creationAttr.m_MaxFuel.ToString("#,##0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.SimpleObject)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Durability".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = "Object".ToLocalizationString() + "\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Durability.ToString("#,##0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.Robot)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Attack".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = "Object".ToLocalizationString() + "\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Attack.ToString("#,##0") + " /s\r\n" + creationAttr.m_Durability.ToString("#,##0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.AITurret)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Attack".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = "Object".ToLocalizationString() + "\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Attack.ToString("#,##0") + " /s\r\n" + creationAttr.m_Durability.ToString("#,##0") + "\r\n";
			}
			else if (creationAttr.m_Type == ECreation.ArmorHead || creationAttr.m_Type == ECreation.ArmorBody || creationAttr.m_Type == ECreation.ArmorArmAndLeg || creationAttr.m_Type == ECreation.ArmorHandAndFoot || creationAttr.m_Type == ECreation.ArmorDecoration)
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n\r\n" + "Defense".ToLocalizationString() + ":\r\n" + "Durability".ToLocalizationString() + ":\r\n\r\n";
				m_AttrValues.text = "Object".ToLocalizationString() + "\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n\r\n" + creationAttr.m_Defense.ToString("#,##0") + "\r\n" + creationAttr.m_Durability.ToString("#,##0") + "\r\n";
			}
			else
			{
				m_AttrNames.text = "Creation".ToLocalizationString() + ":\r\n" + "Weight".ToLocalizationString() + ":\r\n" + "Volume".ToLocalizationString() + ":\r\n" + "Sell Price".ToLocalizationString() + ":\r\n";
				m_AttrValues.text = "[FF0000]???[-]\r\n" + VCUtils.WeightToString(creationAttr.m_Weight) + "\r\n" + VCUtils.VolumeToString(creationAttr.m_Volume) + "\r\n" + creationAttr.m_SellPrice.ToString("#,##0") + " " + "Meat".ToLocalizationString() + "\r\n";
			}
			if (creationAttr.m_Errors.Count > 0)
			{
				UILabel attrNames = m_AttrNames;
				attrNames.text = attrNames.text + "\r\n[FF0000]" + "Errors".ToLocalizationString() + ":\r\n";
				foreach (string error in creationAttr.m_Errors)
				{
					UILabel attrNames2 = m_AttrNames;
					attrNames2.text = attrNames2.text + "> " + error.ToLocalizationString() + "\r\n";
				}
				m_AttrNames.text += "[-]";
			}
			if (creationAttr.m_Warnings.Count > 0)
			{
				UILabel attrNames3 = m_AttrNames;
				attrNames3.text = attrNames3.text + "\r\n[FFFF00]" + "Warnings".ToLocalizationString() + ":\r\n";
				foreach (string warning in creationAttr.m_Warnings)
				{
					UILabel attrNames4 = m_AttrNames;
					attrNames4.text = attrNames4.text + "> " + warning.ToLocalizationString() + "\r\n";
				}
				m_AttrNames.text += "[-]";
			}
			m_CostList.RefreshCostList();
			m_UITable.Reposition();
		}
		else
		{
			m_AttrNames.text = "[FF0000]" + m_NonEditorError + "[-]";
			m_AttrValues.text = string.Empty;
			m_CostList.RefreshCostList();
			m_UITable.Reposition();
		}
		if (m_IsEditor)
		{
			if (VCEditor.s_Scene.m_CreationAttr.m_Errors.Count > 0)
			{
				VCEStatusBar.ShowText("Your creation has some errors".ToLocalizationString(), Color.red, 10f);
			}
			else if (VCEditor.s_Scene.m_CreationAttr.m_Warnings.Count > 0)
			{
				VCEStatusBar.ShowText("Your creation has some warnings".ToLocalizationString(), Color.yellow, 10f);
			}
			if (creationAttr.m_Weight > 0.0001f)
			{
				VCEditor.Instance.m_MassCenterTrans.localPosition = creationAttr.m_CenterOfMass;
				VCEditor.Instance.m_MassCenterTrans.gameObject.SetActive(value: true);
			}
			else
			{
				VCEditor.Instance.m_MassCenterTrans.gameObject.SetActive(value: false);
			}
			VCIsoRemark vCIsoRemark2 = new VCIsoRemark();
			vCIsoRemark2.m_Attribute = creationAttr;
			VCEditor.s_Scene.m_IsoData.m_HeadInfo.Remarks = vCIsoRemark2.xml;
		}
	}

	public void OnCostRefresh()
	{
		OnCreationInfoRefresh();
	}

	private void OnDisable()
	{
		if (m_IsEditor)
		{
			VCEditor.Instance.m_MassCenterTrans.gameObject.SetActive(value: false);
		}
	}

	private void UpdateItemsTrackState(CreationAttr attr)
	{
		if ((bool)ckItemTrack)
		{
			_curAttr = attr;
			string text = VCEditor.s_Scene.m_IsoData.m_HeadInfo.Name;
			bool flag = !string.IsNullOrEmpty(text) && _curAttr != null && (bool)GameUI.Instance && (bool)GameUI.Instance.mItemsTrackWnd;
			ckItemTrack.gameObject.SetActive(flag);
			if (flag)
			{
				ckItemTrack.isChecked = GameUI.Instance.mItemsTrackWnd.ContainsIso(text);
			}
		}
	}

	private void OnItemTrackCk(bool isChecked)
	{
		if (!ckItemTrack || _curAttr == null || !GameUI.Instance || !GameUI.Instance.mItemsTrackWnd)
		{
			return;
		}
		string text = VCEditor.s_Scene.m_IsoData.m_HeadInfo.Name;
		if (!string.IsNullOrEmpty(text))
		{
			if (isChecked)
			{
				GameUI.Instance.mItemsTrackWnd.AddIso(text, _curAttr.m_Cost);
			}
			else
			{
				GameUI.Instance.mItemsTrackWnd.RemoveIso(text);
			}
		}
	}
}
