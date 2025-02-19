using UnityEngine;

namespace NovaEnv;

public class ScreenDropsEmitter : MonoBehaviour
{
	[HideInInspector]
	public Executor Executor;

	public GameObject ScreenDropPrefab;

	public bool EmitNow;

	private void Emit()
	{
		ScreenDrop component = Utils.CreateGameObject(ScreenDropPrefab, "Screen Drop", base.transform).GetComponent<ScreenDrop>();
		component.Executor = Executor;
		component.pos = new Vector3(Random.value * 70f - 35f, Random.value * 70f - 35f, 0f);
		component.Slip();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!(Executor != null))
		{
			return;
		}
		Ray ray = new Ray((!(Executor.Settings.MainCamera != null)) ? Vector3.zero : Executor.Settings.MainCamera.transform.position, Executor.Storm.transform.up);
		if (!Physics.Raycast(ray, 50f))
		{
			float num = Executor.Storm.RainDropsCountChange.Evaluate(Executor.Storm.Strength) * 0.4f;
			if (Random.value < num)
			{
				Emit();
			}
			if (EmitNow)
			{
				Emit();
				EmitNow = false;
			}
		}
	}
}
