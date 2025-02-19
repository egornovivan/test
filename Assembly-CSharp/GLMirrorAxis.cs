using UnityEngine;

public class GLMirrorAxis : GLBehaviour
{
	public VCEMirrorGL m_Parent;

	public ECoordAxis m_Axis;

	public Color m_LineColor = Color.white;

	private Vector3 max = Vector3.zero;

	public void Update()
	{
		max = VCEditor.s_Scene.m_Setting.EditorWorldSize;
		LineRenderer component = GetComponent<LineRenderer>();
		component.SetWidth(VCEditor.s_Scene.m_Setting.m_VoxelSize * 2f, VCEditor.s_Scene.m_Setting.m_VoxelSize * 2f);
		if (m_Axis == ECoordAxis.X)
		{
			component.SetPosition(1, new Vector3(max.x, 0f, 0f));
		}
		else if (m_Axis == ECoordAxis.Y)
		{
			component.SetPosition(1, new Vector3(0f, max.y, 0f));
		}
		else if (m_Axis == ECoordAxis.Z)
		{
			component.SetPosition(1, new Vector3(0f, 0f, max.z));
		}
	}

	public override void OnGL()
	{
		max = VCEditor.s_Scene.m_Setting.EditorWorldSize;
		if (m_Axis == ECoordAxis.X)
		{
			GL.Begin(1);
			GL.Color(m_LineColor);
			GL.Vertex3(0f, 0f, base.transform.localPosition.z);
			GL.Vertex3(0f, max.y, base.transform.localPosition.z);
			GL.Vertex3(0f, base.transform.localPosition.y, 0f);
			GL.Vertex3(0f, base.transform.localPosition.y, max.z);
			GL.Vertex3(max.x, 0f, base.transform.localPosition.z);
			GL.Vertex3(max.x, max.y, base.transform.localPosition.z);
			GL.Vertex3(max.x, base.transform.localPosition.y, 0f);
			GL.Vertex3(max.x, base.transform.localPosition.y, max.z);
			foreach (float x2 in m_Parent.m_Xs)
			{
				float x = x2;
				GL.Vertex3(x, 0f, base.transform.localPosition.z);
				GL.Vertex3(x, max.y, base.transform.localPosition.z);
				GL.Vertex3(x, base.transform.localPosition.y, 0f);
				GL.Vertex3(x, base.transform.localPosition.y, max.z);
			}
			GL.End();
		}
		else if (m_Axis == ECoordAxis.Y)
		{
			GL.Begin(1);
			GL.Color(m_LineColor);
			GL.Vertex3(0f, 0f, base.transform.localPosition.z);
			GL.Vertex3(max.x, 0f, base.transform.localPosition.z);
			GL.Vertex3(base.transform.localPosition.x, 0f, 0f);
			GL.Vertex3(base.transform.localPosition.x, 0f, max.z);
			GL.Vertex3(0f, max.y, base.transform.localPosition.z);
			GL.Vertex3(max.x, max.y, base.transform.localPosition.z);
			GL.Vertex3(base.transform.localPosition.x, max.y, 0f);
			GL.Vertex3(base.transform.localPosition.x, max.y, max.z);
			foreach (float y2 in m_Parent.m_Ys)
			{
				float y = y2;
				GL.Vertex3(0f, y, base.transform.localPosition.z);
				GL.Vertex3(max.x, y, base.transform.localPosition.z);
				GL.Vertex3(base.transform.localPosition.x, y, 0f);
				GL.Vertex3(base.transform.localPosition.x, y, max.z);
			}
			GL.End();
		}
		else
		{
			if (m_Axis != ECoordAxis.Z)
			{
				return;
			}
			GL.Begin(1);
			GL.Color(m_LineColor);
			GL.Vertex3(base.transform.localPosition.x, 0f, 0f);
			GL.Vertex3(base.transform.localPosition.x, max.y, 0f);
			GL.Vertex3(0f, base.transform.localPosition.y, 0f);
			GL.Vertex3(max.x, base.transform.localPosition.y, 0f);
			GL.Vertex3(base.transform.localPosition.x, 0f, max.z);
			GL.Vertex3(base.transform.localPosition.x, max.y, max.z);
			GL.Vertex3(0f, base.transform.localPosition.y, max.z);
			GL.Vertex3(max.x, base.transform.localPosition.y, max.z);
			foreach (float z2 in m_Parent.m_Zs)
			{
				float z = z2;
				GL.Vertex3(base.transform.localPosition.x, 0f, z);
				GL.Vertex3(base.transform.localPosition.x, max.y, z);
				GL.Vertex3(0f, base.transform.localPosition.y, z);
				GL.Vertex3(max.x, base.transform.localPosition.y, z);
			}
			GL.End();
		}
	}
}
