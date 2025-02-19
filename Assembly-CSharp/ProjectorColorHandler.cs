using UnityEngine;

[RequireComponent(typeof(Projector))]
public class ProjectorColorHandler : MonoBehaviour
{
	public Material sourceMat;

	public Color mainColor;

	public Color origColor = Color.clear;

	public bool flare;

	public float flareDuratione;

	private bool isFlaring;

	private bool forwad;

	private Material _projMat;

	private Projector _Projector;

	private Color curColor;

	private float velocity;

	private void Start()
	{
		_projMat = Object.Instantiate(sourceMat);
		_Projector = base.gameObject.GetComponent<Projector>();
		_Projector.material = _projMat;
	}

	private void Update()
	{
		_projMat.SetColor("_TintColor", mainColor);
		Vector3 position = base.transform.position;
		_projMat.SetVector("_CenterAndRadius", new Vector4(position.x, position.y, position.z, _Projector.orthographicSize));
		if (flare && !isFlaring)
		{
			curColor = origColor;
			isFlaring = true;
			forwad = true;
		}
		if (isFlaring)
		{
			float smoothTime = 0.5f * flareDuratione;
			if (forwad)
			{
				curColor.r = Mathf.SmoothDamp(curColor.r, mainColor.r, ref velocity, smoothTime);
				curColor.g = Mathf.SmoothDamp(curColor.g, mainColor.g, ref velocity, smoothTime);
				curColor.b = Mathf.SmoothDamp(curColor.b, mainColor.b, ref velocity, smoothTime);
				curColor.a = mainColor.a;
				if (Mathf.Abs(curColor.r - mainColor.r) < 0.001f && Mathf.Abs(curColor.b - mainColor.b) < 0.001f && Mathf.Abs(curColor.g - mainColor.g) < 0.001f)
				{
					forwad = false;
					curColor = mainColor;
				}
				_projMat.SetColor("_TintColor", curColor);
				return;
			}
			curColor.r = Mathf.SmoothDamp(curColor.r, origColor.r, ref velocity, smoothTime);
			curColor.g = Mathf.SmoothDamp(curColor.g, origColor.g, ref velocity, smoothTime);
			curColor.b = Mathf.SmoothDamp(curColor.b, origColor.b, ref velocity, smoothTime);
			curColor.a = origColor.a;
			if (Mathf.Abs(curColor.r - origColor.r) < 0.001f && Mathf.Abs(curColor.b - origColor.b) < 0.001f && Mathf.Abs(curColor.g - origColor.g) < 0.001f)
			{
				flare = false;
				isFlaring = false;
				base.gameObject.SetActive(value: false);
			}
			_projMat.SetColor("_TintColor", curColor);
		}
		else
		{
			_projMat.SetColor("_TintColor", origColor);
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		_Projector.material = null;
		Object.Destroy(_projMat);
	}
}
