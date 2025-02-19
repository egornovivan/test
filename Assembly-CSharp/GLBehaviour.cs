using UnityEngine;

public abstract class GLBehaviour : MonoBehaviour
{
	public int m_RenderOrder;

	public Material m_Material;

	public abstract void OnGL();
}
