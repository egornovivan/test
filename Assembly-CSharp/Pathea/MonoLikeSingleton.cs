namespace Pathea;

public abstract class MonoLikeSingleton<T> : PeSingleton<T>, IPesingleton, MonoLikeSingletonMgr.IMonoLike where T : class, new()
{
	void IPesingleton.Init()
	{
		PeSingleton<MonoLikeSingletonMgr>.Instance.Register(this);
		OnInit();
	}

	protected virtual void OnInit()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void OnDestroy()
	{
		PeSingleton<T>.Instance = (T)null;
	}
}
