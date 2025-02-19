using UnityEngine;

public static class UIMouseEvent
{
	private static int lastCheckOnAnyGUIFrame = -1;

	private static bool lastOnAnyGUI;

	private static int lastCheckOpAnyGUIFrame = -1;

	private static bool lastOpAnyGUI;

	private static int lastCheckOnAnyScrollFrame = -1;

	private static bool lastOnAnyScroll;

	private static int lastCheckOpAnyScrollFrame = -1;

	private static bool lastOpAnyScroll;

	public static bool onAnyGUI
	{
		get
		{
			if (Time.frameCount == lastCheckOnAnyGUIFrame)
			{
				return lastOnAnyGUI;
			}
			lastCheckOnAnyGUIFrame = Time.frameCount;
			if (UICamera.mainCamera != null)
			{
				Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
				lastOnAnyGUI = Physics.Raycast(ray, 512f, 1073741824);
				return lastOnAnyGUI;
			}
			lastOnAnyGUI = false;
			return false;
		}
	}

	public static bool opAnyGUI
	{
		get
		{
			if (Time.frameCount == lastCheckOpAnyGUIFrame)
			{
				return lastOpAnyGUI;
			}
			if (Input.GetMouseButton(0))
			{
				return lastOpAnyGUI;
			}
			if (Input.GetMouseButton(1))
			{
				return lastOpAnyGUI;
			}
			if (Input.GetMouseButton(2))
			{
				return lastOpAnyGUI;
			}
			lastCheckOpAnyGUIFrame = Time.frameCount;
			if (UICamera.mainCamera != null)
			{
				Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
				lastOpAnyGUI = Physics.Raycast(ray, 512f, 1073741824);
				return lastOpAnyGUI;
			}
			lastOpAnyGUI = false;
			return false;
		}
	}

	public static bool onAnyScroll
	{
		get
		{
			if (Time.frameCount == lastCheckOnAnyScrollFrame)
			{
				return lastOnAnyScroll;
			}
			lastCheckOnAnyScrollFrame = Time.frameCount;
			if (UICamera.mainCamera != null)
			{
				Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out var hitInfo, 512f, 1073741824))
				{
					if (hitInfo.collider.GetComponent<UIOnScrollMouse>() != null)
					{
						lastOnAnyScroll = true;
					}
					else
					{
						lastOnAnyScroll = false;
					}
				}
				else
				{
					lastOnAnyScroll = false;
				}
				return lastOnAnyScroll;
			}
			lastOnAnyScroll = false;
			return false;
		}
	}

	public static bool opAnyScroll
	{
		get
		{
			if (Time.frameCount == lastCheckOpAnyScrollFrame)
			{
				return lastOpAnyScroll;
			}
			if (Input.GetMouseButton(0))
			{
				return lastOpAnyScroll;
			}
			if (Input.GetMouseButton(1))
			{
				return lastOpAnyScroll;
			}
			if (Input.GetMouseButton(2))
			{
				return lastOpAnyScroll;
			}
			lastCheckOpAnyScrollFrame = Time.frameCount;
			if (UICamera.mainCamera != null)
			{
				Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out var hitInfo, 512f, 1073741824))
				{
					if (hitInfo.collider.GetComponent<UIOnScrollMouse>() != null)
					{
						lastOpAnyScroll = true;
					}
					else
					{
						lastOpAnyScroll = false;
					}
				}
				else
				{
					lastOpAnyScroll = false;
				}
				return lastOpAnyScroll;
			}
			lastOpAnyScroll = false;
			return false;
		}
	}
}
