using System.Collections.Generic;
using ItemAsset;
using Pathea;

public class PeTipsMsgMan : MonoLikeSingleton<PeTipsMsgMan>
{
	public delegate void DNotify(PeTipMsg msg);

	public const int c_MaxTipsMsgCnt = 20;

	private List<PeTipMsg> m_TipsMsg;

	private bool _playerEventAdded;

	public event DNotify onAddTipMsg;

	public void AddTipMsg(PeTipMsg msg)
	{
		m_TipsMsg.Add(msg);
		if (this.onAddTipMsg != null)
		{
			this.onAddTipMsg(msg);
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_TipsMsg = new List<PeTipMsg>();
	}

	public override void Update()
	{
		base.Update();
		if (!_playerEventAdded && PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			if (!(cmpt == null))
			{
				cmpt.getItemEventor.Subscribe(OnPlayerItemPackageEvent);
				_playerEventAdded = true;
			}
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnPlayerItemPackageEvent(object sender, PlayerPackageCmpt.GetItemEventArg e)
	{
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (!(cmpt == null))
		{
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(e.protoId);
			if (itemProto != null)
			{
				string content = $"{itemProto.GetName()} X {e.count} ({cmpt.GetItemCount(e.protoId)})";
				new PeTipMsg(content, itemProto.icon[0], PeTipMsg.EMsgLevel.Norm);
			}
		}
	}
}
