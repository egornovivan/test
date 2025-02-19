using Jboy;
using UnityEngine;

namespace CustomData;

public class QuaternionToJson
{
	public static void JsonSerializer(JsonWriter writer, object o)
	{
		Quaternion quaternion = (Quaternion)o;
		writer.WriteObjectStart();
		writer.WritePropertyName("x");
		Json.WriteObject(quaternion.x, writer);
		writer.WritePropertyName("y");
		Json.WriteObject(quaternion.y, writer);
		writer.WritePropertyName("z");
		Json.WriteObject(quaternion.z, writer);
		writer.WritePropertyName("w");
		Json.WriteObject(quaternion.w, writer);
		writer.WriteObjectEnd();
	}

	public static object JsonDeserializer(JsonReader reader)
	{
		Quaternion quaternion = default(Quaternion);
		reader.ReadObjectStart();
		reader.ReadPropertyName("x");
		quaternion.x = Json.ReadObject<float>(reader);
		reader.ReadPropertyName("y");
		quaternion.y = Json.ReadObject<float>(reader);
		reader.ReadPropertyName("z");
		quaternion.z = Json.ReadObject<float>(reader);
		reader.ReadPropertyName("w");
		quaternion.w = Json.ReadObject<float>(reader);
		reader.ReadObjectEnd();
		return quaternion;
	}
}
