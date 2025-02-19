using UnityEngine;

public class GLGridInitEffect : GLBehaviour
{
	private float m_Time;

	public float m_AnimationLife = 2f;

	public Vector3 m_CellSize = Vector3.one;

	public IntVector3 m_CellCount = IntVector3.One;

	public Color m_NormalColor;

	public Color m_FlowColor;

	public Color m_GlowColor;

	private void Start()
	{
		m_Time = 0f;
	}

	private void OnEnable()
	{
		m_Time = 0f;
	}

	private void Update()
	{
		m_Time += Time.deltaTime;
	}

	public override void OnGL()
	{
		for (int i = 0; i < m_CellCount.x; i++)
		{
			for (int j = 0; j < m_CellCount.z; j++)
			{
				DrawCell(i, j);
			}
		}
	}

	private void DrawCell(int x, int z)
	{
		Vector3 vector = new Vector3(((float)x + 0.02f) * m_CellSize.x, m_CellSize.y * 0.02f, ((float)z + 0.02f) * m_CellSize.z);
		Vector3 vector2 = new Vector3(((float)x + 0.98f) * m_CellSize.x, m_CellSize.y * 0.02f, ((float)z + 0.98f) * m_CellSize.z);
		float num = m_Time / m_AnimationLife * new Vector2(m_CellCount.x, m_CellCount.z).magnitude;
		float num2 = new Vector2((float)x - (float)m_CellCount.x * 0.5f, (float)z - (float)m_CellCount.z * 0.5f).magnitude + (Mathf.Sin(x) + Mathf.Cos(z)) * 3f;
		float num3 = 5f;
		float num4 = Mathf.Clamp01((num3 - Mathf.Abs(num2 - num)) / num3);
		GL.Begin(7);
		GL.Color(QuadColor(num4, num2 < num));
		GL.Vertex3(vector.x, vector.y + num4 * m_CellSize.y, vector.z);
		GL.Vertex3(vector.x, vector.y, vector2.z);
		GL.Vertex3(vector2.x, vector.y + num4 * m_CellSize.y, vector2.z);
		GL.Vertex3(vector2.x, vector.y, vector.z);
		GL.End();
	}

	private Color QuadColor(float brightness, bool inner)
	{
		if (brightness > 0.5f)
		{
			return Color.Lerp(m_FlowColor, m_GlowColor, (brightness - 0.5f) * 2f);
		}
		return Color.Lerp((!inner) ? Color.clear : m_NormalColor, m_FlowColor, brightness * 2f);
	}
}
