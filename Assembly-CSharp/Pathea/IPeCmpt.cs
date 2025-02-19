using System.IO;

namespace Pathea;

public interface IPeCmpt
{
	void Serialize(BinaryWriter w);

	void Deserialize(BinaryReader r);

	string GetTypeName();
}
