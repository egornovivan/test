using TrainingScene;

namespace Pathea.GameLoader;

internal class LoadTutorial : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		TrainingRoomLoader.LoadTrainingRoom();
	}
}
