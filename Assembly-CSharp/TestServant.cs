using Pathea;
using UnityEngine;

public class TestServant : MonoBehaviour
{
	[SerializeField]
	private int addServantId = 9008;

	[SerializeField]
	private bool add;

	private Texture mTex;

	private void Update()
	{
		if (add)
		{
			add = false;
			AddServant();
		}
	}

	private void AddServant()
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(addServantId);
		if (peEntity == null)
		{
			Debug.Log("Get entity failed!");
			return;
		}
		ServantLeaderCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
		if (!(null == cmpt))
		{
			NpcCmpt npcCmpt = peEntity.NpcCmpt;
			if (npcCmpt != null)
			{
				CSMain.SetNpcFollower(peEntity);
			}
			EntityInfoCmpt cmpt2 = peEntity.GetCmpt<EntityInfoCmpt>();
			if (cmpt2 != null)
			{
				mTex = cmpt2.faceTex;
			}
		}
	}

	private void OnGUI()
	{
		if (mTex != null)
		{
			GUILayout.Label(mTex, GUILayout.MaxWidth(200f), GUILayout.MaxHeight(200f));
		}
	}
}
