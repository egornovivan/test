public interface IVxChunkHelperProc
{
	IVxSurfExtractor SurfExtractor { get; }

	int ChunkSig { get; }

	void ChunkProcPreSetDataVT(ILODNodeData ndata, byte[] data, bool bFromPool);

	void ChunkProcPreLoadData(ILODNodeData ndata);

	bool ChunkProcExtractData(ILODNodeData ndata);

	VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z);

	void ChunkProcPostGenMesh(IVxSurfExtractReq ireq);

	void OnBegUpdateNodeData(ILODNodeData ndata);

	void OnEndUpdateNodeData(ILODNodeData ndata);

	void OnDestroyNodeData(ILODNodeData ndata);
}
