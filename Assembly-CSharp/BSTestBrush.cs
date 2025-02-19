using UnityEngine;

public class BSTestBrush : BSBrush
{
	public GameObject Gizmo;

	public bool IsVoxel = true;

	private BSMath.DrawTarget m_Target;

	private BSGLNearVoxelIndicator m_Indicator;

	private BSGLNearBlockIndicator m_BlockIndicator;

	public int BrushSize = 1;

	private void Awake()
	{
		m_Indicator = base.gameObject.GetComponent<BSGLNearVoxelIndicator>();
		m_BlockIndicator = base.gameObject.GetComponent<BSGLNearBlockIndicator>();
	}

	private void Update()
	{
		if (VFVoxelTerrain.self == null)
		{
			return;
		}
		if (IsVoxel)
		{
			Gizmo.transform.localScale = new Vector3(BrushSize, BrushSize, BrushSize);
			m_Indicator.enabled = true;
			m_BlockIndicator.enabled = false;
		}
		else
		{
			m_Indicator.enabled = false;
			m_BlockIndicator.enabled = true;
			Gizmo.transform.localScale = new Vector3(0.5f * (float)BrushSize, 0.5f * (float)BrushSize, 0.5f * (float)BrushSize);
			m_BlockIndicator.m_Expand = 1 + BrushSize;
		}
		IBSDataSource iBSDataSource = null;
		BSVoxel voxel;
		if (IsVoxel)
		{
			voxel = default(BSVoxel);
			voxel.value0 = byte.MaxValue;
			voxel.value1 = 2;
			iBSDataSource = BuildingMan.Voxels;
		}
		else
		{
			voxel = default(BSVoxel);
			voxel.value0 = B45Block.MakeBlockType(1, 0);
			voxel.value1 = 0;
			iBSDataSource = BuildingMan.Blocks;
		}
		if (!BSMath.RayCastDrawTarget(Camera.main.ScreenPointToRay(Input.mousePosition), iBSDataSource, out m_Target, 128, ignoreDiagonal: false, BuildingMan.Datas))
		{
			return;
		}
		IntVector3 intVector = new IntVector3(Mathf.FloorToInt(m_Target.cursor.x * (float)iBSDataSource.ScaleInverted), Mathf.FloorToInt(m_Target.cursor.y * (float)iBSDataSource.ScaleInverted), Mathf.FloorToInt(m_Target.cursor.z * (float)iBSDataSource.ScaleInverted));
		float num = (float)Mathf.FloorToInt((float)BrushSize * 0.5f) * iBSDataSource.Scale;
		Vector3 vector = intVector.ToVector3() * iBSDataSource.Scale - new Vector3(num, num, num);
		int num2 = ((BrushSize % 2 == 0) ? 1 : 0);
		if (num != 0f)
		{
			Vector3 zero = Vector3.zero;
			if (m_Target.rch.normal.x > 0f)
			{
				zero.x += num;
			}
			else if (m_Target.rch.normal.x < 0f)
			{
				zero.x -= num - iBSDataSource.Scale * (float)num2;
			}
			else
			{
				zero.x = 0f;
			}
			if (m_Target.rch.normal.y > 0f)
			{
				zero.y += num;
			}
			else if (m_Target.rch.normal.y < 0f)
			{
				zero.y -= num - iBSDataSource.Scale * (float)num2;
			}
			else
			{
				zero.y = 0f;
			}
			if (m_Target.rch.normal.z > 0f)
			{
				zero.z += num;
			}
			else if (m_Target.rch.normal.z < 0f)
			{
				zero.z -= num - iBSDataSource.Scale * (float)num2;
			}
			else
			{
				zero.z = 0f;
			}
			vector += zero;
		}
		Gizmo.transform.position = vector + iBSDataSource.Offset;
		if (!Input.GetMouseButtonDown(0))
		{
			return;
		}
		int brushSize = BrushSize;
		for (int i = 0; i < brushSize; i++)
		{
			for (int j = 0; j < brushSize; j++)
			{
				for (int k = 0; k < brushSize; k++)
				{
					IntVector3 intVector2 = new IntVector3(Mathf.FloorToInt(vector.x * (float)iBSDataSource.ScaleInverted) + i, Mathf.FloorToInt(vector.y * (float)iBSDataSource.ScaleInverted) + j, Mathf.FloorToInt(vector.z * (float)iBSDataSource.ScaleInverted) + k);
					iBSDataSource.Write(voxel, intVector2.x, intVector2.y, intVector2.z);
				}
			}
		}
	}

	protected override void Do()
	{
	}
}
