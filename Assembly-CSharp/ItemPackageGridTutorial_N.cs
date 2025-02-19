using UnityEngine;

public class ItemPackageGridTutorial_N : MonoBehaviour
{
	private int m_ProtoID;

	public int ProtoID => m_ProtoID;

	public void SetProtoID(int protoID)
	{
		m_ProtoID = protoID;
	}
}
