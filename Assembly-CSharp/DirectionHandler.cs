using UnityEngine;

[ExecuteInEditMode]
public class DirectionHandler : MonoBehaviour
{
	public bool RuntimeOnly = true;

	public Transform DirTrans;

	public string PropName;

	public bool FilpDir;

	private void Update()
	{
		if (RuntimeOnly && !Application.isPlaying)
		{
			return;
		}
		if (Application.isPlaying)
		{
			if (GetComponent<Renderer>() != null && GetComponent<Renderer>().material != null && DirTrans != null && !string.IsNullOrEmpty(PropName) && GetComponent<Renderer>().material.HasProperty(PropName))
			{
				Vector3 vector = ((!FilpDir) ? DirTrans.forward : (-DirTrans.forward));
				Vector4 vector2 = GetComponent<Renderer>().material.GetVector(PropName);
				GetComponent<Renderer>().material.SetVector(PropName, new Vector4(vector.x, vector.y, vector.z, vector2.w));
			}
		}
		else if (GetComponent<Renderer>() != null && GetComponent<Renderer>().sharedMaterial != null && DirTrans != null && !string.IsNullOrEmpty(PropName) && GetComponent<Renderer>().sharedMaterial.HasProperty(PropName))
		{
			Vector3 vector3 = ((!FilpDir) ? DirTrans.forward : (-DirTrans.forward));
			Vector4 vector4 = GetComponent<Renderer>().sharedMaterial.GetVector(PropName);
			GetComponent<Renderer>().sharedMaterial.SetVector(PropName, new Vector4(vector3.x, vector3.y, vector3.z, vector4.w));
		}
	}
}
