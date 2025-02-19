using System.Collections.Generic;
using UnityEngine;

public class VoxelEditor : MonoBehaviour
{
	private const int m_shift = 5;

	private const int m_voxelNumPerChunkMask = 31;

	private const int m_SubTerCellSize = 32;

	public VFVoxelTerrain m_voxelTerrain;

	public GameObject[] m_treePrototypeList;

	public float[] m_treePrototypeBendfactor;

	public float[] m_treePrototypeSizes;

	public List<string> m_treePrototypeName;

	public static VoxelEditor m_this;

	public static VoxelEditor Get()
	{
		return m_this;
	}

	private void Init()
	{
		m_voxelTerrain = VFVoxelTerrain.self;
		if (m_voxelTerrain != null)
		{
			base.gameObject.transform.parent = m_voxelTerrain.gameObject.transform;
		}
	}

	private void Awake()
	{
		m_this = this;
		if (GameConfig.IsMultiMode)
		{
			base.enabled = false;
		}
		else
		{
			base.enabled = true;
		}
	}

	private void Start()
	{
		Init();
		m_treePrototypeName = new List<string>();
	}

	private VFVoxel[,,] ReadVoxels(IntVector3 _centerIdx, int _alterRadius, int _filterSize)
	{
		int num = _alterRadius + _filterSize;
		int num2 = 2 * num + 1;
		VFVoxel[,,] array = new VFVoxel[num2, num2, num2];
		IVxDataSource voxels = m_voxelTerrain.Voxels;
		IntVector3 intVector = new IntVector3();
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				for (int k = -num; k <= num; k++)
				{
					int num3 = _centerIdx.x + i;
					int num4 = _centerIdx.y + j;
					int num5 = _centerIdx.z + k;
					if (num3 >= 0 && num4 >= 0 && num5 >= 0)
					{
						intVector.x = num3 >> 5;
						intVector.y = num4 >> 5;
						intVector.z = num5 >> 5;
						VFVoxelChunkData vFVoxelChunkData = voxels.readChunk(intVector.x, intVector.y, intVector.z);
						num3 &= 0x1F;
						num4 &= 0x1F;
						num5 &= 0x1F;
						array[i + num, j + num, k + num] = vFVoxelChunkData.ReadVoxelAtIdx(num3, num4, num5);
					}
				}
			}
		}
		return array;
	}

	public void AutoFillVegetation()
	{
	}
}
