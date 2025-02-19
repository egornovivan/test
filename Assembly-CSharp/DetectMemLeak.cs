using UnityEngine;

public class DetectMemLeak : MonoBehaviour
{
	private int nObjs;

	private int nTexs;

	private int nAudios;

	private int nMats;

	private int nCompos;

	private int nGObjs;

	private int nMeshs;

	private int nGrassMeshs;

	private int nTreeMeshs;

	private int nOclMeshs;

	private int nChnkGos;

	private float lastUpdateTime;

	private void UpdateStatistics()
	{
		nObjs = Resources.FindObjectsOfTypeAll(typeof(Object)).Length;
		nTexs = Resources.FindObjectsOfTypeAll(typeof(Texture)).Length;
		nAudios = Resources.FindObjectsOfTypeAll(typeof(AudioClip)).Length;
		nMats = Resources.FindObjectsOfTypeAll(typeof(Material)).Length;
		nCompos = Resources.FindObjectsOfTypeAll(typeof(Component)).Length;
		nGObjs = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Length;
		Object[] array = Resources.FindObjectsOfTypeAll(typeof(Mesh));
		nMeshs = array.Length;
		nGrassMeshs = 0;
		nTreeMeshs = 0;
		nOclMeshs = 0;
		Object[] array2 = array;
		foreach (Object @object in array2)
		{
			if (@object.name.Equals("ocl_mesh"))
			{
				nOclMeshs++;
			}
			if (@object.name.Contains("tree"))
			{
				nTreeMeshs++;
			}
			else if (@object.name.Contains("grass"))
			{
				nGrassMeshs++;
			}
		}
		nChnkGos = Resources.FindObjectsOfTypeAll(typeof(VFVoxelChunkGo)).Length;
	}

	private void OnGUI()
	{
		if (Time.time > lastUpdateTime + 1f)
		{
			UpdateStatistics();
			lastUpdateTime = Time.time;
		}
		GUI.color = new Color(1f, 0f, 0f);
		GUILayout.BeginArea(new Rect(Screen.width - 256, 0f, Screen.width, 256f));
		GUILayout.Label("All-----------" + nObjs);
		GUILayout.Label("Textures------" + nTexs);
		GUILayout.Label("AudioClips----" + nAudios);
		GUILayout.Label("Materials-----" + nMats);
		GUILayout.Label("Components----" + nCompos);
		GUILayout.Label("GameObjects---" + nGObjs);
		GUILayout.Label("Meshes--------" + nMeshs + "(" + nOclMeshs + "/" + nGrassMeshs + "/" + nTreeMeshs + "/" + (nMeshs - nGrassMeshs - nTreeMeshs - nOclMeshs) + ")");
		GUILayout.Label("chks GObj-----" + nChnkGos);
		GUILayout.EndArea();
	}
}
