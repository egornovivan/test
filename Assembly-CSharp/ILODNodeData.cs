public interface ILODNodeData
{
	bool IsEmpty { get; }

	bool IsIdle { get; }

	void BegUpdateNodeData();

	void EndUpdateNodeData();

	void OnDestroyNodeData();

	void UpdateTimeStamp();
}
