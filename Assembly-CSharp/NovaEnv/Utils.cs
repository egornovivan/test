using UnityEngine;

namespace NovaEnv;

public static class Utils
{
	public static GameObject CreateGameObject(GameObject res, string name, Transform parent)
	{
		if (res != null)
		{
			GameObject gameObject = Object.Instantiate(res);
			gameObject.name = name;
			gameObject.transform.SetParent(parent, worldPositionStays: false);
			gameObject.transform.localPosition = Vector3.zero;
			if (parent != null)
			{
				gameObject.layer = parent.gameObject.layer;
			}
			return gameObject;
		}
		GameObject gameObject2 = new GameObject(name);
		gameObject2.transform.SetParent(parent, worldPositionStays: false);
		gameObject2.transform.localPosition = Vector3.zero;
		if (parent != null)
		{
			gameObject2.layer = parent.gameObject.layer;
		}
		return gameObject2;
	}

	public static T CreateGameObject<T>(GameObject res, string name, Transform parent) where T : Component
	{
		if (res != null)
		{
			GameObject gameObject = Object.Instantiate(res);
			gameObject.name = name;
			gameObject.transform.SetParent(parent, worldPositionStays: false);
			gameObject.transform.localPosition = Vector3.zero;
			if (parent != null)
			{
				gameObject.layer = parent.gameObject.layer;
			}
			T component = gameObject.GetComponent<T>();
			if (component == null)
			{
				Object.Destroy(gameObject);
			}
			return component;
		}
		GameObject gameObject2 = new GameObject(name);
		gameObject2.transform.SetParent(parent, worldPositionStays: false);
		gameObject2.transform.localPosition = Vector3.zero;
		if (parent != null)
		{
			gameObject2.layer = parent.gameObject.layer;
		}
		return gameObject2.AddComponent<T>();
	}

	public static GameObject CreateGameObject(PrimitiveType primitive, string name, Transform parent)
	{
		GameObject gameObject = GameObject.CreatePrimitive(primitive);
		gameObject.name = name;
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		gameObject.transform.localPosition = Vector3.zero;
		if (parent != null)
		{
			gameObject.layer = parent.gameObject.layer;
		}
		if (gameObject.GetComponent<Collider>() != null)
		{
			Object.Destroy(gameObject.GetComponent<Collider>());
		}
		return gameObject;
	}

	public static void DestroyChild(GameObject gameObject)
	{
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			Object.Destroy(gameObject.transform.GetChild(i).gameObject);
		}
	}

	public static float NormalizeDegree(float deg)
	{
		while (deg > 180f)
		{
			deg -= 360f;
		}
		while (deg < -180f)
		{
			deg += 360f;
		}
		return deg;
	}

	public static Color ColorSaturate(Color c, float saturate)
	{
		float grayscale = c.grayscale;
		Color a = new Color(grayscale, grayscale, grayscale, 1f);
		return Color.Lerp(a, c, saturate);
	}
}
