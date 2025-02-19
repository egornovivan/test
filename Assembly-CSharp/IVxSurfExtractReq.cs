using UnityEngine;

public interface IVxSurfExtractReq
{
	byte[] VolumeData { get; }

	bool IsInvalid { get; }

	int Signature { get; }

	int Priority { get; }

	int MeshSplitThreshold { get; }

	int FillMesh(Mesh mesh);

	bool OnReqFinished();
}
