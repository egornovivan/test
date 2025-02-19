using UnityEngine;

public class UIButtonEffect : MonoBehaviour
{
	[SerializeField]
	private UISpecularHandler ButtonTex;

	[SerializeField]
	private Shader BraceShader;

	[SerializeField]
	private Texture2D BraceTex;

	private UITexture BraceLT;

	private UITexture BraceLB;

	private UITexture BraceRT;

	private UITexture BraceRB;

	private float t;

	[SerializeField]
	private float During = 0.4f;

	[SerializeField]
	private float Direction;

	[SerializeField]
	private AnimationCurve OffsetChange;

	[SerializeField]
	private Gradient ColorChange;

	private bool isMouseDown;

	public void MouseEnter()
	{
		Direction = 1f;
	}

	public void MouseLeave()
	{
		Direction = -2f;
	}

	public void MouseDown()
	{
		isMouseDown = true;
	}

	public void MouseUp()
	{
		isMouseDown = false;
	}

	private void Awake()
	{
		GameObject gameObject = new GameObject("brace");
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		BraceLT = gameObject.AddComponent<UITexture>();
		BraceLT.shader = BraceShader;
		BraceLT.mainTexture = BraceTex;
		gameObject = new GameObject("brace");
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		BraceLB = gameObject.AddComponent<UITexture>();
		BraceLB.shader = BraceShader;
		BraceLB.mainTexture = BraceTex;
		gameObject = new GameObject("brace");
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		BraceRT = gameObject.AddComponent<UITexture>();
		BraceRT.shader = BraceShader;
		BraceRT.mainTexture = BraceTex;
		gameObject = new GameObject("brace");
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		BraceRB = gameObject.AddComponent<UITexture>();
		BraceRB.shader = BraceShader;
		BraceRB.mainTexture = BraceTex;
		BraceLT.transform.localScale = new Vector3(8f, 24f, 1f);
		BraceLB.transform.localScale = new Vector3(8f, -24f, 1f);
		BraceRT.transform.localScale = new Vector3(-8f, 24f, 1f);
		BraceRB.transform.localScale = new Vector3(-8f, -24f, 1f);
	}

	private void Reset()
	{
		t = 0f;
		Direction = 0f;
		if (BraceLT != null)
		{
			BraceLT.gameObject.SetActive(value: false);
		}
		if (BraceLB != null)
		{
			BraceLB.gameObject.SetActive(value: false);
		}
		if (BraceRT != null)
		{
			BraceRT.gameObject.SetActive(value: false);
		}
		if (BraceRB != null)
		{
			BraceRB.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		Reset();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void OnDestroy()
	{
		if (BraceLT != null)
		{
			Object.Destroy(BraceLT.gameObject);
		}
		if (BraceLB != null)
		{
			Object.Destroy(BraceLB.gameObject);
		}
		if (BraceRT != null)
		{
			Object.Destroy(BraceRT.gameObject);
		}
		if (BraceRB != null)
		{
			Object.Destroy(BraceRB.gameObject);
		}
	}

	private void Update()
	{
		t += Time.deltaTime / During * Direction;
		t = Mathf.Clamp01(t);
		if (t <= 0f)
		{
			BraceLT.gameObject.SetActive(value: false);
			BraceLB.gameObject.SetActive(value: false);
			BraceRT.gameObject.SetActive(value: false);
			BraceRB.gameObject.SetActive(value: false);
		}
		else
		{
			BraceLT.gameObject.SetActive(value: true);
			BraceLB.gameObject.SetActive(value: true);
			BraceRT.gameObject.SetActive(value: true);
			BraceRB.gameObject.SetActive(value: true);
		}
		float num = ((!isMouseDown) ? 1f : 0.7f);
		float z = ((!isMouseDown) ? 0f : 180f);
		float num2 = ((!isMouseDown) ? 0f : (-1f));
		Color black = Color.black;
		black = ((!(Direction > 0.001f)) ? (ColorChange.Evaluate(1f) * t) : ColorChange.Evaluate(t));
		black *= num;
		BraceLT.color = black;
		BraceLB.color = black;
		BraceRT.color = black;
		BraceRB.color = black;
		ButtonTex.transform.eulerAngles = new Vector3(0f, 0f, z);
		Vector3 localScale = ButtonTex.transform.localScale;
		float num3 = localScale.x / 2f - 2f + Mathf.Max(0f, OffsetChange.Evaluate(t)) + num2;
		float num4 = localScale.y / 2f - 7f + Mathf.Max(0f, OffsetChange.Evaluate(t)) + num2;
		float z2 = ButtonTex.transform.localPosition.z;
		BraceLT.transform.localPosition = new Vector3(0f - num3, num4, z2);
		BraceLB.transform.localPosition = new Vector3(0f - num3, 0f - num4, z2);
		BraceRT.transform.localPosition = new Vector3(num3, num4, z2);
		BraceRB.transform.localPosition = new Vector3(num3, 0f - num4, z2);
	}
}
