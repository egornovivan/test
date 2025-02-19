using System;

namespace WhiteCat.Internal;

public class ObjectRecycler : BaseBehaviour
{
	[NonSerialized]
	public ObjectPool objectPool;
}
