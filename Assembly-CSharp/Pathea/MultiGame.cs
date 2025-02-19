namespace Pathea;

public abstract class MultiGame
{
	private bool mNewGame;

	private string mYirdName;

	protected bool newGame
	{
		get
		{
			return mNewGame;
		}
		private set
		{
			mNewGame = value;
		}
	}

	public string yirdName
	{
		get
		{
			return mYirdName;
		}
		private set
		{
			mYirdName = value;
		}
	}

	public void Load(bool newGame, string yirdName)
	{
		SingleGameStory.curType = SingleGameStory.StoryScene.MainLand;
		mNewGame = newGame;
		mYirdName = yirdName;
		LoadYird();
		Load();
	}

	private void LoadYird()
	{
		if (string.IsNullOrEmpty(mYirdName))
		{
			mYirdName = GetDefaultYirdName();
		}
		PeSingleton<ArchiveMgr>.Instance.LoadYird(mYirdName);
	}

	protected abstract void Load();

	protected abstract string GetDefaultYirdName();
}
