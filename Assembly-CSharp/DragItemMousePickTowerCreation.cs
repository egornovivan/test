using UnityEngine;
using WhiteCat;

public class DragItemMousePickTowerCreation : DragItemMousePickCreation
{
	private AITurretController _controller;

	protected override string tipsText
	{
		get
		{
			string text = string.Empty;
			if (_controller != null)
			{
				if (base.itemObj != null && base.itemObj.protoData != null)
				{
					text = "[5CB0FF]" + base.itemObj.protoData.name + "[-]\n" + PELocalization.GetString(8000129);
				}
				string text2 = text;
				text = text2 + "\n" + Mathf.FloorToInt(_controller.energy) + "/" + Mathf.FloorToInt(_controller.maxEnergy);
				text += $"\n{PELocalization.GetString(82220001)} {Mathf.FloorToInt(_controller.hp)}/{Mathf.FloorToInt(_controller.maxHp)}";
			}
			return text;
		}
	}

	public void Init(AITurretController controller)
	{
		_controller = controller;
	}

	protected override void InitCmd(CmdList cmdList)
	{
		base.InitCmd(cmdList);
		if (base.itemObj != null && base.itemObj.protoData != null && base.itemObj.protoData.unchargeable)
		{
			cmdList.Add("Refill", OnRefill);
		}
	}

	private void OnRefill()
	{
		int ammoNum = GameUI.Instance.mItemOp.AmmoNum;
		if (ammoNum > 0 && !(_controller == null) && base.pkg != null)
		{
			int maxRefillNum = GetMaxRefillNum();
			if (maxRefillNum < ammoNum)
			{
				GameUI.Instance.mItemOp.SetRefill(Mathf.FloorToInt(_controller.energy), Mathf.FloorToInt(_controller.maxEnergy), GetMaxRefillNum());
			}
			else if (base.pkg.Destroy(_controller.bulletProtoId, ammoNum))
			{
				_controller.SetEnergy(_controller.energy + (float)ammoNum);
				GameUI.Instance.mItemOp.SetRefill(Mathf.FloorToInt(_controller.energy), Mathf.FloorToInt(_controller.maxEnergy), GetMaxRefillNum());
			}
		}
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && CanCmd() && base.itemObj != null && base.itemObj.protoData != null && base.itemObj.protoData.unchargeable)
		{
			GameUI.Instance.mItemOp.SetRefill(Mathf.FloorToInt(_controller.energy), Mathf.FloorToInt(_controller.maxEnergy), GetMaxRefillNum());
		}
	}

	public int GetMaxRefillNum()
	{
		if (_controller == null)
		{
			return 0;
		}
		if (base.pkg == null)
		{
			return 0;
		}
		int count = base.pkg.GetCount(_controller.bulletProtoId);
		int num = Mathf.FloorToInt(_controller.maxEnergy) - Mathf.FloorToInt(_controller.energy);
		return (num <= count) ? num : count;
	}
}
