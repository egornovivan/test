using UnityEngine;

namespace NovaEnv;

public class CloudLayer : MonoBehaviour
{
	private int layerIndex;

	private Executor executor;

	private GameObject LayerModel;

	public Material LayerMat;

	private Vector3 CloudOffset = Vector3.zero;

	public Color Color1;

	public Color Color2;

	public Color Color3;

	public Color Color4;

	public int LayerIndex
	{
		get
		{
			return layerIndex;
		}
		set
		{
			layerIndex = value;
			LayerMat.renderQueue = 2300 - layerIndex;
			base.transform.localPosition = (Executor.Settings.CloudHeight + (float)layerIndex) * Vector3.up;
			base.transform.localScale = new Vector3(2f, 3f, 2f) * Executor.Settings.CloudArea;
			CloudOffset = (layerIndex + 5) * 60 * new Vector3(0.1f, 0.1f, 0.1f);
		}
	}

	public Executor Executor
	{
		get
		{
			return executor;
		}
		set
		{
			executor = value;
			base.gameObject.AddComponent<MeshFilter>().sharedMesh = Executor.Settings.CloudLayerModel.GetComponent<MeshFilter>().sharedMesh;
			LayerMat = new Material(Executor.Settings.CloudShader);
			LayerMat.SetTexture("_NoiseTexture", Executor.NoiseTexture);
			base.gameObject.AddComponent<MeshRenderer>().material = LayerMat;
		}
	}

	private void OnDestroy()
	{
		Object.Destroy(LayerMat);
	}

	private void Update()
	{
		LayerMat.SetColor("_CloudColor1", Color1);
		LayerMat.SetColor("_CloudColor2", Color2);
		LayerMat.SetColor("_CloudColor3", Color3);
		LayerMat.SetColor("_CloudColor4", Color4);
		LayerMat.SetVector("_SunDirection", Executor.SunDirection);
		Vector3 windDirection = Executor.Wind.WindDirection;
		windDirection.y = windDirection.z;
		windDirection.z = 0f;
		windDirection *= 0.02f;
		windDirection.z = windDirection.magnitude * 1.2f;
		CloudOffset += Mathf.Sqrt((float)executor.Settings.TimeElapseSpeed) * 0.5f * Time.deltaTime * windDirection;
		LayerMat.SetVector("_CloudOffset", CloudOffset);
	}
}
