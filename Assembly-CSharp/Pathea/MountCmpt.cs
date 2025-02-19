using System.Collections;

namespace Pathea;

public class MountCmpt : PeCmpt, IPeMsg
{
	public PeEntity Mount { get; private set; }

	public void OnMsg(EMsg msg, params object[] args)
	{
		if (msg != EMsg.View_Prefab_Build)
		{
			return;
		}
		if (PeGameMgr.IsSingle)
		{
			RelationshipDataMgr.RecoverRelationship(PeSingleton<PeCreature>.Instance.mainPlayerId);
		}
		else if ((bool)Mount)
		{
			if (Mount.hasView)
			{
				RideMount();
				return;
			}
			Mount.biologyViewCmpt.Build();
			StartCoroutine("RideMountIterator");
		}
	}

	public override void OnUpdate()
	{
		if (PeGameMgr.IsMulti && !base.Entity.IsMainPlayer && (bool)Mount && (bool)Mount.peTrans && (bool)base.Entity && !base.Entity.hasView && (bool)base.Entity.peTrans)
		{
			base.Entity.peTrans.position = Mount.peTrans.position;
		}
	}

	private IEnumerator RideMountIterator()
	{
		if ((bool)Mount && !Mount.hasView)
		{
			yield return null;
		}
		RideMount();
	}

	private void RideMount()
	{
		if ((bool)Mount && Mount.hasView && (bool)Mount.biologyViewCmpt && (bool)Mount.biologyViewCmpt.biologyViewRoot && (bool)Mount.biologyViewCmpt.biologyViewRoot.modelController && (bool)base.Entity && (bool)base.Entity.operateCmpt)
		{
			MousePickRides component = Mount.biologyViewCmpt.biologyViewRoot.modelController.GetComponent<MousePickRides>();
			if ((bool)component)
			{
				component.ExecRide(base.Entity);
			}
		}
	}

	public void SetMount(PeEntity mount)
	{
		if (null != mount)
		{
			Mount = mount;
			Mount.SetMount(isMount: true);
			if (!PeGameMgr.IsMulti)
			{
				RelationshipDataMgr.AddRelationship(base.Entity, Mount);
			}
		}
	}

	public void DelMount()
	{
		if (null != Mount)
		{
			if (!PeGameMgr.IsMulti)
			{
				RelationshipDataMgr.RemoveRalationship(base.Entity.Id, Mount.ProtoID);
			}
			Mount.SetMount(isMount: false);
			Mount = null;
		}
	}
}
