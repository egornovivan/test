namespace Pathea;

public abstract class PeSingleton<T> where T : class, new()
{
	private static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new T();
				if (instance is IPesingleton pesingleton)
				{
					pesingleton.Init();
				}
			}
			return instance;
		}
		protected set
		{
			instance = value;
		}
	}
}
