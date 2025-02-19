using UnityEngine;

namespace Pathea;

public class PEPortal : MonoBehaviour
{
	public Vector3 transPoint;

	public int descriptionID;

	private void OnTriggerEnter(Collider other)
	{
		PeEntity componentInParent = other.GetComponentInParent<PeEntity>();
		if (null != componentInParent && componentInParent == PeSingleton<MainPlayer>.Instance.entity)
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(descriptionID), Do);
		}
	}

	private void Do()
	{
		if (PeGameMgr.IsMulti)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_PlayerReset, transPoint);
				PlayerNetwork.mainPlayer.RequestChangeScene(0);
			}
		}
		else
		{
			PeSingleton<FastTravelMgr>.Instance.TravelTo(transPoint);
		}
	}
}
