using UnityEngine;

public class GLMirrorPlane : GLBehaviour
{
	public VCEMirrorGL m_Parent;

	public ECoordPlane m_Plane;

	public Color m_LineColor = Color.white;

	public Color m_PlaneColor = new Color(0.1f, 0.1f, 0.1f, 0.1f);

	private Vector3 max = Vector3.zero;

	public override void OnGL()
	{
		max = VCEditor.s_Scene.m_Setting.EditorWorldSize;
		if (m_Plane == ECoordPlane.ZY)
		{
			GL.Begin(7);
			GL.Color(m_PlaneColor);
			GL.Vertex3(base.transform.localPosition.x, 0f, 0f);
			GL.Vertex3(base.transform.localPosition.x, 0f, max.z);
			GL.Vertex3(base.transform.localPosition.x, max.y, max.z);
			GL.Vertex3(base.transform.localPosition.x, max.y, 0f);
			GL.End();
			GL.Begin(1);
			GL.Color(m_LineColor);
			GL.Vertex3(base.transform.localPosition.x, 0f, 0f);
			GL.Vertex3(base.transform.localPosition.x, 0f, max.z);
			GL.Vertex3(base.transform.localPosition.x, 0f, max.z);
			GL.Vertex3(base.transform.localPosition.x, max.y, max.z);
			GL.Vertex3(base.transform.localPosition.x, max.y, max.z);
			GL.Vertex3(base.transform.localPosition.x, max.y, 0f);
			GL.Vertex3(base.transform.localPosition.x, max.y, 0f);
			GL.Vertex3(base.transform.localPosition.x, 0f, 0f);
			foreach (float y3 in m_Parent.m_Ys)
			{
				float y = y3;
				GL.Color(new Color(1f, 1f, 0.5f, 1f));
				GL.Vertex3(base.transform.localPosition.x, y, 0f);
				GL.Vertex3(base.transform.localPosition.x, y, max.z);
			}
			GL.End();
		}
		else if (m_Plane == ECoordPlane.XZ)
		{
			GL.Begin(7);
			GL.Color(m_PlaneColor);
			GL.Vertex3(0f, base.transform.localPosition.y, 0f);
			GL.Vertex3(0f, base.transform.localPosition.y, max.z);
			GL.Vertex3(max.x, base.transform.localPosition.y, max.z);
			GL.Vertex3(max.x, base.transform.localPosition.y, 0f);
			GL.End();
			GL.Begin(1);
			GL.Color(m_LineColor);
			GL.Vertex3(0f, base.transform.localPosition.y, 0f);
			GL.Vertex3(0f, base.transform.localPosition.y, max.z);
			GL.Vertex3(0f, base.transform.localPosition.y, max.z);
			GL.Vertex3(max.x, base.transform.localPosition.y, max.z);
			GL.Vertex3(max.x, base.transform.localPosition.y, max.z);
			GL.Vertex3(max.x, base.transform.localPosition.y, 0f);
			GL.Vertex3(max.x, base.transform.localPosition.y, 0f);
			GL.Vertex3(0f, base.transform.localPosition.y, 0f);
			GL.End();
		}
		else
		{
			if (m_Plane != ECoordPlane.XY)
			{
				return;
			}
			GL.Begin(7);
			GL.Color(m_PlaneColor);
			GL.Vertex3(0f, 0f, base.transform.localPosition.z);
			GL.Vertex3(0f, max.y, base.transform.localPosition.z);
			GL.Vertex3(max.x, max.y, base.transform.localPosition.z);
			GL.Vertex3(max.x, 0f, base.transform.localPosition.z);
			GL.End();
			GL.Begin(1);
			GL.Color(m_LineColor);
			GL.Vertex3(0f, 0f, base.transform.localPosition.z);
			GL.Vertex3(0f, max.y, base.transform.localPosition.z);
			GL.Vertex3(0f, max.y, base.transform.localPosition.z);
			GL.Vertex3(max.x, max.y, base.transform.localPosition.z);
			GL.Vertex3(max.x, max.y, base.transform.localPosition.z);
			GL.Vertex3(max.x, 0f, base.transform.localPosition.z);
			GL.Vertex3(max.x, 0f, base.transform.localPosition.z);
			GL.Vertex3(0f, 0f, base.transform.localPosition.z);
			foreach (float x2 in m_Parent.m_Xs)
			{
				float x = x2;
				GL.Color(new Color(0.5f, 0.5f, 1f, 1f));
				GL.Vertex3(x, 0f, base.transform.localPosition.z);
				GL.Vertex3(x, max.y, base.transform.localPosition.z);
			}
			foreach (float y4 in m_Parent.m_Ys)
			{
				float y2 = y4;
				GL.Color(new Color(0.5f, 1f, 1f, 0.8f));
				GL.Vertex3(0f, y2, base.transform.localPosition.z);
				GL.Vertex3(max.x, y2, base.transform.localPosition.z);
			}
			GL.End();
		}
	}
}
