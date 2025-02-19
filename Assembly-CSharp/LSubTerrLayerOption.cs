using System;

[Serializable]
public class LSubTerrLayerOption
{
	public string Name = string.Empty;

	public float MinTreeHeight;

	public float MaxTreeHeight = 10000f;

	public GraphicOptionGroup BillboardDist;

	public GraphicOptionGroup BillboardFadeLen;
}
