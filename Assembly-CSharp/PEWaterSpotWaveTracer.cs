using PETools;
using UnityEngine;

public class PEWaterSpotWaveTracer : SpotWaveTracer
{
	public string Desc;

	public float Height;

	public float Width;

	public float Length;

	public bool IsValid = true;

	protected override void Init()
	{
		base.Init();
		if (PEWaveSystem.Self != null)
		{
			PEWaveSystem.Self.WaveRenderer.Add(this);
		}
	}

	public override void CustomUpdate()
	{
		if (!IsValid)
		{
			return;
		}
		Vector3 position = TracerTrans.position;
		if (position.x < -99999f || position.x > 99999f || position.z < -99999f || position.z > 99999f)
		{
			return;
		}
		float num = position.x - Length * 0.5f;
		float y = position.y;
		float num2 = position.z - Width * 0.5f;
		float num3 = position.x + Length * 0.5f;
		float num4 = position.y + Height;
		float num5 = position.z + Width * 0.5f;
		float num6 = num4 - y;
		float y2 = position.y + num6;
		bool flag = false;
		for (float num7 = num; num7 <= num3 + 0.5f; num7 += 1f)
		{
			for (float num8 = num2; num8 <= num5 + 0.5f; num8 += 1f)
			{
				flag = PE.PointInWater(new Vector3(num7, y2, num8)) > 0.52f;
				if (flag)
				{
					break;
				}
			}
		}
		float waterHeight = 0f;
		bool autoGenWave = false;
		float num9 = 0f;
		if (!flag)
		{
			Ray ray = new Ray(new Vector3(position.x, position.y + num6, position.z), Vector3.down);
			Vector3 point = Vector3.zero;
			float num10 = 0f;
			if (PE.RaycastVoxel(ray, out point, Mathf.CeilToInt(num6), 1, 2))
			{
				waterHeight = (int)point.y + 1;
				num10 = Mathf.Abs(point.y - position.y);
			}
			num9 = num10 / num6 * 20f;
			autoGenWave = true;
		}
		float strength = Strength;
		AutoGenWave = autoGenWave;
		WaterHeight = waterHeight;
		Strength = strength + num9;
		base.CustomUpdate();
		Strength = strength;
	}

	public override void Draw()
	{
		if (IsValid)
		{
			base.Draw();
		}
	}
}
