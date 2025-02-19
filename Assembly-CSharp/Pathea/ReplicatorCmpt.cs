using System.IO;

namespace Pathea;

public class ReplicatorCmpt : PeCmpt, Replicator.IHandler
{
	private PlayerPackageCmpt mPackage;

	public Replicator replicator { get; private set; }

	public ReplicatorCmpt()
	{
		replicator = new Replicator(this);
	}

	int Replicator.IHandler.ItemCount(int itemId)
	{
		return mPackage.package.GetCount(itemId);
	}

	bool Replicator.IHandler.DeleteItem(int itemId, int count)
	{
		return mPackage.Destory(itemId, count);
	}

	bool Replicator.IHandler.CreateItem(int itemId, int count)
	{
		return mPackage.Add(itemId, count);
	}

	bool Replicator.IHandler.HasEnoughPackage(int itemId, int count)
	{
		return mPackage.package.CanAdd(itemId, count);
	}

	public override void Start()
	{
		base.Start();
		mPackage = base.Entity.GetCmpt<PlayerPackageCmpt>();
	}

	public override void Serialize(BinaryWriter w)
	{
		replicator.Serialize(w);
	}

	public override void Deserialize(BinaryReader r)
	{
		replicator.Deserialize(r);
	}

	public override void OnUpdate()
	{
		if (RandomMapConfig.useSkillTree && null != base.Entity.skillTreeCmpt)
		{
			replicator.needTimeScale = GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckReduceTime(1f);
		}
		replicator.UpdateReplicate();
	}
}
