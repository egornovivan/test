using Pathea;

namespace PeMap;

public class ArchivableLabelMgr<T0, T> : MyListArchiveSingleton<T0, T> where T0 : class, new() where T : class, ILabel, ISerializable, new()
{
	public override void Add(T item)
	{
		if (PeSingleton<LabelMgr>.Instance.Add(item))
		{
			base.Add(item);
		}
	}

	public override bool Remove(T item)
	{
		if (PeSingleton<LabelMgr>.Instance.Remove(item))
		{
			return base.Remove(item);
		}
		return false;
	}
}
