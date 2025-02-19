using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadStoryPlayerSpawnPos : LoadPlayerSpawnPos
{
	private SingleGameStory.StoryScene mType;

	public LoadStoryPlayerSpawnPos(bool bNew, SingleGameStory.StoryScene type = SingleGameStory.StoryScene.MainLand)
		: base(bNew)
	{
		mType = type;
	}

	protected override void New()
	{
		if (mType == SingleGameStory.StoryScene.MainLand)
		{
			SetPos(new Vector3(12227f, 121.5f, 6095f));
		}
		else if (mType == SingleGameStory.StoryScene.DienShip0)
		{
			SetPos(new Vector3(14798.09f, 20.98818f, 8246.396f));
		}
		else if (mType == SingleGameStory.StoryScene.L1Ship)
		{
			SetPos(new Vector3(9649.354f, 90.488f, 12744.77f));
		}
	}
}
