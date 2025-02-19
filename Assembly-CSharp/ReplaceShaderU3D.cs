using UnityEngine;

public class ReplaceShaderU3D : MonoBehaviour
{
	private void Awake()
	{
		Renderer[] componentsInChildren = base.transform.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				Shader shader = CorruptShaderReplacement.FindValidShader(material.shader.name);
				if (shader != null)
				{
					material.shader = shader;
				}
			}
		}
	}
}
