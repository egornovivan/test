using UnityEngine;

public class ForceTest : MonoBehaviour
{
	public TextAsset text;

	private void Start()
	{
		Singleton<ForceSetting>.Instance.Load(text);
	}

	private void Update()
	{
	}
}
