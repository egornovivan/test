using UnityEngine;

public class LockShadowDistance : MonoBehaviour
{
	[SerializeField]
	private float LockShadowDist = 80f;

	private float tempShadowDist;

	private void Update()
	{
		if (QualitySettings.shadowDistance != LockShadowDist)
		{
			tempShadowDist = QualitySettings.shadowDistance;
			QualitySettings.shadowDistance = LockShadowDist;
		}
	}

	private void OnDestroy()
	{
		QualitySettings.shadowDistance = tempShadowDist;
	}
}
