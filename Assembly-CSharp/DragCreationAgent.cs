using ItemAsset;
using UnityEngine;

public class DragCreationAgent : DragArticleAgent, ISceneObjAgent
{
	Vector3 ISceneObjAgent.Pos => base.position;

	public DragCreationAgent()
	{
	}

	public DragCreationAgent(Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id, NetworkInterface net = null)
		: base(drag, pos, scl, rot, id, net)
	{
	}
}
