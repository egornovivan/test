public interface ILODNodeDataMan
{
	int IdxInLODNodeData { get; set; }

	LODOctreeMan LodMan { get; set; }

	ILODNodeData CreateLODNodeData(LODOctreeNode node);

	void ProcPostLodInit();

	void ProcPostLodUpdate();

	void ProcPostLodRefresh();
}
