namespace Pathea;

public class PeMousePickCmpt : PeCmpt, IPeMsg
{
	private MousePickable mMousePick;

	public MousePickablePeEntity mousePick => mousePickable as MousePickablePeEntity;

	private MousePickable mousePickable
	{
		get
		{
			if (null == mMousePick)
			{
				mMousePick = base.Entity.GetGameObject().GetComponent<MousePickable>();
				if (mMousePick == null)
				{
					CommonCmpt cmpt = base.Entity.GetCmpt<CommonCmpt>();
					if (null == cmpt || cmpt.entityProto.proto == EEntityProto.Npc || cmpt.entityProto.proto == EEntityProto.RandomNpc)
					{
						mMousePick = base.Entity.GetGameObject().AddComponent<MousePickableNPC>();
					}
					else
					{
						mMousePick = base.Entity.GetGameObject().AddComponent<MousePickablePeEntity>();
					}
				}
			}
			return mMousePick;
		}
	}

	void IPeMsg.OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
			mousePickable.CollectColliders();
			break;
		case EMsg.View_Prefab_Destroy:
			mousePickable.ClearCollider();
			break;
		}
	}
}
