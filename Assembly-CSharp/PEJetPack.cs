using ItemAsset;
using Pathea;
using UnityEngine;

public class PEJetPack : PECtrlAbleEquipment
{
	private const string AttachBone = "Bow_box";

	public int m_StartSoundID = 794;

	public int m_SoundID = 794;

	public GameObject m_EffectObj;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_View.AttachObject(base.gameObject, "Bow_box");
	}
}
