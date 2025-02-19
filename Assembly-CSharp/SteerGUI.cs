using Steer3D;
using UnityEngine;

public class SteerGUI : MonoBehaviour
{
	public SteerAgent agent;

	public RectTransform desired_rect;

	public RectTransform velocity_rect;

	private bool dragging;

	private void Start()
	{
	}

	private void Update()
	{
		if (dragging)
		{
			if (Input.GetMouseButtonUp(0))
			{
				dragging = false;
			}
			Vector3 vector = base.transform.InverseTransformPoint(Input.mousePosition);
			vector = Vector3.ClampMagnitude(vector, 90f);
			if (vector.magnitude < 5f)
			{
				vector = Vector3.zero;
			}
			desired_rect.anchoredPosition = new Vector2(vector.x, vector.y);
		}
		Vector3 desired_vel = new Vector3(desired_rect.anchoredPosition.x, 0f, desired_rect.anchoredPosition.y) / 90f;
		agent.AddDesiredVelocity(desired_vel, 4f, 0.8f);
		Vector3 vector2 = agent.velocity * 90f;
		velocity_rect.anchoredPosition = new Vector2(vector2.x, vector2.z);
	}

	public void OnPointerDown()
	{
		dragging = true;
	}
}
