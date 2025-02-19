using UnityEngine;

public class GLComponentBound : GLBehaviour
{
	public VCEComponentTool m_ParentComponent;

	public Color m_BoundColor = s_Green;

	public bool m_Highlight;

	public static Color s_Green = new Color(0.5f, 1f, 0f, 1f);

	public static Color s_Red = new Color(1f, 0f, 0.135f, 1f);

	public static Color s_Blue = new Color(0f, 0.385f, 1f, 1f);

	public static Color s_Orange = new Color(1f, 0.5f, 0f, 1f);

	public static Color s_Yellow = new Color(1f, 1f, 0f, 1f);

	public override void OnGL()
	{
		Vector3[] array = new Vector3[8]
		{
			new Vector3(0.5f, 0.5f, 0.5f),
			new Vector3(-0.5f, 0.5f, 0.5f),
			new Vector3(-0.5f, -0.5f, 0.5f),
			new Vector3(0.5f, -0.5f, 0.5f),
			new Vector3(0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, -0.5f, -0.5f),
			new Vector3(0.5f, -0.5f, -0.5f)
		};
		for (int i = 0; i < 8; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = base.transform.TransformPoint(array[i]);
		}
		Color boundColor = m_BoundColor;
		Color boundColor2 = m_BoundColor;
		boundColor.a = 1f;
		boundColor2 *= ((!m_Highlight) ? 0.65f : 1f);
		boundColor2.a = 1f;
		boundColor2.a *= ((!m_Highlight) ? 0.1f : (0.25f + Mathf.Sin(Time.time * 6f) * 0.1f));
		GL.Begin(1);
		GL.Color(boundColor);
		GL.Vertex(array[0]);
		GL.Vertex(array[1]);
		GL.Vertex(array[1]);
		GL.Vertex(array[2]);
		GL.Vertex(array[2]);
		GL.Vertex(array[3]);
		GL.Vertex(array[3]);
		GL.Vertex(array[0]);
		GL.Vertex(array[4]);
		GL.Vertex(array[5]);
		GL.Vertex(array[5]);
		GL.Vertex(array[6]);
		GL.Vertex(array[6]);
		GL.Vertex(array[7]);
		GL.Vertex(array[7]);
		GL.Vertex(array[4]);
		GL.Vertex(array[0]);
		GL.Vertex(array[4]);
		GL.Vertex(array[1]);
		GL.Vertex(array[5]);
		GL.Vertex(array[2]);
		GL.Vertex(array[6]);
		GL.Vertex(array[3]);
		GL.Vertex(array[7]);
		GL.End();
		GL.Begin(7);
		GL.Color(boundColor2);
		GL.Vertex(array[0]);
		GL.Vertex(array[1]);
		GL.Vertex(array[2]);
		GL.Vertex(array[3]);
		GL.Vertex(array[4]);
		GL.Vertex(array[5]);
		GL.Vertex(array[6]);
		GL.Vertex(array[7]);
		GL.Vertex(array[0]);
		GL.Vertex(array[4]);
		GL.Vertex(array[5]);
		GL.Vertex(array[1]);
		GL.Vertex(array[1]);
		GL.Vertex(array[5]);
		GL.Vertex(array[6]);
		GL.Vertex(array[2]);
		GL.Vertex(array[2]);
		GL.Vertex(array[6]);
		GL.Vertex(array[7]);
		GL.Vertex(array[3]);
		GL.Vertex(array[3]);
		GL.Vertex(array[7]);
		GL.Vertex(array[4]);
		GL.Vertex(array[0]);
		GL.End();
	}
}
