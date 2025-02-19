using Pathea;
using Railway;
using UnityEngine;

public class RailwayRunner : MonoBehaviour
{
	private static bool useGameTime;

	private static double lastGameTime;

	private static float deltaTime()
	{
		if (!useGameTime)
		{
			return Time.deltaTime * GameTime.NormalTimeSpeed;
		}
		float result = (float)(GameTime.Timer.Second - lastGameTime);
		lastGameTime = GameTime.Timer.Second;
		return result;
	}

	public static void SetTime(bool value)
	{
		useGameTime = value;
		if (useGameTime)
		{
			lastGameTime = GameTime.Timer.Second;
		}
	}

	private void Update()
	{
		PeSingleton<Manager>.Instance.UpdateTrain(deltaTime());
	}
}
