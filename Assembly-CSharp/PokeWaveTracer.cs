using UnityEngine;

public class PokeWaveTracer : glWaveTracer
{
	public float scale = 1f;

	public float strength = 2f;

	private Material material;

	private Vector3 v1;

	private Vector3 v2;

	private Vector3 v3;

	private Vector3 v4;

	public override void CustomUpdate()
	{
		Vector3 position = base.Position;
		Vector2 vector = new Vector2(1f, 0f);
		Vector2 vector2 = new Vector3(0f, 1f);
		Vector2 vector3 = new Vector2(position.x, position.z);
		v1 = vector3 - (vector + vector2) * scale * 0.5f;
		v2 = vector3 + (vector - vector2) * scale * 0.5f;
		v3 = vector3 + (vector + vector2) * scale * 0.5f;
		v4 = vector3 - (vector - vector2) * scale * 0.5f;
	}

	public override void Draw()
	{
		if (this.material == null)
		{
			this.material = Resources.Load("Materials/PokeWaveMat") as Material;
			if (this.material != null)
			{
				this.material = Object.Instantiate(this.material);
			}
		}
		if (!(this.material == null))
		{
			Material material = this.material;
			material.SetFloat("_Strength", strength);
			GL.PushMatrix();
			int passCount = material.passCount;
			for (int i = 0; i < passCount; i++)
			{
				material.SetPass(i);
				GL.Begin(7);
				GL.Color(Color.white);
				GL.TexCoord2(0f, 0f);
				GL.Vertex(new Vector3(v1.x, WaterHeight, v1.y));
				GL.TexCoord2(1f, 0f);
				GL.Vertex(new Vector3(v2.x, WaterHeight, v2.y));
				GL.TexCoord2(1f, 1f);
				GL.Vertex(new Vector3(v3.x, WaterHeight, v3.y));
				GL.TexCoord2(0f, 1f);
				GL.Vertex(new Vector3(v4.x, WaterHeight, v4.y));
				GL.End();
			}
			GL.PopMatrix();
		}
	}
}
