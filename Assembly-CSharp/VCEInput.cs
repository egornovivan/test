using UnityEngine;

public class VCEInput : MonoBehaviour
{
	public static Ray s_PickRay;

	public static Ray s_UIRay;

	public static bool s_MouseOnUI;

	public static bool s_MouseLeftPress;

	public static bool s_Cancel;

	private static Vector3 s_RightDownPos;

	public static bool s_Shift;

	public static bool s_Alt;

	public static bool s_Control;

	public static bool s_Left;

	public static bool s_Right;

	public static bool s_Forward;

	public static bool s_Back;

	public static bool s_Up;

	public static bool s_Down;

	public static bool s_Increase;

	public static bool s_Decrease;

	public static bool s_Undo;

	public static bool s_Redo;

	public static bool s_Delete;

	public static KeyCode s_LeftKeyCode;

	public static KeyCode s_RightKeyCode;

	public static KeyCode s_ForwardKeyCode;

	public static KeyCode s_BackKeyCode;

	private static float s_IncreasePressTime;

	private static float s_DecreasePressTime;

	public static bool s_RightDblClick;

	private void Start()
	{
	}

	private void Update()
	{
		s_PickRay = VCEditor.Instance.m_MainCamera.ScreenPointToRay(Input.mousePosition);
		s_UIRay = VCEditor.Instance.m_UI.m_UICamera.ScreenPointToRay(Input.mousePosition);
		s_MouseOnUI = Physics.Raycast(s_UIRay, 1000f, VCConfig.s_UILayerMask);
		if (Input.GetMouseButtonDown(1))
		{
			s_RightDownPos = Input.mousePosition;
		}
		if (Input.GetMouseButtonUp(1))
		{
			if ((Input.mousePosition - s_RightDownPos).magnitude > 4.1f)
			{
				s_Cancel = false;
			}
			else
			{
				s_Cancel = true;
			}
		}
		else
		{
			s_Cancel = false;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			s_Cancel = true;
		}
		s_MouseLeftPress = Input.GetMouseButton(0);
		s_Shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !UICamera.inputHasFocus;
		s_Control = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !UICamera.inputHasFocus;
		s_Alt = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && !UICamera.inputHasFocus;
		if (Application.isEditor)
		{
			s_Undo = s_Shift && Input.GetKeyDown(KeyCode.Z) && !UICamera.inputHasFocus;
			s_Redo = s_Shift && Input.GetKeyDown(KeyCode.Y) && !UICamera.inputHasFocus;
			s_Delete = Input.GetKeyDown(KeyCode.Comma) && !UICamera.inputHasFocus;
		}
		else
		{
			s_Undo = s_Control && Input.GetKeyDown(KeyCode.Z) && !UICamera.inputHasFocus;
			s_Redo = s_Control && Input.GetKeyDown(KeyCode.Y) && !UICamera.inputHasFocus;
			s_Delete = Input.GetKeyDown(KeyCode.Delete) && !UICamera.inputHasFocus;
		}
		Vector3[] array = new Vector3[4]
		{
			Vector3.left,
			Vector3.right,
			Vector3.forward,
			Vector3.back
		};
		Vector3[] array2 = new Vector3[4]
		{
			-VCEditor.Instance.m_MainCamera.transform.right,
			VCEditor.Instance.m_MainCamera.transform.right,
			VCEditor.Instance.m_MainCamera.transform.forward,
			-VCEditor.Instance.m_MainCamera.transform.forward
		};
		int[] array3 = new int[4];
		bool[] array4 = new bool[6]
		{
			Input.GetKeyDown(KeyCode.LeftArrow),
			Input.GetKeyDown(KeyCode.RightArrow),
			Input.GetKeyDown(KeyCode.UpArrow),
			Input.GetKeyDown(KeyCode.DownArrow),
			Input.GetKeyDown(KeyCode.PageUp),
			Input.GetKeyDown(KeyCode.PageDown)
		};
		KeyCode[] array5 = new KeyCode[4]
		{
			KeyCode.LeftArrow,
			KeyCode.RightArrow,
			KeyCode.UpArrow,
			KeyCode.DownArrow
		};
		for (int i = 0; i < 4; i++)
		{
			float num = 360f;
			int num2 = -1;
			for (int j = 0; j < 4; j++)
			{
				Vector3 vector = Vector3.Dot(Vector3.up, array2[j]) * Vector3.up;
				Vector3 normalized = (array2[j] - vector).normalized;
				float num3 = Vector3.Angle(array[i], normalized);
				if (num3 < num)
				{
					num = num3;
					num2 = j;
				}
			}
			array3[i] = num2;
		}
		s_Left = array4[array3[0]];
		s_Right = array4[array3[1]];
		s_Forward = array4[array3[2]];
		s_Back = array4[array3[3]];
		s_Up = array4[4];
		s_Down = array4[5];
		s_LeftKeyCode = array5[array3[0]];
		s_RightKeyCode = array5[array3[1]];
		s_ForwardKeyCode = array5[array3[2]];
		s_BackKeyCode = array5[array3[3]];
		s_Increase = false;
		s_Decrease = false;
		if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.UpArrow))
		{
			s_Increase = true;
		}
		if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.DownArrow))
		{
			s_Decrease = true;
		}
		if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.UpArrow))
		{
			s_IncreasePressTime += Time.deltaTime;
		}
		else
		{
			s_IncreasePressTime = 0f;
		}
		if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.DownArrow))
		{
			s_DecreasePressTime += Time.deltaTime;
		}
		else
		{
			s_DecreasePressTime = 0f;
		}
		if (s_IncreasePressTime > 0.65f && Time.frameCount % 2 == 0)
		{
			s_Increase = true;
		}
		if (s_DecreasePressTime > 0.65f && Time.frameCount % 2 == 0)
		{
			s_Decrease = true;
		}
		s_RightDblClick = false;
	}
}
