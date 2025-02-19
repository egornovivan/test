using UnityEngine;

namespace WhiteCat;

public class SingletonBehaviour<T> : BaseBehaviour where T : SingletonBehaviour<T>
{
	private static T _instance;

	public static T instance => _instance ? _instance : (_instance = GetInstance());

	private static T GetInstance()
	{
		T[] array = Object.FindObjectsOfType<T>();
		if (array.Length == 0)
		{
			Debug.Log(string.Concat("There is no instance of singleton type ", typeof(T), ", a new instance will be created immediately."));
			GameObject gameObject = new GameObject(typeof(T).ToString());
			Object.DontDestroyOnLoad(gameObject);
			return gameObject.AddComponent<T>();
		}
		if (array.Length > 1)
		{
			Debug.LogError(string.Concat("There are more than one instance of singleton type ", typeof(T), ", the first found one will be returned."));
		}
		return array[0];
	}

	protected virtual void Awake()
	{
		if ((bool)_instance)
		{
			Debug.LogError(string.Concat("Already exist a instance of singleton type ", typeof(T), ", you should not create it again."));
		}
		else
		{
			_instance = this as T;
		}
	}
}
