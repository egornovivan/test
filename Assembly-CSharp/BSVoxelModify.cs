using UnityEngine;

public class BSVoxelModify : BSModify
{
	public delegate bool DNotify(int opType, IntVector3[] index, BSVoxel[] voxel, BSVoxel[] oldvoxel, EBSBrushMode mode, IBSDataSource dt);

	private IBSDataSource dataSource;

	private EBSBrushMode m_Mode;

	private BSVoxel[] m_OldVoxels;

	private IntVector3[] m_Index;

	private BSVoxel[] m_NewVoxels;

	private Bounds m_Bound;

	public Bounds Bound => m_Bound;

	public static event DNotify onModifyCheck;

	public static event DNotify onAfterDo;

	public BSVoxelModify(IntVector3[] index, BSVoxel[] old_voxels, BSVoxel[] new_voxels, IBSDataSource ds, EBSBrushMode mode)
	{
		m_OldVoxels = old_voxels;
		m_Index = index;
		m_NewVoxels = new_voxels;
		dataSource = ds;
		m_Mode = mode;
		if (index.Length <= 0)
		{
			return;
		}
		Vector3 vector = new Vector3((float)index[0].x * dataSource.Scale, (float)index[0].y * dataSource.Scale, (float)index[0].z * dataSource.Scale);
		Vector3 max = vector;
		m_Bound = default(Bounds);
		foreach (IntVector3 intVector in index)
		{
			Vector3 vector2 = new Vector3((float)intVector.x * dataSource.Scale, (float)intVector.y * dataSource.Scale, (float)intVector.z * dataSource.Scale);
			if (vector.x > vector2.x)
			{
				vector.x = vector2.x;
			}
			else if (max.x < vector2.x)
			{
				max.x = vector2.x;
			}
			if (vector.y > vector2.y)
			{
				vector.y = vector2.y;
			}
			else if (max.y < vector2.y)
			{
				max.y = vector2.y;
			}
			if (vector.z > vector2.z)
			{
				vector.z = vector2.z;
			}
			else if (max.z < vector2.z)
			{
				max.z = vector2.z;
			}
		}
		m_Bound.min = vector;
		m_Bound.max = max;
	}

	public override bool Redo()
	{
		if (m_Mode == EBSBrushMode.Add)
		{
			if (BSVoxelModify.onModifyCheck == null || BSVoxelModify.onModifyCheck(1, m_Index, m_NewVoxels, m_OldVoxels, m_Mode, dataSource))
			{
				for (int i = 0; i < m_Index.Length; i++)
				{
					dataSource.Write(m_NewVoxels[i], m_Index[i].x, m_Index[i].y, m_Index[i].z);
				}
				if (BSVoxelModify.onAfterDo != null)
				{
					BSVoxelModify.onAfterDo(1, m_Index, m_NewVoxels, m_OldVoxels, m_Mode, dataSource);
				}
				return true;
			}
			return false;
		}
		if (m_Mode == EBSBrushMode.Subtract)
		{
			if (BSVoxelModify.onModifyCheck == null || BSVoxelModify.onModifyCheck(1, m_Index, m_NewVoxels, m_OldVoxels, m_Mode, dataSource))
			{
				for (int j = 0; j < m_Index.Length; j++)
				{
					dataSource.Write(m_NewVoxels[j], m_Index[j].x, m_Index[j].y, m_Index[j].z);
				}
				if (BSVoxelModify.onAfterDo != null)
				{
					BSVoxelModify.onAfterDo(1, m_Index, m_NewVoxels, m_OldVoxels, m_Mode, dataSource);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public override bool Undo()
	{
		if (m_Mode == EBSBrushMode.Add)
		{
			if (BSVoxelModify.onModifyCheck == null || BSVoxelModify.onModifyCheck(0, m_Index, m_OldVoxels, m_NewVoxels, EBSBrushMode.Subtract, dataSource))
			{
				for (int i = 0; i < m_Index.Length; i++)
				{
					dataSource.Write(m_OldVoxels[i], m_Index[i].x, m_Index[i].y, m_Index[i].z);
				}
				if (BSVoxelModify.onAfterDo != null)
				{
					BSVoxelModify.onAfterDo(0, m_Index, m_OldVoxels, m_NewVoxels, m_Mode, dataSource);
				}
				return true;
			}
			return false;
		}
		if (m_Mode == EBSBrushMode.Subtract)
		{
			if (BSVoxelModify.onModifyCheck == null || BSVoxelModify.onModifyCheck(0, m_Index, m_OldVoxels, m_NewVoxels, EBSBrushMode.Add, dataSource))
			{
				for (int j = 0; j < m_Index.Length; j++)
				{
					dataSource.Write(m_OldVoxels[j], m_Index[j].x, m_Index[j].y, m_Index[j].z);
				}
				if (BSVoxelModify.onAfterDo != null)
				{
					BSVoxelModify.onAfterDo(0, m_Index, m_OldVoxels, m_NewVoxels, m_Mode, dataSource);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public void Do()
	{
		Redo();
	}

	public override bool IsNull()
	{
		Vector3 min = m_Bound.min;
		Vector3 max = m_Bound.max;
		Vector3 min2 = dataSource.Lod0Bound.min;
		Vector3 max2 = dataSource.Lod0Bound.max;
		if (min.x < min2.x || max.x > max2.x || min.y < min2.y || max.y > max2.y || min.z < min2.z || max.z > max2.z)
		{
			return true;
		}
		return m_OldVoxels.Length == 0 && m_NewVoxels.Length == 0;
	}
}
