using UnityEngine;

public class VCEShowInEditor : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Renderer>().enabled = false;
	}

	private void Update()
	{
		VCEComponentTool componentOrOnParent = VCUtils.GetComponentOrOnParent<VCEComponentTool>(base.gameObject);
		if (componentOrOnParent != null)
		{
			GetComponent<Renderer>().enabled = componentOrOnParent.m_InEditor;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
