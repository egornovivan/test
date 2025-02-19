using UnityEngine;

[ExecuteInEditMode]
public class NavMeshRenderer : MonoBehaviour
{
	private string lastLevel = string.Empty;

	public string SomeFunction()
	{
		return lastLevel;
	}

	private void Update()
	{
	}
}
