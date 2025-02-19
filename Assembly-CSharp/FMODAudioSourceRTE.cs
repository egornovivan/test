using FMOD.Studio;
using UnityEngine;
using UnityEngine.Rendering;

public class FMODAudioSourceRTE : MonoBehaviour
{
	public delegate void DNotify(FMODAudioSourceRTE rte, bool state);

	public FMODAudioSource audioSrc;

	private GameObject gizmo;

	private Collider col;

	private Renderer r;

	private Material mat;

	private GUISkin gskin;

	private GameObject bound;

	private Material boundmat;

	public static FMODAudioSourceRTE selected;

	public static FMODAudioSourceRTE editing;

	public static bool showEditingPanel = true;

	private float listenDist = float.PositiveInfinity;

	public Color gizmoColor = Color.yellow;

	private string inputPath = "event:/";

	private bool boundactive;

	public static FMODAudioListener listener => FMODAudioListener.listener;

	public static event DNotify OnEditingStateChange;

	public static event DNotify OnSave;

	private void Start()
	{
		audioSrc = GetComponent<FMODAudioSource>();
		gizmo = GameObject.CreatePrimitive(PrimitiveType.Quad);
		gizmo.transform.parent = base.transform;
		gizmo.transform.localPosition = Vector3.zero;
		r = gizmo.GetComponent<Renderer>();
		mat = Object.Instantiate(Resources.Load<Material>("AudioGizmoMat"));
		r.material = mat;
		mat.color = Color.yellow;
		col = gizmo.GetComponent<Collider>();
		(col as MeshCollider).convex = true;
		col.isTrigger = true;
		bound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Collider component = bound.GetComponent<Collider>();
		Object.DestroyImmediate(component);
		bound.transform.parent = base.transform;
		bound.transform.localPosition = Vector3.zero;
		Renderer component2 = bound.GetComponent<Renderer>();
		boundmat = Object.Instantiate(Resources.Load<Material>("AudioBoundMat"));
		component2.shadowCastingMode = ShadowCastingMode.Off;
		component2.receiveShadows = false;
		component2.material = boundmat;
		bound.SetActive(value: false);
		gizmo.layer = LayerMask.NameToLayer("PE Environment");
		gizmo.transform.rotation = Camera.main.transform.rotation;
		gskin = Resources.Load<GUISkin>("AudioRTESkin");
		if (PeFmodEditor.savedAudios != null)
		{
			FMODAudioSource fMODAudioSource = PeFmodEditor.savedAudios.Find((FMODAudioSource iter) => iter == audioSrc);
			if (fMODAudioSource != null)
			{
				gizmoColor = Color.green;
			}
		}
		if (audioSrc.audioInst != null)
		{
			audioSrc.audioInst.getDescription(out var description);
			description.getPath(out inputPath);
		}
	}

	private void OnDisable()
	{
		if (selected == this)
		{
			selected = null;
			editing = null;
			if (FMODAudioSourceRTE.OnEditingStateChange != null)
			{
				FMODAudioSourceRTE.OnEditingStateChange(this, state: false);
			}
		}
	}

	private void OnDestroy()
	{
		if (selected == this)
		{
			selected = null;
			editing = null;
			if (FMODAudioSourceRTE.OnEditingStateChange != null)
			{
				FMODAudioSourceRTE.OnEditingStateChange(this, state: false);
			}
		}
		Object.Destroy(gizmo);
		Object.Destroy(bound);
		Object.Destroy(mat);
		Object.Destroy(boundmat);
	}

