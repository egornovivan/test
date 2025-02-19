using System.Collections.Generic;
using UnityEngine;

public class GLSelectionBoxes : GLBehaviour
{
	public List<SelBox> m_Boxes;

	public Gradient m_LineColor;

	public Gradient m_BoxColor;

	private void Start()
	{
		m_Boxes = new List<SelBox>();
	}

	public override void OnGL()
	{
		foreach (SelBox box2 in m_Boxes)
		{
			IntBox box = box2.m_Box;
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			bounds.SetMinMax(new Vector3((float)box.xMin - 0.05f, (float)box.yMin - 0.05f, (float)box.zMin - 0.05f) * VCEditor.s_Scene.m_Setting.m_VoxelSize, new Vector3((float)box.xMax + 1.05f, (float)box.yMax + 1.05f, (float)box.zMax + 1.05f) * VCEditor.s_Scene.m_Setting.m_VoxelSize);
			float num = (float)(int)box2.m_Val / 255f;
			if (num > 0.99f)
			{
				GL.Begin(1);
				GL.Color(m_LineColor.Evaluate(num));
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
			}
			GL.Begin(7);
			GL.Color(m_BoxColor.Evaluate(num));
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
