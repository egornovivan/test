using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Pathea.IO;
using UnityEngine;

public class PeFmodEditor : MonoBehaviour
{
	public static bool active;

	private FMODAudioListener listener;

	private MovingGizmo giz_move;

	private RotatingGizmo giz_rotate;

	private GUISkin gskin;

	public static List<FMODAudioSource> savedAudios;

	private Rect windowRect = new Rect(200f, 100f, 265f, 200f);

	public static void Save()
	{
		if (savedAudios == null)
		{
			return;
		}
		string text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Audio Saves/";
		string path = text + "audio_points.xml";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string empty = string.Empty;
		empty += "<AUDIOLIST>\r\n";
		foreach (FMODAudioSource savedAudio in savedAudios)
		{
			empty += savedAudio.xml;
		}
		empty += "</AUDIOLIST>\r\n";
		FileUtil.SaveBytes(path, Encoding.UTF8.GetBytes(empty));
	}

	public static void Load()
	{
		if (savedAudios != null)
		{
			foreach (FMODAudioSource savedAudio in savedAudios)
			{
				Object.Destroy(savedAudio.gameObject);
			}
			savedAudios.Clear();
			savedAudios = null;
		}
		savedAudios = new List<FMODAudioSource>();
		string text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Audio Saves/";
		string text2 = text + "audio_points.xml";
		if (!File.Exists(text2))
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(text2);
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			if (!(childNode.Name == "AUDIO"))
			{
				continue;
			}
			Vector3 zero = Vector3.zero;
			zero.x = XmlConvert.ToSingle(childNode.Attributes["posx"].Value);
			zero.y = XmlConvert.ToSingle(childNode.Attributes["posy"].Value);
			zero.z = XmlConvert.ToSingle(childNode.Attributes["posz"].Value);
			Vector3 zero2 = Vector3.zero;
			zero2.x = XmlConvert.ToSingle(childNode.Attributes["rotx"].Value);
			zero2.y = XmlConvert.ToSingle(childNode.Attributes["roty"].Value);
			zero2.z = XmlConvert.ToSingle(childNode.Attributes["rotz"].Value);
			string value = childNode.Attributes["path"].Value;
			float volume = XmlConvert.ToSingle(childNode.Attributes["volume"].Value);
			float pitch = XmlConvert.ToSingle(childNode.Attributes["pitch"].Value);
			float minDistance = XmlConvert.ToSingle(childNode.Attributes["mindist"].Value);
			float maxDistance = XmlConvert.ToSingle(childNode.Attributes["maxdist"].Value);
			FMODAudioSource fMODAudioSource = CreateAudioSource(zero, zero2);
			fMODAudioSource.path = value;
			fMODAudioSource.volume = volume;
			fMODAudioSource.pitch = pitch;
			fMODAudioSource.minDistance = minDistance;
			fMODAudioSource.maxDistance = maxDistance;
			foreach (XmlNode childNode2 in childNode.ChildNodes)
			{
				if (childNode2.Name == "PARAM")
				{
					string value2 = childNode2.Attributes["name"].Value;
					float value3 = XmlConvert.ToSingle(childNode2.Attributes["value"].Value);
					fMODAudioSource.SetParam(value2, value3);
				}
			}
			savedAudios.Add(fMODAudioSource);
		}
	}

	public static void AlterSaveState(FMODAudioSourceRTE rte, bool state)
	{
		FMODAudioSource fMODAudioSource = savedAudios.Find((FMODAudioSource iter) => iter == rte.audioSrc);
		if (fMODAudioSource == null)
		{
			savedAudios.Add(rte.audioSrc);
			rte.gizmoColor = Color.green;
		}
		else
		{
			savedAudios.Remove(rte.audioSrc);
			rte.gizmoColor = Color.yellow;
		}
	}

	private void Start()
	{
		savedAudios = new List<FMODAudioSource>();
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Moving Gizmo")) as GameObject;
		gameObject.transform.parent = base.transform;
		giz_move = gameObject.GetComponent<MovingGizmo>();
		giz_move.MainCamera = Camera.main;
		gameObject.SetActive(value: false);
		GameObject gameObject2 = Object.Instantiate(Resources.Load("Prefabs/Rotating Gizmo")) as GameObject;
		gameObject2.transform.parent = base.transform;
		giz_rotate = gameObject2.GetComponent<RotatingGizmo>();
		giz_rotate.MainCamera = Camera.main;
		gameObject2.SetActive(value: false);
		FMODAudioSourceRTE.OnEditingStateChange += OnAudioEdit;
		FMODAudioSourceRTE.OnSave += AlterSaveState;
		gskin = Resources.Load<GUISkin>("AudioRTESkin");
	}

	private void Update()
	{
		if (!active)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (Input.GetMouseButtonDown(0) && FMODAudioSourceRTE.editing == null)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, 2000f, 1 << LayerMask.NameToLayer("PE Environment")))
			{
				FMODAudioSourceRTE componentInParent = hitInfo.collider.gameObject.GetComponentInParent<FMODAudioSourceRTE>();
				FMODAudioSourceRTE.selected = componentInParent;
			}
			else
			{
				FMODAudioSourceRTE.selected = null;
			}
		}
		if ((Input.GetKeyDown(KeyCode.Delete) && !Application.isEditor) || (Input.GetKeyDown(KeyCode.Comma) && Application.isEditor))
		{
			savedAudios.Remove(FMODAudioSourceRTE.selected.audioSrc);
			Object.Destroy(FMODAudioSourceRTE.selected.gameObject);
			FMODAudioSourceRTE.selected = null;
		}
	}

	private void OnDestroy()
	{
		FMODAudioSourceRTE.OnEditingStateChange -= OnAudioEdit;
		FMODAudioSourceRTE.OnSave -= AlterSaveState;
	}

	private void OnGUI()
	{
		GUI.depth = -10000;
		GUI.skin = gskin;
		windowRect = GUI.Window(6131504, windowRect, FMODWindow, "FMOD Editor");
	}

	private void FMODWindow(int id)
	{
		GUI.depth = -10000;
		GUI.DragWindow(new Rect(0f, 0f, 265f, 18f));
		if (GUI.Button(new Rect(15f, 30f, 110f, 25f), "Create Audio"))
		{
			Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 5f;
			if (Physics.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out var hitInfo, 5f))
			{
				pos = hitInfo.point;
			}
			CreateAudioSource(pos, Vector3.zero);
		}
		if (giz_move.gameObject.activeSelf)
		{
			GUI.color = Color.red;
		}
		if (GUI.Button(new Rect(15f, 65f, 110f, 25f), "Moving Gizmo"))
		{
			giz_move.gameObject.SetActive(value: true);
			giz_rotate.gameObject.SetActive(value: false);
		}
		GUI.color = Color.white;
		if (GUI.Button(new Rect(140f, 30f, 110f, 25f), "No Gizmo"))
		{
			giz_move.gameObject.SetActive(value: false);
			giz_rotate.gameObject.SetActive(value: false);
		}
		if (giz_rotate.gameObject.activeSelf)
		{
			GUI.color = Color.red;
		}
		if (GUI.Button(new Rect(140f, 65f, 110f, 25f), "Rotation Gizmo"))
		{
			giz_move.gameObject.SetActive(value: false);
			giz_rotate.gameObject.SetActive(value: true);
		}
		GUI.color = Color.white;
		if (GUI.Button(new Rect(15f, 125f, 110f, 25f), "Load"))
		{
			Load();
		}
		if (GUI.Button(new Rect(140f, 125f, 110f, 25f), "Save"))
		{
			Save();
		}
		GUI.Label(new Rect(17f, 163f, 250f, 25f), savedAudios.Count + " audio(s) will be saved.");
	}

	private static FMODAudioSource CreateAudioSource(Vector3 pos, Vector3 rot)
	{
		GameObject gameObject = GameObject.Find("Audio RTE Group");
		if (gameObject == null)
		{
			gameObject = new GameObject("Audio RTE Group");
			gameObject.transform.position = Vector3.zero;
		}
		GameObject gameObject2 = new GameObject("Audio Source");
		gameObject2.transform.position = pos;
		gameObject2.transform.eulerAngles = rot;
		gameObject2.transform.parent = gameObject.transform;
		return gameObject2.AddComponent<FMODAudioSource>();
	}

	private void OnAudioEdit(FMODAudioSourceRTE rte, bool edit)
	{
		giz_move.Targets.Clear();
		giz_rotate.Targets.Clear();
		if (edit)
		{
			giz_move.Targets.Add(rte.transform);
			giz_rotate.Targets.Add(rte.transform);
		}
	}
}