	private void Update()
	{
		listenDist = float.PositiveInfinity;
		if (listener != null)
		{
			listenDist = Vector3.Distance(listener.transform.position, base.transform.position);
		}
		gizmo.transform.rotation = Camera.main.transform.rotation;
		if (selected == this)
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				if (editing == this)
				{
					editing = null;
				}
				else
				{
					editing = this;
				}
				if (FMODAudioSourceRTE.OnEditingStateChange != null)
				{
					FMODAudioSourceRTE.OnEditingStateChange(this, editing == this);
				}
			}
			if (Input.GetKeyDown(KeyCode.KeypadPlus) && FMODAudioSourceRTE.OnSave != null)
			{
				FMODAudioSourceRTE.OnSave(this, state: true);
			}
		}
		mat.color = ((!(selected == this)) ? new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoColor.a * 0.7f) : Color.Lerp(gizmoColor, Color.white, 0.8f));
		bound.SetActive(boundactive);
	}

	private void OnGUI()
	{
		boundactive = false;
		GUI.depth = -5000;
		if (!(editing == this))
		{
			return;
		}
		if (Vector3.Dot(Camera.main.transform.forward, (base.transform.position - Camera.main.transform.position).normalized) > 0.1f)
		{
			GUI.skin = gskin;
			Vector3 vector = Camera.main.WorldToScreenPoint(base.transform.position);
			vector.x -= 30f;
			vector.y -= 25f;
			int num = 120;
			if (audioSrc.audioInst != null)
			{
				int count = 0;
				audioSrc.audioInst.getParameterCount(out count);
				num = 304 + 25 * count;
			}
			GUI.BeginGroup(new Rect(vector.x, (float)Screen.height - vector.y, 300f, num));
			GUI.Box(new Rect(0f, 0f, 300f, num), string.Empty);
			GUILayout.BeginHorizontal();
			GUILayout.Space(5f);
			GUILayout.BeginVertical();
			GUILayout.Space(8f);
			GUILayout.Label("Load Event");
			GUILayout.BeginHorizontal();
			GUILayout.Space(13f);
			inputPath = GUILayout.TextField(inputPath, GUILayout.Width(200f));
			if (GUILayout.Button("Load", GUILayout.Width(50f)))
			{
				audioSrc.path = inputPath;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(15f);
			GUILayout.Label("Audio Settings");
			if (audioSrc.audioInst != null)
			{
				EventDescription description = null;
				audioSrc.audioInst.getDescription(out description);
				if (description != null && description.isValid())
				{
					bool is3D = false;
					description.is3D(out is3D);
					bool oneshot = false;
					description.isOneshot(out oneshot);
					float distance = 0f;
					float distance2 = 0f;
					if (is3D)
					{
						description.getMinimumDistance(out distance);
						description.getMaximumDistance(out distance2);
						boundactive = true;
						bound.transform.localScale = Vector3.one * audioSrc.maxDistance * 2f;
					}
					string label = ((!is3D) ? "2D Sound" : "3D Sound");
					string text = ((!is3D) ? string.Empty : ("Distance Area ( " + distance.ToString("0.##") + "m ~ " + distance2.ToString("0.##") + "m )"));
					if (listener != null)
					{
						LabelField("Distance", listenDist.ToString("#,##0.00") + " m");
					}
					else
					{
						LabelField("Distance", "-");
					}
					LabelField(label, text);
					audioSrc.minDistance = FloatField("Min Dist", audioSrc.minDistance);
					audioSrc.maxDistance = FloatField("Max Dist", audioSrc.maxDistance);
					LabelField("Is Oneshot", oneshot.ToString());
					audioSrc.volume = Slider("Volume", audioSrc.volume, 0f, 1f);
					audioSrc.pitch = Slider("Pitch", audioSrc.pitch, 0f, 4f);
					int count2 = 0;
					audioSrc.audioInst.getParameterCount(out count2);
					for (int i = 0; i < count2; i++)
					{
						ParameterInstance instance = null;
						audioSrc.audioInst.getParameterByIndex(i, out instance);
						PARAMETER_DESCRIPTION description2 = default(PARAMETER_DESCRIPTION);
						instance.getDescription(out description2);
						float value = 0f;
						float num2 = 0f;
						instance.getValue(out value);
						num2 = Slider(description2.name, value, description2.minimum, description2.maximum);
						if (num2 != value)
						{
							instance.setValue(num2);
						}
					}
					GUILayout.Space(8f);
					GUILayout.BeginHorizontal();
					GUILayout.Space(17f);
					if (GUILayout.Button("Play", GUILayout.Width(80f)))
					{
						audioSrc.Play();
					}
					GUILayout.Space(4f);
					if (GUILayout.Button("Stop", GUILayout.Width(80f)))
					{
						audioSrc.Stop();
					}
					GUILayout.Space(4f);
					if (GUILayout.Button("Unload", GUILayout.Width(80f)))
					{
						audioSrc.path = string.Empty;
					}
					GUILayout.EndHorizontal();
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(13f);
				GUILayout.Label("No Event Loaded");
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUI.EndGroup();
		}
		else if (audioSrc.is3D)
		{
			boundactive = true;
			bound.transform.localScale = Vector3.one * audioSrc.maxDistance * 2f;
		}
	}

	private void LabelField(string label, string text)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(13f);
		GUILayout.Label(label, GUILayout.Width(66f));
		GUILayout.Label(text);
		GUILayout.EndHorizontal();
	}

	private string TextField(string label, string text)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(13f);
		GUILayout.Label(label, GUILayout.Width(66f));
		string result = GUILayout.TextField(text, GUILayout.Width(180f));
		GUILayout.EndHorizontal();
		return result;
	}

	private float FloatField(string label, float value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(13f);
		GUILayout.Label(label, GUILayout.Width(66f));
		string s = GUILayout.TextField(value.ToString(), GUILayout.Width(180f));
		float result = value;
		GUILayout.EndHorizontal();
		if (float.TryParse(s, out result))
		{
			return result;
		}
		return value;
	}

	private float Slider(string label, float value, float min, float max)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(13f);
		GUILayout.Label(label, GUILayout.Width(66f));
		GUILayout.BeginVertical(GUILayout.Width(130f));
		GUILayout.Space(9f);
		value = GUILayout.HorizontalSlider(value, min, max);
		GUILayout.EndVertical();
		GUILayout.Space(5f);
		GUI.SetNextControlName(label + " edit");
		string s = GUILayout.TextField(value.ToString("0.##"), GUILayout.Width(55f));
		float result = value;
		if (float.TryParse(s, out result))
		{
			value = Mathf.Clamp(result, min, max);
		}
		GUILayout.EndHorizontal();
		return value;
	}
}
