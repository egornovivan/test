using UnityEngine;

namespace Pathea;

public class RobotCmpt : PeCmpt, IPeMsg
{
	private PEBarrelController m_Barrel;

	public void Translate(Vector3 pos)
	{
		Vector3 position = pos;
		position.y += 5f;
		base.Entity.peTrans.position = position;
	}

	private new void Start()
	{
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
			BiologyViewRoot biologyViewRoot = (BiologyViewRoot)args[1];
			m_Barrel = biologyViewRoot.barrelController;
		}
	}
}
