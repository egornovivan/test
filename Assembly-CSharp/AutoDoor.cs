using DunGen;
using Pathea;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
	public enum DoorState
	{
		Open,
		Closed,
		Opening,
		Closing
	}

	public GameObject Door;

	public Vector3 OpenOffset = new Vector3(0f, 2.5f, 0f);

	public float Speed = 3f;

	private Vector3 closedPosition;

	private DoorState currentState = DoorState.Closed;

	private float currentFramePosition;

	private Door doorComponent;

	private void Start()
	{
		doorComponent = GetComponent<Door>();
		closedPosition = Door.transform.localPosition;
	}

	private void Update()
	{
		if (currentState == DoorState.Opening || currentState == DoorState.Closing)
		{
			Vector3 b = closedPosition + OpenOffset;
			float num = Speed * Time.deltaTime;
			if (currentState == DoorState.Closing)
			{
				num *= -1f;
			}
			currentFramePosition += num;
			currentFramePosition = Mathf.Clamp(currentFramePosition, 0f, 1f);
			Door.transform.localPosition = Vector3.Lerp(closedPosition, b, currentFramePosition);
			if (currentFramePosition == 1f)
			{
				currentState = DoorState.Open;
			}
			else if (currentFramePosition == 0f)
			{
				currentState = DoorState.Closed;
				doorComponent.IsOpen = false;
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		MainPlayerCmpt componentInParent = other.GetComponentInParent<MainPlayerCmpt>();
		if (!(componentInParent == null))
		{
			currentState = DoorState.Opening;
			doorComponent.IsOpen = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		MainPlayerCmpt componentInParent = other.GetComponentInParent<MainPlayerCmpt>();
		if (!(componentInParent == null))
		{
			currentState = DoorState.Closing;
		}
	}
}
