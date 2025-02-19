public interface IVxDataLoader
{
	bool IsIdle { get; }

	bool ImmMode { get; set; }

	void AddRequest(VFVoxelChunkData data);

	void ProcessReqs();

	void Close();
}
