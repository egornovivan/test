public class VFTile
{
	public byte[][][] terraVoxels;

	public byte[][][] waterVoxels;

	public int[][] nTerraYLens;

	public int[][] nWaterYLens;

	public int tileX;

	public int tileZ;

	public int tileL;

	public int tileH;

	public static int DataLenZ => 35;

	public static int DataLenX => 35;

	public int MaxDataLenY => (tileH + 1) * 2;

	public VFTile(int lod, int maxHight)
	{
		tileX = int.MinValue;
		tileZ = int.MinValue;
		tileL = lod;
		tileH = maxHight;
		terraVoxels = new byte[DataLenZ][][];
		waterVoxels = new byte[DataLenZ][][];
		nTerraYLens = new int[DataLenZ][];
		nWaterYLens = new int[DataLenZ][];
		int maxDataLenY = MaxDataLenY;
		for (int i = 0; i < DataLenZ; i++)
		{
			terraVoxels[i] = new byte[DataLenX][];
			waterVoxels[i] = new byte[DataLenX][];
			nTerraYLens[i] = new int[DataLenX];
			nWaterYLens[i] = new int[DataLenX];
			for (int j = 0; j < DataLenX; j++)
			{
				terraVoxels[i][j] = new byte[maxDataLenY];
				waterVoxels[i][j] = new byte[maxDataLenY];
			}
		}
	}

	public static int DataSize(int h)
	{
		return DataLenX * (h + 1) * DataLenZ;
	}
}
