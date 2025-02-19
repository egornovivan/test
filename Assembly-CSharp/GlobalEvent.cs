using UnityEngine;

public class GlobalEvent : MonoBehaviour
{
	public delegate void VoidFunc();

	public delegate void IndexBaseFunc(int index);

	public delegate void PositionBaseFunc(Vector3 positon);

	private static GlobalEvent mInstance;

	public static GlobalEvent Instance => mInstance;

	public static event IndexBaseFunc OnPlayerGetOnTrain;

	public static event PositionBaseFunc OnPlayerGetOffTrain;

	public static event VoidFunc OnPowerPlantStateChanged;

	public static event VoidFunc OnMouseUnlock;

	private void Awake()
	{
		mInstance = this;
	}

	private void OnDestroy()
	{
		GlobalEvent.OnPlayerGetOnTrain = null;
		GlobalEvent.OnPlayerGetOffTrain = null;
		GlobalEvent.OnPowerPlantStateChanged = null;
		GlobalEvent.OnMouseUnlock = null;
	}

	public static void PlayerGetOnTrain(int index)
	{
		if (GlobalEvent.OnPlayerGetOnTrain != null)
		{
			GlobalEvent.OnPlayerGetOnTrain(index);
		}
	}

	public static void PlayerGetOffTrain(Vector3 position)
	{
		if (GlobalEvent.OnPlayerGetOffTrain != null)
		{
			GlobalEvent.OnPlayerGetOffTrain(position);
		}
	}

	public static void NoticePowerPlantStateChanged()
	{
		if (GlobalEvent.OnPowerPlantStateChanged != null)
		{
			GlobalEvent.OnPowerPlantStateChanged();
		}
	}

	public static void NoticeMouseUnlock()
	{
		if (GlobalEvent.OnMouseUnlock != null)
		{
			GlobalEvent.OnMouseUnlock();
		}
	}
}
