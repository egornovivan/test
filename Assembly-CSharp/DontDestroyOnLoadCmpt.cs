using UnityEngine;

public class DontDestroyOnLoadCmpt : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(this);
	}
}
