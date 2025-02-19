using UnityEngine;

public class UIOthers : MonoBehaviour
{
	private static UIOthers mInctence;

	public static UIOthers Inctence => mInctence;

	private void Awake()
	{
		mInctence = this;
	}

	private void TestRevive()
	{
		GameUI.Instance.mRevive.Show();
	}
}
