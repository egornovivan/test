using Pathea;
using UnityEngine;

public abstract class OperatableItem : MousePickableChildCollider
{
	private const int SqrOperateMaxDistance = 36;

	[SerializeField]
	protected int m_id;

	protected override string tipsText => PELocalization.GetString(8000141);

	public virtual bool Operate()
	{
		return true;
	}

	public virtual bool Init(int id)
	{
		m_id = id;
		return true;
	}

	public override string ToString()
	{
		return "[OperatableItem:" + m_id + "]";
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) || PeInput.Get(PeInput.LogicFunction.OpenItemMenu))
		{
			Operate();
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		PeEntity componentInParent = base.gameObject.GetComponentInParent<PeEntity>();
		if (null != componentInParent)
		{
			Init(componentInParent.Id);
		}
	}
}
