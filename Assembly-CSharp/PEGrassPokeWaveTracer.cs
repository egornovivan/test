using PETools;
using UnityEngine;

public class PEGrassPokeWaveTracer : PokeWaveTracer
{
	public float Width = 1f;

	public float Length = 1f;

	public float Height = 2f;

	private bool m_GenWave;

	public float MaxScale = 1f;

	public float TweeFactor = 0.5f;

	private float _targetScale;

	private float _curTime;

	protected override void Init()
	{
		base.Init();
		if (PEWaveSystem.Self != null)
		{
			PEWaveSystem.Self.GrassWaveRenderer.Add(this);
		}
		_targetScale = MaxScale;
	}

	public override void CustomUpdate()
	{
		Vector3 position = TracerTrans.position;
		if (position.x < -99999f || position.x > 99999f || position.z < -99999f || position.z > 99999f)
		{
			return;
		}
		float num = position.x - Length * 0.5f;
		float num2 = position.z - Width * 0.5f;
		float num3 = position.x + Length * 0.5f;
		float num4 = position.z + Width * 0.5f;
		WaterHeight = position.y;
		bool flag = false;
		for (float num5 = num; num5 <= num3 + 0.5f; num5 += 1f)
		{
			for (float num6 = num2; num6 <= num4 + 0.5f; num6 += 1f)
			{
				flag = PE.PointInTerrain(new Vector3(num5, position.y, num6)) > 0.52f;
				if (flag)
				{
					break;
				}
			}
		}
		bool genWave = m_GenWave;
		if (!flag)
		{
			Ray ray = new Ray(new Vector3(position.x, position.y, position.z), Vector3.down);
			Vector3 point = Vector3.zero;
			if (PE.RaycastVoxel(ray, out point, Mathf.CeilToInt(Height), 1, 1))
			{
				m_GenWave = true;
			}
			else
			{
				m_GenWave = false;
			}
		}
		else
		{
			m_GenWave = true;
		}
		if (genWave != m_GenWave)
		{
			if (genWave)
			{
				_targetScale = 0f;
				_curTime = Time.time;
			}
			else
			{
				_targetScale = MaxScale;
				_curTime = Time.time;
			}
		}
		scale = Mathf.Lerp(scale, _targetScale, Mathf.Clamp01(Mathf.Pow(Time.time - _curTime, 2f) * TweeFactor));
		base.CustomUpdate();
	}

	public override void Draw()
	{
		base.Draw();
	}
}
