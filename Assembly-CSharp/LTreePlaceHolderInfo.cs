using UnityEngine;

public class LTreePlaceHolderInfo
{
	public Vector3 m_Offset;

	public float m_HeightScale;

	public float m_WidthScale;

	public Vector3 TerrOffset => new Vector3(m_Offset.x / 256f, m_Offset.y / 3000f, m_Offset.z / 256f);

	public LTreePlaceHolderInfo(Vector3 offset, float heightscale, float widthscale)
	{
		m_Offset = offset;
		m_HeightScale = heightscale;
		m_WidthScale = widthscale;
	}
}
