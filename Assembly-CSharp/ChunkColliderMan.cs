using System.Collections.Generic;

public class ChunkColliderMan
{
	private Dictionary<IntVector3, int> colliderPos;

	public ChunkColliderMan()
	{
		colliderPos = new Dictionary<IntVector3, int>();
	}

	public void addRebuildChunk(IntVector3 chunkIdx)
	{
		if (!colliderPos.ContainsKey(chunkIdx))
		{
			colliderPos.Add(chunkIdx, 0);
		}
	}

	public bool isColliderBeingRebuilt(IntVector3 chunkIdx)
	{
		return colliderPos.ContainsKey(chunkIdx);
	}

	public void colliderBuilt(IntVector3 chunkIdx)
	{
		colliderPos.Remove(chunkIdx);
	}
}
