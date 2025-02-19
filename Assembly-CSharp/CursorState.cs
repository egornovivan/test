using System;
using UnityEngine;

public class CursorState : MonoBehaviour
{
	public enum EType
	{
		None,
		Normal,
		Hand
	}

	[Serializable]
	public class CursorIcon
	{
		public Texture2D none;

		public Texture2D nml;

		public Texture2D hand;
	}

	private static CursorState _self;

	private CursorHandler m_Handler;

	public CursorIcon m_Icon;

	private bool onece;

	public static CursorState self
	{
		get
		{
			if (_self == null && Application.isPlaying)
			{
				GameObject gameObject = Resources.Load<GameObject>("Prefabs/CursorState");
				if (gameObject != null)
				{
					UnityEngine.Object.Instantiate(gameObject);
				}
			}
			return _self;
		}
	}

	public CursorHandler Handler => m_Handler;

	public void SetHandler(CursorHandler handler)
	{
		if (handler != m_Handler)
		{
			m_Handler = handler;
		}
	}

	public void ClearHandler(CursorHandler handler)
	{
		if (m_Handler == handler)
		{
			m_Handler = null;
		}
	}

	private void Awake()
	{
		_self = this;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (m_Handler != null)
		{
			switch (m_Handler.Type)
			{
			case EType.Normal:
				Cursor.SetCursor(m_Icon.nml, new Vector2(m_Icon.nml.width / 2, m_Icon.nml.height / 2), CursorMode.Auto);
				break;
			case EType.Hand:
				Cursor.SetCursor(m_Icon.hand, new Vector2(m_Icon.hand.width / 2, m_Icon.hand.height / 2), CursorMode.Auto);
				break;
			case EType.None:
				Cursor.SetCursor(m_Icon.none, new Vector2(m_Icon.none.width / 2, m_Icon.none.height / 2), CursorMode.Auto);
				break;
			default:
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				break;
			}
			onece = false;
		}
		else if (!onece)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			onece = true;
		}
	}
}
