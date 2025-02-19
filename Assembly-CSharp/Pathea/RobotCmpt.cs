using UnityEngine;

namespace Pathea;

public class RobotCmpt : PeCmpt, IPeMsg
{
	public void Translate(Vector3 pos)
	{
		Vector3 position = pos;
		position.y += 5f;
		base.Entity.peTrans.position = position;
	}

	private void Update()
	{
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Object.DestroyObject(base.gameObject);
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		if (msg == EMsg.View_Prefab_Build)
		{
		}
	}
}
