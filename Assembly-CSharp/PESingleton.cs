using System;

public class PESingleton<T>
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Activator.CreateInstance<T>();
			}
			return _instance;
		}
	}
}
