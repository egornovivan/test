using ItemAsset;
using UnityEngine;

public class VCEUIPartItem : MonoBehaviour
{
	public VCEUIPartList m_ParentList;

	public VCPartInfo m_PartInfo;

	public UISprite m_IconSprite;

	public UILabel m_NameLabel;

	public GameObject m_HoverBtn;

	public GameObject m_SelectedSign;

	private void Start()
	{
		m_IconSprite.spriteName = m_PartInfo.m_IconPath.Split(',')[0];
		m_NameLabel.text = VCUtils.Capital(ItemProto.GetName(m_PartInfo.m_ItemID), everyword: true);
		m_SelectedSign.SetActive(value: false);
	}

	private void Update()
	{
		m_SelectedSign.SetActive(VCEditor.SelectedPart == m_PartInfo);
	}

	private void OnSelectClick()
	{
		VCEditor.SelectedPart = m_PartInfo;
	}
}
