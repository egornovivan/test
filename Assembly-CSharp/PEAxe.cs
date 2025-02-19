using ItemAsset;

public class PEAxe : PEHoldAbleEquipment
{
	public int m_FellSkillID;

	public float[] m_AnimDownThreshold = new float[2] { 40f, 20f };

	public float m_AnimSpeed = 1f;

	public float m_StaminaCost = 5f;

	public string fellAnim = "Fell";

	public ItemObject ItemObj => m_ItemObj;
}
