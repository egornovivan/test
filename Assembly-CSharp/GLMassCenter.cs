using UnityEngine;

public class GLMassCenter : GLBehaviour
{
	public GUISkin GSkin;

	public Color m_LineXColor = Color.white;

	public Color m_LineYColor = Color.white;

	public Color m_LineZColor = Color.white;

	private float vs = 1f;

	private Vector3 max = Vector3.zero;

	public override void OnGL()
	{
		vs = VCEditor.s_Scene.m_Setting.m_VoxelSize;
		max = VCEditor.s_Scene.m_Setting.EditorWorldSize;
		float num = vs * 10f;
		Color lineXColor = m_LineXColor;
		Color lineYColor = m_LineYColor;
		Color lineZColor = m_LineZColor;
		lineXColor.a = 0.3f;
		lineYColor.a = 0.3f;
		lineZColor.a = 0.3f;
		GL.Begin(1);
		GL.Color(lineXColor);
		GL.Vertex3(0f, base.transform.localPosition.y, base.transform.localPosition.z);
		GL.Vertex3(max.x, base.transform.localPosition.y, base.transform.localPosition.z);
		GL.Color(lineYColor);
		GL.Vertex3(base.transform.localPosition.x, 0f, base.transform.localPosition.z);
		GL.Vertex3(base.transform.localPosition.x, max.y, base.transform.localPosition.z);
		GL.Color(lineZColor);
		GL.Vertex3(base.transform.localPosition.x, base.transform.localPosition.y, 0f);
		GL.Vertex3(base.transform.localPosition.x, base.transform.localPosition.y, max.z);
		GL.Color(m_LineXColor);
		GL.Vertex(base.transform.localPosition + Vector3.right * num);
		GL.Vertex(base.transform.localPosition + Vector3.left * num);
		GL.Color(m_LineYColor);
		GL.Vertex(base.transform.localPosition + Vector3.up * num);
		GL.Vertex(base.transform.localPosition + Vector3.down * num);
		GL.Color(m_LineZColor);
		GL.Vertex(base.transform.localPosition + Vector3.forward * num);
		GL.Vertex(base.transform.localPosition + Vector3.back * num);
		GL.End();
	}

	private void OnGUI()
	{
	}
}
