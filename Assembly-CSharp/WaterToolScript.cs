using UnityEngine;

[ExecuteInEditMode]
public class WaterToolScript : MonoBehaviour
{
	public void CreateRiver()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "River";
		gameObject.AddComponent(typeof(MeshFilter));
		gameObject.AddComponent(typeof(MeshRenderer));
		gameObject.AddComponent<AttachedRiverScript>();
		if (base.transform.parent != null)
		{
			gameObject.transform.parent = base.transform.parent;
		}
		AttachedRiverScript attachedRiverScript = (AttachedRiverScript)gameObject.GetComponent("AttachedRiverScript");
		attachedRiverScript.riverObject = gameObject;
		attachedRiverScript.parentTerrain = base.gameObject;
	}
}
