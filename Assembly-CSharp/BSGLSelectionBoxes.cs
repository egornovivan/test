using System.Collections.Generic;
using BSTools;
using UnityEngine;

public class BSGLSelectionBoxes : GLBehaviour
{
	public List<BSTools.SelBox> m_Boxes;

	public Gradient m_LineColor;

	public Gradient m_BoxColor;

	public float scale;

	public Vector3 offset;

	private void Start()
	{
		m_Boxes = new List<BSTools.SelBox>();
		GlobalGLs.AddGL(this);
	}

	public override void OnGL()
	{
		if (m_Material == null)
		{
			m_Material = new Material(Shader.Find("Lines/Colored Blended"));
			m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		}
		GL.PushMatrix();
		for (int i = 0; i < m_Material.passCount; i++)
		{
			m_Material.SetPass(i);
			foreach (BSTools.SelBox box2 in m_Boxes)
			{
				BSTools.IntBox box = box2.m_Box;
				Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
				bounds.SetMinMax(new Vector3(box.xMin, box.yMin, box.zMin) * scale + offset, new Vector3((float)box.xMax + 1f, (float)box.yMax + 1f, (float)box.zMax + 1f) * scale + offset);
				float num = 0.03f;
				if (Camera.main != null)
				{
					float magnitude = (Camera.main.transform.position - bounds.min).magnitude;
					float magnitude2 = (Camera.main.transform.position - bounds.max).magnitude;
					magnitude2 = ((!(magnitude2 > magnitude)) ? magnitude : magnitude2);
					num = Mathf.Clamp(magnitude2 * 0.002f, 0.03f, 0.1f);
				}
				bounds.min -= new Vector3(num, num, num);
				bounds.max += new Vector3(num, num, num);
				float num2 = (float)(int)box2.m_Val / 255f;
				if (num2 > 0.99f)
				{
					GL.Begin(1);
					GL.Color(m_LineColor.Evaluate(num2));
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
				GL.Color(m_BoxColor.Evaluate(num2));
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
		GL.PopMatrix();
	}
}
