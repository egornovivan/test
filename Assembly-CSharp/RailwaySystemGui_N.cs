using UnityEngine;

public class RailwaySystemGui_N : MonoBehaviour
{
	private static RailwaySystemGui_N mInstance;

	public UIBaseWnd mPointOpWnd;

	public static RailwaySystemGui_N Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}
}
