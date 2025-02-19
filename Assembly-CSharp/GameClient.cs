using UnityEngine;

public class GameClient : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(this);
		LogManager.InitLogManager();
		ClientConfig.InitClientConfig();
		MapsConfig.InitMapConfig();
	}
}
