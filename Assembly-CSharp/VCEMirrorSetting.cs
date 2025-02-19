using UnityEngine;

public class VCEMirrorSetting
{
	public bool m_XPlane;

	public bool m_YPlane;

	public bool m_ZPlane;

	public bool m_XYAxis;

	public bool m_YZAxis;

	public bool m_ZXAxis;

	public bool m_Point;

	public byte m_Mask = 7;

	public float m_PosX;

	public float m_PosY;

	public float m_PosZ;

	private float m_VoxelPosX;

	private float m_VoxelPosY;

	private float m_VoxelPosZ;

	private int m_ColorPosX;

	private int m_ColorPosY;

	private int m_ColorPosZ;

	private float m_WorldPosX;

	private float m_WorldPosY;

	private float m_WorldPosZ;

	public IntVector3[] Output = new IntVector3[8];

	public VCComponentData[] ComponentOutput = new VCComponentData[8];

	public int OutputCnt;

	public bool XPlane_Masked => m_XPlane && (m_Mask & 1) == 1;

	public bool YPlane_Masked => m_YPlane && (m_Mask & 2) == 2;

	public bool ZPlane_Masked => m_ZPlane && (m_Mask & 4) == 4;

	public bool XYAxis_Masked => m_XYAxis && (m_Mask & 3) == 3;

	public bool YZAxis_Masked => m_YZAxis && (m_Mask & 6) == 6;

	public bool ZXAxis_Masked => m_ZXAxis && (m_Mask & 5) == 5;

	public bool Point_Masked => m_Point && (m_Mask & 7) == 7;

	public bool Enabled => m_XPlane || m_YPlane || m_ZPlane || m_XYAxis || m_YZAxis || m_ZXAxis || m_Point;

	public bool Enabled_Masked => XPlane_Masked || YPlane_Masked || ZPlane_Masked || XYAxis_Masked || YZAxis_Masked || ZXAxis_Masked || Point_Masked;

	public int MirrorCount => (m_XPlane ? 1 : 0) + (m_YPlane ? 1 : 0) + (m_ZPlane ? 1 : 0) + (m_XYAxis ? 1 : 0) + (m_YZAxis ? 1 : 0) + (m_ZXAxis ? 1 : 0) + (m_Point ? 1 : 0);

	public int MirrorCount_Masked => (XPlane_Masked ? 1 : 0) + (YPlane_Masked ? 1 : 0) + (ZPlane_Masked ? 1 : 0) + (XYAxis_Masked ? 1 : 0) + (YZAxis_Masked ? 1 : 0) + (ZXAxis_Masked ? 1 : 0) + (Point_Masked ? 1 : 0);

	public Vector3 WorldPos => new Vector3(m_WorldPosX, m_WorldPosY, m_WorldPosZ);

	public void Reset(int size_x, int size_y, int size_z)
	{
		m_XPlane = false;
		m_YPlane = false;
		m_ZPlane = false;
		m_XYAxis = false;
		m_YZAxis = false;
		m_ZXAxis = false;
		m_Point = false;
		m_Mask = 7;
		m_PosX = (float)size_x * 0.5f;
		m_PosY = (float)size_y * 0.5f;
		m_PosZ = (float)size_z * 0.5f;
		if (m_PosY > 50f)
		{
			m_PosY = 50f;
		}
	}

	public void Validate()
	{
		if (m_Point)
		{
			m_XPlane = (m_YPlane = (m_ZPlane = false));
			m_XYAxis = (m_YZAxis = (m_ZXAxis = false));
		}
		if (m_XYAxis)
		{
			m_XPlane = (m_YPlane = false);
		}
		if (m_YZAxis)
		{
			m_YPlane = (m_ZPlane = false);
		}
		if (m_ZXAxis)
		{
			m_ZPlane = (m_XPlane = false);
		}
	}

	public void CalcPrepare(float voxel_size)
	{
		Validate();
		m_VoxelPosX = m_PosX - 0.5f;
		m_VoxelPosY = m_PosY - 0.5f;
		m_VoxelPosZ = m_PosZ - 0.5f;
		m_ColorPosX = Mathf.RoundToInt(m_PosX * 2f + 1f);
		m_ColorPosY = Mathf.RoundToInt(m_PosY * 2f + 1f);
		m_ColorPosZ = Mathf.RoundToInt(m_PosZ * 2f + 1f);
		m_WorldPosX = m_PosX * voxel_size;
		m_WorldPosY = m_PosY * voxel_size;
		m_WorldPosZ = m_PosZ * voxel_size;
		for (int i = 0; i < 8; i++)
		{
			Output[i] = new IntVector3();
			ComponentOutput[i] = null;
		}
	}

	public void MirrorVoxel(IntVector3 ipos)
	{
		Output[0].x = ipos.x;
		Output[0].y = ipos.y;
		Output[0].z = ipos.z;
		OutputCnt = 1;
		if (m_XPlane)
		{
			MirrorVoxelOnce(1);
		}
		if (m_YPlane)
		{
			MirrorVoxelOnce(2);
		}
		if (m_ZPlane)
		{
			MirrorVoxelOnce(4);
		}
		if (m_XYAxis)
		{
			MirrorVoxelOnce(3);
		}
		if (m_YZAxis)
		{
			MirrorVoxelOnce(6);
		}
		if (m_ZXAxis)
		{
			MirrorVoxelOnce(5);
		}
		if (m_Point)
		{
			MirrorVoxelOnce(7);
		}
	}

