using UnityEngine;

public class GLNearVoxelIndicator : GLBehaviour
{
	public IntVector3 m_Center = InvalidPos;

	public int m_Expand = 4;

	public Gradient m_BoxColors;

	public static IntVector3 InvalidPos => new IntVector3(-100, -100, -100);

	private void Update()
	{
		VCEMath.DrawTarget target;
		if (VCEInput.s_MouseOnUI)
		{
			m_Center = InvalidPos;
		}
		else if (VCEMath.RayCastDrawTarget(VCEInput.s_PickRay, out target, 1))
		{
			m_Center.x = target.snapto.x;
			m_Center.y = target.snapto.y;
			m_Center.z = target.snapto.z;
		}
		else
		{
			m_Center = InvalidPos;
		}
	}

	private void OnDisable()
	{
		m_Center = InvalidPos;
	}

	public override void OnGL()
	{
		if (m_Center.x < -50)
		{
			return;
		}
		for (int i = -m_Expand; i <= m_Expand; i++)
		{
			for (int j = -m_Expand; j <= m_Expand; j++)
			{
				for (int k = -m_Expand; k <= m_Expand; k++)
				{
					IntVector3 intVector = new IntVector3(i + m_Center.x, j + m_Center.y, k + m_Center.z);
					if (VCEditor.s_Scene.m_IsoData.GetVoxel(VCIsoData.IPosToKey(intVector)).Volume > 0)
					{
						IntBox intBox = default(IntBox);
						intBox.xMin = (short)intVector.x;
						intBox.xMax = (short)intVector.x;
						intBox.yMin = (short)intVector.y;
						intBox.yMax = (short)intVector.y;
						intBox.zMin = (short)intVector.z;
						intBox.zMax = (short)intVector.z;
						Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
						bounds.SetMinMax(new Vector3((float)intBox.xMin - 0.03f, (float)intBox.yMin - 0.03f, (float)intBox.zMin - 0.03f) * VCEditor.s_Scene.m_Setting.m_VoxelSize, new Vector3((float)intBox.xMax + 1.03f, (float)intBox.yMax + 1.03f, (float)intBox.zMax + 1.03f) * VCEditor.s_Scene.m_Setting.m_VoxelSize);
						float time = (float)Mathf.Max(Mathf.Abs(i), Mathf.Abs(j), Mathf.Abs(k)) / (float)m_Expand;
						Color color = m_BoxColors.Evaluate(time);
						Color c = color;
						color.a *= 1.5f;
						GL.Begin(1);
						GL.Color(color);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
						GL.End();
						GL.Begin(7);
						GL.Color(c);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.min.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.min.z);
						GL.Vertex3(bounds.min.x, bounds.min.y, bounds.max.z);
						GL.Vertex3(bounds.min.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.max.y, bounds.max.z);
						GL.Vertex3(bounds.max.x, bounds.min.y, bounds.max.z);
						GL.End();
					}
				}
			}
		}
	}
}
