using UnityEngine;

namespace Pathea.Graphic;

public static class EditorGraphics
{
	public static void DrawXZRect(Vector3 pos, Vector3 size, Color color)
	{
		Vector3 vector = pos + Vector3.right * size.x;
		Vector3 vector2 = vector + Vector3.forward * size.z;
		Vector3 vector3 = pos + Vector3.forward * size.z;
		Debug.DrawLine(pos, vector, color);
		Debug.DrawLine(vector, vector2, color);
		Debug.DrawLine(vector2, vector3, color);
		Debug.DrawLine(vector3, pos, color);
	}
}
