using Jboy;
using UnityEngine;

namespace CustomData;

public class Vector3ToJson
{
	public static void JsonSerializer(JsonWriter writer, object o)
	{
		Vector3 vector = (Vector3)o;
		writer.WriteObjectStart();
		writer.WritePropertyName("x");
		Json.WriteObject(vector.x, writer);
		writer.WritePropertyName("y");
		Json.WriteObject(vector.y, writer);
		writer.WritePropertyName("z");
		Json.WriteObject(vector.z, writer);
		writer.WriteObjectEnd();
	}

	public static object JsonDeserializer(JsonReader reader)
	{
		Vector3 vector = default(Vector3);
		reader.ReadObjectStart();
		reader.ReadPropertyName("x");
		vector.x = Json.ReadObject<float>(reader);
		reader.ReadPropertyName("y");
		vector.y = Json.ReadObject<float>(reader);
		reader.ReadPropertyName("z");
		vector.z = Json.ReadObject<float>(reader);
		reader.ReadObjectEnd();
		return vector;
	}
}
