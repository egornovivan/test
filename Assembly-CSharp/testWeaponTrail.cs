using System.Collections.Generic;
using UnityEngine;

public class testWeaponTrail : MonoBehaviour
{
	public int splitCount = 10;

	public float lenth = 0.5f;

	public float lifeTime = 0.5f;

	public float bufferTime = 0.5f;

	public bool start = true;

	private Mesh mesh;

	private Vector3[] vertices;

	private Color[] colors;

	private Vector2[] uv;

	private int[] triangles;

	private MeshRenderer meshRender;

	private Material mat;

	private Color startColor = Color.white;

	private Color endColor = new Color(1f, 1f, 1f, 0f);

	private float time;

	private float needTime = 2f;

	private float timeTransitionSpeed = 1f;

	private Vector3 lastPos = Vector3.zero;

	private Vector3 lastFwd = Vector3.zero;

	private Vector3 currentPos = Vector3.zero;

	private Vector3 currentFwd = Vector3.zero;

	private float tempRadius;

	private float percent;

	private float u;

	private Vector3 tempFwd;

	private Vector3 tempStart;

	private TrailLattice tempLat;

	private List<TrailLattice> lattice = new List<TrailLattice>();

	private void Awake()
	{
		mesh = GetComponent<MeshFilter>().mesh;
		meshRender = GetComponent<MeshRenderer>();
		mat = meshRender.material;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (start)
		{
			StartTrail(lifeTime, bufferTime);
		}
		currentPos = base.transform.position;
		currentFwd = base.transform.forward;
	}

	private void LateUpdate()
	{
		float deltaTime = Mathf.Clamp(Time.deltaTime * 1f, 0f, 0.066f);
		Itterate();
		if (time > 0f)
		{
			UpdateTrail(Time.time, deltaTime);
		}
	}

	private void StartTrail(float timeToTweenTo, float fdTime)
	{
		needTime = timeToTweenTo;
		if (time != needTime)
		{
			timeTransitionSpeed = Mathf.Abs(needTime - time) / fdTime;
		}
		if (time <= 0f)
		{
			time = 0.01f;
		}
		currentPos = base.transform.position;
		currentFwd = base.transform.forward;
		lastPos = base.transform.position;
		lastFwd = base.transform.forward;
		start = false;
	}

	private void ClearTrail()
	{
		needTime = 0f;
		time = 0f;
		if (mesh != null)
		{
			mesh.Clear();
			lattice.Clear();
		}
		start = false;
	}

	private void UpdateTrail(float time_Time, float deltaTime)
	{
		mesh.Clear();
		while (lattice.Count > 0 && time_Time > lattice[lattice.Count - 1].time + time)
		{
			lattice.RemoveAt(lattice.Count - 1);
		}
		vertices = new Vector3[lattice.Count * 2];
		uv = new Vector2[lattice.Count * 2];
		colors = new Color[lattice.Count * 2];
		triangles = new int[lattice.Count * 6 - 6];
		for (int i = 0; i < lattice.Count; i++)
		{
			tempLat = lattice[i];
			ref Vector3 reference = ref vertices[i * 2];
			reference = base.transform.InverseTransformPoint(tempLat.startPos);
			ref Vector3 reference2 = ref vertices[i * 2 + 1];
			reference2 = base.transform.InverseTransformPoint(tempLat.endPos);
			u = Mathf.Clamp01((time_Time - tempLat.time) / time);
			ref Vector2 reference3 = ref uv[i * 2];
			reference3 = new Vector2(u, 0f);
			ref Vector2 reference4 = ref uv[i * 2 + 1];
			reference4 = new Vector2(u, 1f);
			ref Color reference5 = ref colors[i * 2 + 1];
			ref Color reference6 = ref colors[i * 2];
			reference5 = (reference6 = Color.Lerp(startColor, endColor, u));
		}
		for (int j = 0; j < lattice.Count - 1; j++)
		{
			triangles[j * 6] = j * 2;
			triangles[j * 6 + 1] = j * 2 + 1;
			triangles[j * 6 + 2] = j * 2 + 2;
			triangles[j * 6 + 3] = j * 2 + 2;
			triangles[j * 6 + 4] = j * 2 + 1;
			triangles[j * 6 + 5] = j * 2 + 3;
		}
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.colors = colors;
		mesh.triangles = triangles;
		if (time > needTime)
		{
			time -= deltaTime * timeTransitionSpeed;
			if (time <= needTime)
			{
				time = needTime;
			}
		}
		else if (time < needTime)
		{
			time += deltaTime * timeTransitionSpeed;
			if (time >= needTime)
			{
				time = needTime;
			}
		}
	}

	public void Itterate()
	{
		if (time <= 0f)
		{
			ClearTrail();
		}
		lattice.Insert(0, new TrailLattice(lastPos, lastFwd * lenth + lastPos, Time.time - Time.deltaTime));
		for (int i = 1; i < splitCount; i++)
		{
			percent = (float)i / ((float)splitCount - 1f);
			tempFwd = Vector3.Lerp(Vector3.Lerp(lastFwd, currentFwd, percent), base.transform.forward, percent);
			tempStart = Vector3.Lerp(Vector3.Lerp(lastPos, currentPos, percent), base.transform.position, percent);
			lattice.Insert(0, new TrailLattice(tempStart, tempStart + tempFwd * lenth, Time.time + (percent - 1f) * Time.deltaTime));
		}
		lastPos = base.transform.position;
		lastFwd = base.transform.forward;
	}
}
