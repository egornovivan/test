using System;
using Pathea;

public class MousePickablePeEntity : MousePickable
{
	protected override string tipsText => null;

	public event Action<MousePickablePeEntity> pickBodyEventor;

	private void Awake()
	{
		base.priority = MousePicker.EPriority.Level3;
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		PeEntity component = GetComponent<PeEntity>();
		if (PeInput.Get(PeInput.LogicFunction.PickBody) && this.pickBodyEventor != null)
		{
			this.pickBodyEventor(this);
		}
		if (PeInput.Get(PeInput.LogicFunction.TalkToNpc))
		{
			EntityMgr.NPCTalkEvent nPCTalkEvent = new EntityMgr.NPCTalkEvent();
			if (null != component)
			{
				nPCTalkEvent.entity = component;
				PeSingleton<EntityMgr>.Instance.npcTalkEventor.Dispatch(nPCTalkEvent);
			}
		}
		if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu))
		{
			EntityMgr.RMouseClickEntityEvent rMouseClickEntityEvent = new EntityMgr.RMouseClickEntityEvent();
			if (null != component)
			{
				rMouseClickEntityEvent.entity = component;
				PeSingleton<EntityMgr>.Instance.eventor.Dispatch(rMouseClickEntityEvent);
			}
		}
		if (MissionManager.Instance != null && PeInput.Get(PeInput.LogicFunction.TalkToNpc) && MissionManager.Instance.HasMission(678) && base.gameObject.name == "scene_Dien_viyus_ship_on01(Clone)")
		{
			if (PeGameMgr.IsMulti)
			{
				MissionManager.Instance.RequestCompleteMission(678);
			}
			else
			{
				MissionManager.Instance.CompleteMission(678);
			}
		}
	}
}
