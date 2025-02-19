namespace Transvoxel.SurfaceExtractor;

internal class ReuseCell
{
	public readonly int[] Verts;

	public int CaseIndex;

	public ReuseCell(int size)
	{
		CaseIndex = 0;
		Verts = new int[size];
		for (int i = 0; i < size; i++)
		{
			Verts[i] = -1;
		}
	}
}
