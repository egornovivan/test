namespace Pathea;

public abstract class PlayerTypeLoader
{
	private PeGameMgr.ESceneMode mSceneMode;

	public PeGameMgr.ESceneMode sceneMode
	{
		get
		{
			return mSceneMode;
		}
		set
		{
			mSceneMode = value;
		}
	}

	public abstract void Load();
}
