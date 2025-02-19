using Pathea;
using UnityEngine;

public class MousePickableNPC : MousePickablePeEntity
{
	private Vector3 mainPlayerPos => (!(null != PeSingleton<PeCreature>.Instance.mainPlayer)) ? Vector3.zero : PeSingleton<PeCreature>.Instance.mainPlayer.position;

	public NpcCmpt npc { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		npc = GetComponent<NpcCmpt>();
		operateDistance = 4.5f;
	}

	protected override void CheckOperate()
	{
		if (null != npc && npc.CanHanded)
		{
			if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && PeSingleton<PeCreature>.Instance != null && null != PeSingleton<PeCreature>.Instance.mainPlayer)
			{
				MotionMgrCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
				if (null != cmpt)
				{
					PEActionParamN param = PEActionParamN.param;
					param.n = npc.Entity.Id;
					cmpt.DoAction(PEActionType.Hand, param);
				}
			}
		}
		else
		{
			base.CheckOperate();
		}
	}
}
