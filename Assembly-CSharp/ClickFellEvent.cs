using UnityEngine;

public class ClickFellEvent : MousePickableChildCollider
{
	[SerializeField]
	private float m_TreeDefaultOpDis = 2f;

	protected override void OnStart()
	{
		operateDistance = m_TreeDefaultOpDis;
		base.OnStart();
	}

	protected override void CheckOperate()
	{
	}
}
