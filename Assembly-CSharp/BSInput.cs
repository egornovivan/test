using UnityEngine;

public class BSInput : MonoBehaviour
{
	public static Ray s_PickRay;

	public static Ray s_UIRay;

	public static bool s_MouseOnUI;

	public static bool s_Cancel;

	public static bool s_MouseLeftPress;

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

	public static bool s_Increase = false;

	public static bool s_Decrease = false;

	public static bool s_Undo = false;

	public static bool s_Redo = false;

	public static bool s_Delete = false;

	private static readonly Vector3[] c_directions = new Vector3[4]
	{
		Vector3.left,
		Vector3.right,
		Vector3.forward,
		Vector3.back
	};

	private static Vector3[] s_cameraDirs = new Vector3[4];

	private static int[] s_repIndex = new int[4];

	private static bool[] s_inputs = new bool[6];

	private void Update()
	{
		if (Camera.main != null)
		{
			s_PickRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		}
		if (UICamera.mainCamera != null)
		{
			s_UIRay = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
		}
		s_MouseOnUI = Physics.Raycast(s_UIRay, 1000f, 1073741824);
		if (Input.GetMouseButtonDown(1) && !VCEditor.s_Active)
		{
			s_RightDownPos = Input.mousePosition;
		}
		if (Input.GetMouseButtonUp(1) && !VCEditor.s_Active)
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
		s_MouseLeftPress = Input.GetMouseButton(0) && !VCEditor.s_Active;
		s_Shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !UICamera.inputHasFocus && !VCEditor.s_Active;
		s_Control = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !UICamera.inputHasFocus && !VCEditor.s_Active;
		s_Alt = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && !UICamera.inputHasFocus && !VCEditor.s_Active;
		if (Application.isEditor)
		{
			s_Undo = s_Shift && Input.GetKeyDown(KeyCode.Z) && !UICamera.inputHasFocus && !VCEditor.s_Active;
			s_Redo = s_Shift && Input.GetKeyDown(KeyCode.X) && !UICamera.inputHasFocus && !VCEditor.s_Active;
			s_Delete = Input.GetKeyDown(KeyCode.Comma) && !UICamera.inputHasFocus;
		}
		else
		{
			s_Undo = s_Control && Input.GetKeyDown(KeyCode.Z) && !UICamera.inputHasFocus && !VCEditor.s_Active;
			s_Redo = s_Control && Input.GetKeyDown(KeyCode.X) && !UICamera.inputHasFocus && !VCEditor.s_Active;
			s_Delete = Input.GetKeyDown(KeyCode.Delete) && !UICamera.inputHasFocus && !VCEditor.s_Active;
		}
		ref Vector3 reference = ref s_cameraDirs[0];
		reference = -Camera.main.transform.right;
		ref Vector3 reference2 = ref s_cameraDirs[1];
		reference2 = Camera.main.transform.right;
		ref Vector3 reference3 = ref s_cameraDirs[2];
		reference3 = Camera.main.transform.forward;
		ref Vector3 reference4 = ref s_cameraDirs[3];
		reference4 = -Camera.main.transform.forward;
		s_inputs[0] = PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_Lt);
		s_inputs[1] = PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_Rt);
		s_inputs[2] = PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_Up);
		s_inputs[3] = PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_Dn);
		s_inputs[4] = Input.GetKeyDown(KeyCode.PageUp);
		s_inputs[5] = Input.GetKeyDown(KeyCode.PageDown);
		for (int i = 0; i < 4; i++)
		{
			float num = 360f;
			int num2 = -1;
			for (int j = 0; j < 4; j++)
			{
				Vector3 vector = Vector3.Dot(Vector3.up, s_cameraDirs[j]) * Vector3.up;
				Vector3 normalized = (s_cameraDirs[j] - vector).normalized;
				float num3 = Vector3.Angle(c_directions[i], normalized);
				if (num3 < num)
				{
					num = num3;
					num2 = j;
				}
			}
			s_repIndex[i] = num2;
		}
		s_Left = s_inputs[s_repIndex[0]];
		s_Right = s_inputs[s_repIndex[1]];
		s_Forward = s_inputs[s_repIndex[2]];
		s_Back = s_inputs[s_repIndex[3]];
		s_Up = s_inputs[4];
		s_Down = s_inputs[5];
	}
}
