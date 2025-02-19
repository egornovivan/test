using RedGrass;
using UnityEngine;

public class RGFuncTest : MonoBehaviour
{
	public RGScene scene;

	public Transform tracer;

	private void OnGUI()
	{
		if (GUI.Button(new Rect(100f, 100f, 100f, 50f), "Dirty"))
		{
			Vector3 position = tracer.transform.position;
			for (int i = Mathf.RoundToInt(position.y) - 10; i < Mathf.RoundToInt(position.y) + 10; i++)
			{
				scene.data.Remove(Mathf.RoundToInt(position.x), i, Mathf.RoundToInt(position.z));
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
