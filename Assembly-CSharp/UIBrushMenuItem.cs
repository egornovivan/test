using UnityEngine;

public class UIBrushMenuItem : MonoBehaviour
{
	public enum BrushType
	{
		pointAdd,
		pointRemove,
		boxAdd,
		boxRemove,
		diagonalXPos,
		diagonalXNeg,
		diagonalZPos,
		diagonalZNeg,
		SelectAll,
		SelectDetail
	}

	public delegate void ClickEvent(BrushType type);

	public BrushType m_Type;

	public GameObject Target;

	public string ClickFunctionName;

	private BoxCollider mBoxClollider;

	private bool mRaycastGUI;

	private static int s_Counter;

	private bool rayCast;

	public static bool MouseOnHover => s_Counter != 0;

	public event ClickEvent onBtnClick;

	private void Start()
	{
		mBoxClollider = base.gameObject.GetComponent<BoxCollider>();
	}

	private void Update()
	{
		if (!(mBoxClollider == null) && !(UICamera.currentCamera == null))
		{
			if (!rayCast)
			{
				s_Counter++;
			}
			Ray ray = UICamera.mainCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
			rayCast = mBoxClollider.Raycast(ray, out var _, 1000f);
			if (!rayCast)
			{
				s_Counter--;
			}
		}
	}

	private void OnDisable()
	{
		if (rayCast)
		{
			s_Counter--;
			rayCast = false;
		}
	}

	private void OnClick()
	{
		if (Target != null && ClickFunctionName != string.Empty)
		{
			Target.SendMessage(ClickFunctionName, m_Type);
		}
	}
}
