using ItemAsset;
using Pathea;
using UnityEngine;

public class VCEUICostItem : MonoBehaviour
{
	public int m_GameItemId;

	public int m_GameItemCost;

	public UISprite m_IconSprite;

	public UILabel m_NameLabel;

	public UILabel m_CountLabel;

	public bool m_IsEnough;

	private void Start()
	{
		ItemSample itemSample = new ItemSample(m_GameItemId);
		if (itemSample.protoData == null)
		{
			return;
		}
		m_IconSprite.spriteName = itemSample.iconString0;
		m_NameLabel.text = VCUtils.Capital(itemSample.nameText, everyword: true);
		if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			if (VCEditor.Instance.m_CheatWhenMakeCreation)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Cheat".ToLocalizationString() + "[-]";
				m_IsEnough = true;
				return;
			}
			if (PeGameMgr.IsSingleBuild)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Build".ToLocalizationString() + "[-]";
				m_IsEnough = true;
				return;
			}
			if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Tutorial".ToLocalizationString() + "[-]";
				m_IsEnough = true;
				return;
			}
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			int count = cmpt.package.GetCount(m_GameItemId);
			if (count >= m_GameItemCost)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + count.ToString("#,##0").Trim() + "[-]";
				m_IsEnough = true;
			}
			else
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [FF0000]" + count.ToString("#,##0").Trim() + "[-]";
				m_IsEnough = false;
			}
		}
		else
		{
			m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim();
			m_IsEnough = false;
		}
	}

	private void Update()
	{
		if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			if (VCEditor.Instance.m_CheatWhenMakeCreation)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Cheat".ToLocalizationString() + "[-]";
				m_IsEnough = true;
				return;
			}
			if (PeGameMgr.IsSingleBuild)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Build".ToLocalizationString() + "[-]";
				m_IsEnough = true;
				return;
			}
			if (PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + "Tutorial".ToLocalizationString() + "[-]";
				m_IsEnough = true;
				return;
			}
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			int count = cmpt.package.GetCount(m_GameItemId);
			if (count >= m_GameItemCost)
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [60FF00]" + count.ToString("#,##0").Trim() + "[-]";
				m_IsEnough = true;
			}
			else
			{
				m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim() + " / [FF0000]" + count.ToString("#,##0").Trim() + "[-]";
				m_IsEnough = false;
			}
		}
		else
		{
			m_CountLabel.text = m_GameItemCost.ToString("#,##0").Trim();
			m_IsEnough = false;
		}
	}
}
