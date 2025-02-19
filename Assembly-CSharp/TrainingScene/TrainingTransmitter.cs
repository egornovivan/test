using UnityEngine;

namespace TrainingScene;

public class TrainingTransmitter : MonoBehaviour
{
	private bool disable;

	private void OnTriggerEnter()
	{
	}

	private void OnTriggerExit()
	{
		disable = false;
	}

	public void LoadStoryScene()
	{
	}

	public void LoadMenuScene()
	{
	}
}
