using Mono.Data.SqliteClient;

namespace Pathea;

public class DbAttr
{
	public float[] attributeArray = new float[97];

	public void ReadFromDb(SqliteDataReader reader)
	{
		for (int i = 0; i < 97; i++)
		{
			AttribType attribType = (AttribType)i;
			string name = attribType.ToString();
			attributeArray[i] = reader.GetFloat(reader.GetOrdinal(name));
		}
	}
}
