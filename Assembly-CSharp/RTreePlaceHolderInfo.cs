using UnityEngine;

public class RTreePlaceHolderInfo
{
	public Vector3 m_Offset;

	public float m_HeightScale;

	public float m_WidthScale;

	public Vector3 TerrOffset => new Vector3(m_Offset.x / RSubTerrConstant.TerrainSize.x, m_Offset.y / RSubTerrConstant.TerrainSize.y, m_Offset.z / RSubTerrConstant.TerrainSize.z);

	public RTreePlaceHolderInfo(Vector3 offset, float heightscale, float widthscale)
	{
		m_Offset = offset;
		m_HeightScale = heightscale;
		m_WidthScale = widthscale;
	}
}
