using UnityEngine;

namespace TrainingScene;

public class EpsilonMiniMap : MonoBehaviour
{
	public Texture trainingRoomMinimap;

	private void Start()
	{
		UIMinMapCtrl.Instance.mPlayerPosText.gameObject.SetActive(value: false);
		UIMinMapCtrl.Instance.mTimeLabel.gameObject.SetActive(value: false);
		UIMinMapCtrl.Instance.mMiniMapTex.material.SetTexture("_MainTex", trainingRoomMinimap);
	}

	private void Update()
	{
	}
}
