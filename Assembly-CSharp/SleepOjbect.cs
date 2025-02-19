using UnityEngine;

public class SleepOjbect : MonoBehaviour
{
	[SerializeField]
	private float dalayTime;

	private Rigidbody rigid;

	private Collider coll;

	private MeshRenderer meshRenderer;

	[SerializeField]
	private MonoBehaviour[] scriptsToSleep;

	private void Awake()
	{
		rigid = GetComponent<Rigidbody>();
		coll = GetComponent<Collider>();
		meshRenderer = GetComponent<MeshRenderer>();
		scriptsToSleep = GetComponents<MonoBehaviour>();
		rigid.useGravity = false;
		coll.enabled = false;
		meshRenderer.enabled = false;
		MonoBehaviour[] array = scriptsToSleep;
		foreach (MonoBehaviour monoBehaviour in array)
		{
			monoBehaviour.enabled = false;
		}
		Invoke("WakeUpObject", dalayTime);
	}

	private void WakeUpObject()
	{
		rigid.useGravity = true;
		coll.enabled = true;
		meshRenderer.enabled = true;
		MonoBehaviour[] array = scriptsToSleep;
		foreach (MonoBehaviour monoBehaviour in array)
		{
			monoBehaviour.enabled = true;
		}
		Object.Destroy(this);
	}
}
