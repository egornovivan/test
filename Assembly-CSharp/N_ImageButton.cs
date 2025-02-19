using UnityEngine;

[ExecuteInEditMode]
public class N_ImageButton : UIComponent
{
	private UILabel lable;

	private UISprite spr;

	private BoxCollider boxCollider;

	private UISpecularHandler texHandler;

	private UIButtonEffect effect;

	public float lbAlphaFlag = 2f;

	public float normalItensity = 0.35f;

	public float hoverItensity = 0.7f;

	public float pressedItensity = 0.8f;

	public float disableItensity = 0.15f;

	private bool m_Init;

	public UIConpomentEvent e_OnClick = new UIConpomentEvent();

	private bool _disable;

	private float lbNormal => lbAlphaFlag * normalItensity;

	private float lbHover => lbAlphaFlag * hoverItensity;

	private float lbPressed => lbAlphaFlag * pressedItensity;

	private float lbDisable => lbAlphaFlag * disableItensity;

	public bool isEnabled
	{
		get
		{
			return !_disable;
		}
		set
		{
			disable = !value;
		}
	}

	public bool disable
	{
		get
		{
			return _disable;
		}
		set
		{
			Init();
			if (_disable != value)
			{
				_disable = value;
				if (boxCollider == null)
				{
					boxCollider = GetComponent<BoxCollider>();
				}
				if (boxCollider != null)
				{
					boxCollider.enabled = !_disable;
				}
				if (texHandler != null)
				{
					texHandler.Intensity = ((!_disable) ? normalItensity : disableItensity);
				}
				if (spr != null)
				{
					spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, (!_disable) ? lbNormal : lbDisable);
				}
				if (lable != null)
				{
					lable.color = new Color(lable.color.r, lable.color.g, lable.color.b, (!_disable) ? lbNormal : lbDisable);
				}
				if (effect != null)
				{
					effect.enabled = !_disable;
				}
			}
		}
	}

	private void OnEnable()
	{
		Init();
		if (disable)
		{
			return;
		}
		bool flag = UICamera.IsHighlighted(base.gameObject);
		if (spr != null)
		{
			spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, (!flag) ? lbNormal : lbHover);
		}
		if (lable != null)
		{
			lable.color = new Color(lable.color.r, lable.color.g, lable.color.b, (!flag) ? lbNormal : lbHover);
		}
		if (texHandler != null)
		{
			texHandler.Intensity = ((!flag) ? normalItensity : hoverItensity);
		}
		if (effect != null)
		{
			if (flag)
			{
				effect.MouseEnter();
			}
			else
			{
				effect.MouseLeave();
			}
		}
	}

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (m_Init)
		{
			return;
		}
		if (spr == null)
		{
			UISprite[] componentsInChildren = GetComponentsInChildren<UISprite>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				spr = componentsInChildren[0];
			}
		}
		if (lable == null)
		{
			UILabel[] componentsInChildren2 = GetComponentsInChildren<UILabel>(includeInactive: true);
			if (componentsInChildren2 != null && componentsInChildren2.Length > 0)
			{
				lable = componentsInChildren2[0];
			}
		}
		if (texHandler == null)
		{
			UISpecularHandler[] componentsInChildren3 = GetComponentsInChildren<UISpecularHandler>(includeInactive: true);
			if (componentsInChildren3 != null && componentsInChildren3.Length > 0)
			{
				texHandler = componentsInChildren3[0];
			}
		}
		if (boxCollider == null)
		{
			boxCollider = GetComponent<BoxCollider>();
		}
		if (effect == null)
		{
			effect = GetComponent<UIButtonEffect>();
		}
		if (lable != null)
		{
			lable.color = new Color(lable.color.r, lable.color.g, lable.color.b, lbNormal);
		}
		if (texHandler != null)
		{
			texHandler.Intensity = normalItensity;
		}
		m_Init = true;
	}

	private void OnHover(bool isOver)
	{
		if (!base.enabled || _disable)
		{
			return;
		}
		if (spr != null)
		{
			spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, (!isOver) ? lbNormal : lbHover);
		}
		if (lable != null)
		{
			lable.color = new Color(lable.color.r, lable.color.g, lable.color.b, (!isOver) ? lbNormal : lbHover);
		}
		if (texHandler != null)
		{
			texHandler.Intensity = ((!isOver) ? normalItensity : hoverItensity);
		}
		if (effect != null)
		{
			if (isOver)
			{
				effect.MouseEnter();
			}
			else
			{
				effect.MouseLeave();
			}
		}
	}

	private void OnPress(bool pressed)
	{
		if (!base.enabled || _disable)
		{
			return;
		}
		if (spr != null)
		{
			spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, (!pressed) ? lbNormal : lbPressed);
		}
		if (lable != null)
		{
			lable.color = new Color(lable.color.r, lable.color.g, lable.color.b, (!pressed) ? lbNormal : lbPressed);
		}
		if (texHandler != null)
		{
			texHandler.Intensity = ((!pressed) ? normalItensity : pressedItensity);
		}
		if (effect != null)
		{
			if (pressed)
			{
				effect.MouseDown();
			}
			else
			{
				effect.MouseUp();
			}
		}
	}

	private void OnClick()
	{
		if (eventReceiver != null)
		{
			e_OnClick.Send(eventReceiver, this);
		}
	}
}
