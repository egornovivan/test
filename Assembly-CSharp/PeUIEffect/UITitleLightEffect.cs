namespace PeUIEffect;

public class UITitleLightEffect : UIEffect
{
	private UITexture tex;

	private void Awake()
	{
		tex = GetComponent<UITexture>();
	}

	public override void Play()
	{
		base.Play();
	}

	public override void End()
	{
		base.End();
	}

	public void Update()
	{
		if (!m_Runing)
		{
		}
	}
}
