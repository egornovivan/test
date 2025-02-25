using System;
using System.Collections.Generic;
using Pathfinding.RVO;
using Pathfinding.RVO.Sampled;
using UnityEngine;

public class GroupController : MonoBehaviour
{
	private const float rad2Deg = 180f / (float)Math.PI;

	public GUIStyle selectionBox;

	public bool adjustCamera = true;

	private Vector2 start;

	private Vector2 end;

	private bool wasDown;

	private List<RVOExampleAgent> selection = new List<RVOExampleAgent>();

	private Simulator sim;

	private Camera cam;

	public void Start()
	{
		cam = Camera.main;
		RVOSimulator rVOSimulator = UnityEngine.Object.FindObjectOfType(typeof(RVOSimulator)) as RVOSimulator;
		if (rVOSimulator == null)
		{
			base.enabled = false;
			throw new Exception("No RVOSimulator in the scene. Please add one");
		}
		sim = rVOSimulator.GetSimulator();
	}

	public void Update()
	{
		if (Screen.fullScreen && Screen.width != Screen.resolutions[Screen.resolutions.Length - 1].width)
		{
			Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, fullscreen: true);
		}
		if (adjustCamera)
		{
			List<Agent> agents = sim.GetAgents();
			float num = 0f;
			for (int i = 0; i < agents.Count; i++)
			{
				float num2 = Mathf.Max(Mathf.Abs(agents[i].InterpolatedPosition.x), Mathf.Abs(agents[i].InterpolatedPosition.z));
				if (num2 > num)
				{
					num = num2;
				}
			}
			float a = num / Mathf.Tan(cam.fieldOfView * ((float)Math.PI / 180f) / 2f);
			float b = num / Mathf.Tan(Mathf.Atan(Mathf.Tan(cam.fieldOfView * ((float)Math.PI / 180f) / 2f) * cam.aspect));
			cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(0f, Mathf.Max(a, b) * 1.1f, 0f), Time.smoothDeltaTime * 2f);
		}
		if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.Mouse0))
		{
			Order();
		}
	}

	private void OnGUI()
	{
		if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Input.GetKey(KeyCode.A))
		{
			Select(start, end);
			wasDown = false;
		}
		if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
		{
			end = Event.current.mousePosition;
			if (!wasDown)
			{
				start = end;
				wasDown = true;
			}
		}
		if (Input.GetKey(KeyCode.A))
		{
			wasDown = false;
		}
		if (wasDown)
		{
			Rect position = Rect.MinMaxRect(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y));
			if (position.width > 4f && position.height > 4f)
			{
				GUI.Box(position, string.Empty, selectionBox);
			}
		}
	}

	public void Order()
	{
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hitInfo))
		{
			float num = 0f;
			for (int i = 0; i < selection.Count; i++)
			{
				num += selection[i].GetComponent<RVOController>().radius;
			}
			float num2 = num / (float)Math.PI;
			num2 *= 2f;
			for (int j = 0; j < selection.Count; j++)
			{
				float num3 = (float)Math.PI * 2f * (float)j / (float)selection.Count;
				Vector3 target = hitInfo.point + new Vector3(Mathf.Cos(num3), 0f, Mathf.Sin(num3)) * num2;
				selection[j].SetTarget(target);
				selection[j].SetColor(GetColor(num3));
				selection[j].RecalculatePath();
			}
		}
	}

	public void Select(Vector2 _start, Vector2 _end)
	{
		_start.y = (float)Screen.height - _start.y;
		_end.y = (float)Screen.height - _end.y;
		Vector2 vector = Vector2.Min(_start, _end);
		Vector2 vector2 = Vector2.Max(_start, _end);
		if ((vector2 - vector).sqrMagnitude < 16f)
		{
			return;
		}
		selection.Clear();
		RVOExampleAgent[] array = UnityEngine.Object.FindObjectsOfType(typeof(RVOExampleAgent)) as RVOExampleAgent[];
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 vector3 = cam.WorldToScreenPoint(array[i].transform.position);
			if (vector3.x > vector.x && vector3.y > vector.y && vector3.x < vector2.x && vector3.y < vector2.y)
			{
				selection.Add(array[i]);
			}
		}
	}

	public Color GetColor(float angle)
	{
		return HSVToRGB(angle * (180f / (float)Math.PI), 0.8f, 0.6f);
	}

	private static Color HSVToRGB(float h, float s, float v)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = s * v;
		float num5 = h / 60f;
		float num6 = num4 * (1f - Math.Abs(num5 % 2f - 1f));
		if (num5 < 1f)
		{
			num = num4;
			num2 = num6;
		}
		else if (num5 < 2f)
		{
			num = num6;
			num2 = num4;
		}
		else if (num5 < 3f)
		{
			num2 = num4;
			num3 = num6;
		}
		else if (num5 < 4f)
		{
			num2 = num6;
			num3 = num4;
		}
		else if (num5 < 5f)
		{
			num = num6;
			num3 = num4;
		}
		else if (num5 < 6f)
		{
			num = num4;
			num3 = num6;
		}
		float num7 = v - num4;
		num += num7;
		num2 += num7;
		num3 += num7;
		return new Color(num, num2, num3);
	}
}
