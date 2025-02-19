using UnityEngine;

public class RSManager : MonoBehaviour
{
	private static RSManager s_Instance;

	public static RSManager Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void OnDestroy()
	{
		s_Instance = null;
	}

	private void Update()
	{
	}
}
