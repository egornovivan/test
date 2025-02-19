using UnityEngine;

namespace InControl;

public static class Utility
{
	private static Vector2[] circleVertexList = new Vector2[25]
	{
		new Vector2(0f, 1f),
		new Vector2(0.2588f, 0.9659f),
		new Vector2(0.5f, 0.866f),
		new Vector2(0.7071f, 0.7071f),
		new Vector2(0.866f, 0.5f),
		new Vector2(0.9659f, 0.2588f),
		new Vector2(1f, 0f),
		new Vector2(0.9659f, -0.2588f),
		new Vector2(0.866f, -0.5f),
		new Vector2(0.7071f, -0.7071f),
		new Vector2(0.5f, -0.866f),
		new Vector2(0.2588f, -0.9659f),
		new Vector2(0f, -1f),
		new Vector2(-0.2588f, -0.9659f),
		new Vector2(-0.5f, -0.866f),
		new Vector2(-0.7071f, -0.7071f),
		new Vector2(-0.866f, -0.5f),
		new Vector2(-0.9659f, -0.2588f),
		new Vector2(-1f, -0f),
		new Vector2(-0.9659f, 0.2588f),
		new Vector2(-0.866f, 0.5f),
		new Vector2(-0.7071f, 0.7071f),
		new Vector2(-0.5f, 0.866f),
		new Vector2(-0.2588f, 0.9659f),
		new Vector2(0f, 1f)
	};

	public static void DrawCircleGizmo(Vector2 center, float radius)
	{
		Vector2 vector = circleVertexList[0] * radius + center;
		int num = circleVertexList.Length;
		for (int i = 1; i < num; i++)
		{
			Gizmos.DrawLine(vector, vector = circleVertexList[i] * radius + center);
		}
	}

	public static void DrawCircleGizmo(Vector2 center, float radius, Color color)
	{
		Gizmos.color = color;
		DrawCircleGizmo(center, radius);
	}

	public static void DrawOvalGizmo(Vector2 center, Vector2 size)
	{
		Vector2 b = size / 2f;
		Vector2 vector = Vector2.Scale(circleVertexList[0], b) + center;
		int num = circleVertexList.Length;
		for (int i = 1; i < num; i++)
		{
			Gizmos.DrawLine(vector, vector = Vector2.Scale(circleVertexList[i], b) + center);
		}
	}

	public static void DrawOvalGizmo(Vector2 center, Vector2 size, Color color)
	{
		Gizmos.color = color;
		DrawOvalGizmo(center, size);
	}

	public static void DrawRectGizmo(Rect rect)
	{
		Vector3 vector = new Vector3(rect.xMin, rect.yMin);
		Vector3 vector2 = new Vector3(rect.xMax, rect.yMin);
		Vector3 vector3 = new Vector3(rect.xMax, rect.yMax);
		Vector3 vector4 = new Vector3(rect.xMin, rect.yMax);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector3, vector4);
		Gizmos.DrawLine(vector4, vector);
	}

	public static void DrawRectGizmo(Rect rect, Color color)
	{
		Gizmos.color = color;
		DrawRectGizmo(rect);
	}

	public static void DrawRectGizmo(Vector2 center, Vector2 size)
	{
		float num = size.x / 2f;
		float num2 = size.y / 2f;
		Vector3 vector = new Vector3(center.x - num, center.y - num2);
		Vector3 vector2 = new Vector3(center.x + num, center.y - num2);
		Vector3 vector3 = new Vector3(center.x + num, center.y + num2);
		Vector3 vector4 = new Vector3(center.x - num, center.y + num2);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector3, vector4);
		Gizmos.DrawLine(vector4, vector);
	}

	public static void DrawRectGizmo(Vector2 center, Vector2 size, Color color)
	{
		Gizmos.color = color;
		DrawRectGizmo(center, size);
	}

	public static bool GameObjectIsCulledOnCurrentCamera(GameObject gameObject)
	{
		return (Camera.current.cullingMask & (1 << gameObject.layer)) == 0;
	}

	public static Color MoveColorTowards(Color color0, Color color1, float maxDelta)
	{
		float r = Mathf.MoveTowards(color0.r, color1.r, maxDelta);
		float g = Mathf.MoveTowards(color0.g, color1.g, maxDelta);
		float b = Mathf.MoveTowards(color0.b, color1.b, maxDelta);
		float a = Mathf.MoveTowards(color0.a, color1.a, maxDelta);
		return new Color(r, g, b, a);
	}

	public static float ApplyDeadZone(float value, float lowerDeadZone, float upperDeadZone)
	{
		return Mathf.InverseLerp(lowerDeadZone, upperDeadZone, Mathf.Abs(value)) * Mathf.Sign(value);
	}

	public static Vector2 ApplyCircularDeadZone(Vector2 axisVector, float lowerDeadZone, float upperDeadZone)
	{
		float num = Mathf.InverseLerp(lowerDeadZone, upperDeadZone, axisVector.magnitude);
		return axisVector.normalized * num;
	}

	public static Vector2 ApplyCircularDeadZone(float axisX, float axisY, float lowerDeadZone, float upperDeadZone)
	{
		return ApplyCircularDeadZone(new Vector2(axisX, axisY), lowerDeadZone, upperDeadZone);
	}
}
