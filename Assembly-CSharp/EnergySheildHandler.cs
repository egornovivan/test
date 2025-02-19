using UnityEngine;

public class EnergySheildHandler : MonoBehaviour
{
	public bool Demo = true;

	public Color MainColor = new Color(0f, 0.075f, 0.2f, 0.2f);

	public Color HoloColor = new Color(0.15f, 0.4f, 1f, 0.2f);

	public float Tile = 5f;

	public float BodyIntensity;

	public float HoloIntensity;

	public float WaveLength = 0.02f;

	public float Speed = 0.2f;

	private float HitTime1;

	private Vector3 HitPoint1 = Vector3.zero;

	private float HitTime2;

	private Vector3 HitPoint2 = Vector3.zero;

	private float HitTime3;

	private Vector3 HitPoint3 = Vector3.zero;

	private int Count;

	private void Start()
	{
	}

	private void Update()
	{
		if (Demo && Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out var hitInfo, 1000f) && hitInfo.collider == GetComponent<Collider>())
			{
				Impact(hitInfo.point);
			}
		}
		base.transform.rotation = Quaternion.identity;
		Renderer component = GetComponent<Renderer>();
		component.material.name = "Energy sheild instance";
		component.material.SetColor("_Color", MainColor);
		component.material.SetColor("_ImpactColor", HoloColor);
		component.material.SetFloat("_NoiseTile", Tile);
		component.material.SetFloat("_BodyIntensity", BodyIntensity);
		component.material.SetFloat("_HoloIntensity", HoloIntensity);
		component.material.SetFloat("_WaveLength", WaveLength);
		component.material.SetVector("_Impact1Point", new Vector4(HitPoint1.x, HitPoint1.y, HitPoint1.z, 0f));
		component.material.SetFloat("_Impact1Dist", Mathf.Sqrt((Time.time - HitTime1) * Speed) - 0.1f);
		component.material.SetVector("_Impact2Point", new Vector4(HitPoint2.x, HitPoint2.y, HitPoint2.z, 0f));
		component.material.SetFloat("_Impact2Dist", Mathf.Sqrt((Time.time - HitTime2) * Speed) - 0.1f);
		component.material.SetVector("_Impact3Point", new Vector4(HitPoint3.x, HitPoint3.y, HitPoint3.z, 0f));
		component.material.SetFloat("_Impact3Dist", Mathf.Sqrt((Time.time - HitTime3) * Speed) - 0.1f);
	}

	private void LateUpdate()
	{
		base.transform.rotation = Quaternion.identity;
	}

	public void Impact(Vector3 worldpos)
	{
		if (Count % 3 == 0)
		{
			HitTime1 = Time.time;
			HitPoint1 = base.transform.InverseTransformPoint(worldpos);
		}
		else if (Count % 3 == 1)
		{
			HitTime2 = Time.time;
			HitPoint2 = base.transform.InverseTransformPoint(worldpos);
		}
		else if (Count % 3 == 2)
		{
			HitTime3 = Time.time;
			HitPoint3 = base.transform.InverseTransformPoint(worldpos);
		}
		Count++;
	}
}
