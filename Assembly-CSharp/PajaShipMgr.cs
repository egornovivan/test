using UnityEngine;

public class PajaShipMgr : MonoBehaviour
{
	public Light directionLight;

	public MeshRenderer waterRenderer;

	private void Start()
	{
	}

	private void Update()
	{
		if (PEWaveSystem.Self != null && waterRenderer != null && waterRenderer.material != null && PEWaveSystem.Self.Target != null)
		{
			waterRenderer.material.SetTexture("_WaveMap", PEWaveSystem.Self.Target);
		}
	}
}
