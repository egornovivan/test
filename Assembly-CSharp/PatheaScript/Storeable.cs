using System.IO;

namespace PatheaScript;

public interface Storeable
{
	void Store(BinaryWriter w);

	void Restore(BinaryReader r);
}
