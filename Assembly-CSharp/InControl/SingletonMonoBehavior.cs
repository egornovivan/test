using UnityEngine;

namespace InControl;

public class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour
{
	private static object _lock = new object();

	public static T Instance { get; private set; }

	protected void SetSingletonInstance()
	{
		lock (_lock)
		{
			if (!(Instance == null))
			{
				return;
			}
			T[] array = Object.FindObjectsOfType<T>();
			if (array.Length > 0)
			{
				Instance = array[0];
				if (array.Length > 1)
				{
					Debug.LogWarning(string.Concat("Multiple instances of singleton ", typeof(T), " found."));
				}
			}
			else
			{
				Debug.LogError(string.Concat("No instance of singleton ", typeof(T), " found."));
			}
		}
	}
}