	public void MirrorColor(IntVector3 ipos)
	{
		Output[0].x = ipos.x;
		Output[0].y = ipos.y;
		Output[0].z = ipos.z;
		OutputCnt = 1;
		if (m_XPlane)
		{
			MirrorColorOnce(1);
		}
		if (m_YPlane)
		{
			MirrorColorOnce(2);
		}
		if (m_ZPlane)
		{
			MirrorColorOnce(4);
		}
		if (m_XYAxis)
		{
			MirrorColorOnce(3);
		}
		if (m_YZAxis)
		{
			MirrorColorOnce(6);
		}
		if (m_ZXAxis)
		{
			MirrorColorOnce(5);
		}
		if (m_Point)
		{
			MirrorColorOnce(7);
		}
	}

	public void MirrorComponent(VCComponentData cdata)
	{
		ComponentOutput[0] = cdata.Copy();
		OutputCnt = 1;
		if (XPlane_Masked)
		{
			MirrorComponentOnce(1);
		}
		if (YPlane_Masked)
		{
			MirrorComponentOnce(2);
		}
		if (ZPlane_Masked)
		{
			MirrorComponentOnce(4);
		}
		if (XYAxis_Masked)
		{
			MirrorComponentOnce(3);
		}
		if (YZAxis_Masked)
		{
			MirrorComponentOnce(6);
		}
		if (ZXAxis_Masked)
		{
			MirrorComponentOnce(5);
		}
		if (Point_Masked)
		{
			MirrorComponentOnce(7);
		}
	}

	private void MirrorVoxelOnce(int mirror)
	{
		for (int i = 0; i < OutputCnt; i++)
		{
			Output[i + OutputCnt].x = (((mirror & 1) <= 0) ? Output[i].x : ((int)(2f * m_VoxelPosX + 0.001f) - Output[i].x));
			Output[i + OutputCnt].y = (((mirror & 2) <= 0) ? Output[i].y : ((int)(2f * m_VoxelPosY + 0.001f) - Output[i].y));
			Output[i + OutputCnt].z = (((mirror & 4) <= 0) ? Output[i].z : ((int)(2f * m_VoxelPosZ + 0.001f) - Output[i].z));
		}
		OutputCnt <<= 1;
	}

	private void MirrorColorOnce(int mirror)
	{
		for (int i = 0; i < OutputCnt; i++)
		{
			Output[i + OutputCnt].x = (((mirror & 1) <= 0) ? Output[i].x : ((m_ColorPosX << 1) - Output[i].x));
			Output[i + OutputCnt].y = (((mirror & 2) <= 0) ? Output[i].y : ((m_ColorPosY << 1) - Output[i].y));
			Output[i + OutputCnt].z = (((mirror & 4) <= 0) ? Output[i].z : ((m_ColorPosZ << 1) - Output[i].z));
		}
		OutputCnt <<= 1;
	}

	private void MirrorComponentOnce(int mirror)
	{
		for (int i = 0; i < OutputCnt; i++)
		{
			VCComponentData vCComponentData = ComponentOutput[i];
			VCComponentData vCComponentData2 = ComponentOutput[i].Copy();
			ComponentOutput[i + OutputCnt] = vCComponentData2;
			vCComponentData2.m_Position.x = (((mirror & 1) <= 0) ? vCComponentData.m_Position.x : (2f * m_WorldPosX - vCComponentData.m_Position.x));
			vCComponentData2.m_Position.y = (((mirror & 2) <= 0) ? vCComponentData.m_Position.y : (2f * m_WorldPosY - vCComponentData.m_Position.y));
			vCComponentData2.m_Position.z = (((mirror & 4) <= 0) ? vCComponentData.m_Position.z : (2f * m_WorldPosZ - vCComponentData.m_Position.z));
			bool flag = true;
			if (vCComponentData2 is VCPartData)
			{
				flag = VCConfig.s_Parts[vCComponentData2.m_ComponentId].m_Symmetric == 1;
			}
			if (vCComponentData2 is VCDecalData)
			{
				flag = true;
			}
			if (flag)
			{
				if ((mirror & 1) > 0)
				{
					vCComponentData2.m_Rotation.y = 0f - vCComponentData2.m_Rotation.y;
					vCComponentData2.m_Rotation.z = 0f - vCComponentData2.m_Rotation.z;
					if (vCComponentData2 is IVCMultiphaseComponentData)
					{
						(vCComponentData2 as IVCMultiphaseComponentData).InversePhase();
					}
				}
				if ((mirror & 2) > 0)
				{
					vCComponentData2.m_Rotation.x = 0f - vCComponentData2.m_Rotation.x;
					vCComponentData2.m_Rotation.z = 180f - vCComponentData2.m_Rotation.z;
					if (vCComponentData2 is IVCMultiphaseComponentData)
					{
						(vCComponentData2 as IVCMultiphaseComponentData).InversePhase();
					}
				}
				if ((mirror & 4) > 0)
				{
					vCComponentData2.m_Rotation.y = 180f - vCComponentData2.m_Rotation.y;
					vCComponentData2.m_Rotation.z = 0f - vCComponentData2.m_Rotation.z;
					if (vCComponentData2 is IVCMultiphaseComponentData)
					{
						(vCComponentData2 as IVCMultiphaseComponentData).InversePhase();
					}
				}
			}
			else if ((mirror & 1) > 0 && vCComponentData2 is IVCMultiphaseComponentData)
			{
				(vCComponentData2 as IVCMultiphaseComponentData).InversePhase();
			}
			vCComponentData2.m_Rotation = VCEMath.NormalizeEulerAngle(vCComponentData2.m_Rotation);
		}
		OutputCnt <<= 1;
	}
}
