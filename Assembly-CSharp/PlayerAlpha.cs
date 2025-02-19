using UnityEngine;

public class PlayerAlpha : MonoBehaviour
{
	protected float targetAlpha;

	protected GameObject gameObj;

	public bool isDormant;

	protected SkinnedMeshRenderer render;

	protected Shader[] originalShaders;

	protected Shader transparent;

	public virtual void Start()
	{
		render = null;
		isDormant = true;
		gameObj = base.gameObject;
		render = base.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		originalShaders = new Shader[render.materials.Length];
		for (int i = 0; i < render.materials.Length; i++)
		{
			originalShaders[i] = render.materials[i].shader;
		}
		transparent = Shader.Find("Transparent/Bumped Diffuse");
	}

	public float GetCurrentAlpha()
	{
		if (render == null)
		{
			return 0f;
		}
		return render.material.color.a;
	}

	public void setTargetAlpha(float _targetAlpha)
	{
		if (Mathf.Abs(_targetAlpha - targetAlpha) < float.Epsilon)
		{
			return;
		}
		if (isDormant)
		{
			for (int i = 0; i < render.materials.Length; i++)
			{
				render.materials[i].shader = transparent;
			}
		}
		isDormant = false;
		targetAlpha = _targetAlpha;
	}

	protected virtual void UpdateRenderAlpha()
	{
		if (isDormant)
		{
			return;
		}
		float a = render.material.color.a;
		a = ((Mathf.Abs(a - targetAlpha) <= 0.01f) ? targetAlpha : ((!(a < targetAlpha)) ? (a - 0.01f) : (a + 0.01f)));
		for (int i = 0; i < render.materials.Length; i++)
		{
			Color color = render.materials[i].color;
			render.materials[i].color = new Color(color.r, color.g, color.b, a);
		}
		if (Mathf.Abs(a - 1f) <= 0.0001f)
		{
			for (int j = 0; j < render.materials.Length; j++)
			{
				render.materials[j].shader = originalShaders[j];
			}
			isDormant = true;
		}
	}

	public virtual void Update()
	{
		UpdateRenderAlpha();
	}
}
