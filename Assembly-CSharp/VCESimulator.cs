using UnityEngine;

public class VCESimulator : MonoBehaviour
{
	private void OnGUI()
	{
		if (!VCEditor.s_Active && GUI.Button(new Rect((float)(Screen.width - 300) * 0.5f, (float)(Screen.height - 70) * 0.5f, 300f, 70f), "Voxel Creation Editor"))
		{
			VCEditor.Open();
		}
	}
}
