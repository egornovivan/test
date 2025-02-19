public interface ISceneEntityMissionPoint
{
	int MissionId { get; set; }

	int TargetId { get; set; }

	bool Start();

	void Stop();
}
