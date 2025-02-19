using System.Collections.Generic;
using UnityEngine;

public class WeaponTrail : MonoBehaviour
{
	public float height = 2f;

	public float time = 2f;

	public bool alwaysUp;

	public float minDistance = 0.05f;

	public float timeTransitionSpeed = 1f;

	public float desiredTime = 2f;

	private Color startColor = Color.white;

	private Color endColor = new Color(1f, 1f, 1f, 0f);

	private Vector3 currentposition;

	private float now;

	private TronTrailSection currentSection;

	private Matrix4x4 localSpaceTransform;

	private Mesh mesh;

	private Vector3[] vertices;

	private Color[] colors;

	private Vector2[] uv;

	private MeshRenderer meshRenderer;

	private Material trailMaterial;

	private List<TronTrailSection> sections = new List<TronTrailSection>();

	private Vector3 mLastFramePos = Vector3.zero;

	private Quaternion mLastFrameRot = Quaternion.identity;

	private Vector3 mCurrentStartPos = Vector3.zero;

	private Quaternion mCurrentStartRot = Quaternion.identity;

	private void Awake()
	{
		MeshFilter meshFilter = GetComponent(typeof(MeshFilter)) as MeshFilter;
		mesh = meshFilter.mesh;
		meshRenderer = GetComponent(typeof(MeshRenderer)) as MeshRenderer;
		trailMaterial = meshRenderer.material;
	}

	public void StartTrail(float timeToTweenTo, float fadeInTime)
	{
		desiredTime = timeToTweenTo;
		if (time != desiredTime)
		{
			timeTransitionSpeed = Mathf.Abs(desiredTime - time) / fadeInTime;
		}
		if (time <= 0f)
		{
			time = 0.01f;
		}
		mLastFramePos = (mCurrentStartPos = base.transform.position);
		mLastFrameRot = (mCurrentStartRot = base.transform.rotation);
		base.gameObject.SetActive(value: true);
		if (!meshRenderer.enabled)
		{
			meshRenderer.enabled = true;
		}
	}

	public void SetTime(float trailTime, float timeToTweenTo, float tweenSpeed)
	{
		time = trailTime;
		desiredTime = timeToTweenTo;
		timeTransitionSpeed = tweenSpeed;
		if (time <= 0f)
		{
			ClearTrail();
		}
	}

	public void FadeOut(float fadeTime)
	{
		desiredTime = 0f;
		if (time > 0f)
		{
			timeTransitionSpeed = time / fadeTime;
		}
	}

	public void SetTrailColor(Color color)
	{
		trailMaterial.SetColor("_TintColor", color);
	}

	public void Itterate(float itterateTime)
	{
		if (!(mCurrentStartPos != base.transform.position))
		{
			return;
		}
		if (time <= 0f)
		{
			ClearTrail();
		}
		currentposition = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		int num = 10;
		for (int i = 0; i < num; i++)
		{
			float t = (float)i / ((float)num - 1f);
			base.transform.position = Vector3.Lerp(Vector3.Lerp(mLastFramePos, mCurrentStartPos, t), currentposition, t);
			base.transform.rotation = Quaternion.Lerp(Quaternion.Lerp(mLastFrameRot, mCurrentStartRot, t), rotation, t);
			now = Time.time + ((float)i / ((float)num - 1f) - 1f) * Time.deltaTime;
			TronTrailSection tronTrailSection = new TronTrailSection();
			tronTrailSection.point = base.transform.position;
			if (alwaysUp)
			{
				tronTrailSection.upDir = Vector3.up;
			}
			else
			{
				tronTrailSection.upDir = base.transform.TransformDirection(Vector3.up);
			}
			tronTrailSection.time = now;
			sections.Insert(0, tronTrailSection);
		}
		mLastFramePos = base.transform.position;
		mLastFrameRot = base.transform.rotation;
	}

	public void UpdateTrail(float currentTime, float deltaTime)
	{
		base.transform.localPosition = Vector3.zero;
		mesh.Clear();
		while (sections.Count > 0 && currentTime > sections[sections.Count - 1].time + time)
		{
			sections.RemoveAt(sections.Count - 1);
		}
		if (sections.Count < 2)
		{
			return;
		}
		vertices = new Vector3[sections.Count * 2];
		colors = new Color[sections.Count * 2];
		uv = new Vector2[sections.Count * 2];
		localSpaceTransform = base.transform.worldToLocalMatrix;
		for (int i = 0; i < sections.Count; i++)
		{
			currentSection = sections[i];
			float num = 0f;
			if (i != 0)
			{
				num = Mathf.Clamp01((currentTime - currentSection.time) / time);
			}
			Vector3 upDir = currentSection.upDir;
			ref Vector3 reference = ref vertices[i * 2];
			reference = localSpaceTransform.MultiplyPoint(currentSection.point);
			ref Vector3 reference2 = ref vertices[i * 2 + 1];
			reference2 = localSpaceTransform.MultiplyPoint(currentSection.point + upDir * height);
			ref Vector2 reference3 = ref uv[i * 2];
			reference3 = new Vector2(num, 0f);
			ref Vector2 reference4 = ref uv[i * 2 + 1];
			reference4 = new Vector2(num, 1f);
			Color color = Color.Lerp(startColor, endColor, num);
			colors[i * 2] = color;
			colors[i * 2 + 1] = color;
		}
		int[] array = new int[(sections.Count - 1) * 2 * 3];
		for (int j = 0; j < array.Length / 6; j++)
		{
			array[j * 6] = j * 2;
			array[j * 6 + 1] = j * 2 + 1;
			array[j * 6 + 2] = j * 2 + 2;
			array[j * 6 + 3] = j * 2 + 2;
			array[j * 6 + 4] = j * 2 + 1;
			array[j * 6 + 5] = j * 2 + 3;
		}
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.uv = uv;
		mesh.triangles = array;
		if (time > desiredTime)
		{
			time -= deltaTime * timeTransitionSpeed;
			if (time <= desiredTime)
			{
				time = desiredTime;
			}
		}
		else if (time < desiredTime)
		{
			time += deltaTime * timeTransitionSpeed;
			if (time >= desiredTime)
			{
				time = desiredTime;
			}
		}
	}

	public void ClearTrail()
	{
		desiredTime = 0f;
		time = 0f;
		if (mesh != null)
		{
			mesh.Clear();
			sections.Clear();
		}
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		mCurrentStartPos = base.transform.position;
		mCurrentStartRot = base.transform.rotation;
	}
}
