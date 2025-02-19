using UnityEngine;

namespace WhiteCat;

public abstract class VCPart : MonoBehaviour
{
	private string _description;

	[SerializeField]
	private int _descriptionID;

	[HideInInspector]
	public bool hiddenModel;

	public string description
	{
		get
		{
			if (_description == null)
			{
				_description = BuildDescription();
			}
			return _description;
		}
	}

	public static Direction GetDirection(Vector3 vector)
	{
		int num = 0;
		float num2 = Mathf.Abs(vector.x);
		if (Mathf.Abs(vector.y) > num2)
		{
			num = 1;
			num2 = Mathf.Abs(vector.y);
		}
		if (Mathf.Abs(vector.z) > num2)
		{
			num = 2;
		}
		return num switch
		{
			0 => (!(vector.x < 0f)) ? Direction.Right : Direction.Left, 
			1 => (!(vector.y < 0f)) ? Direction.Up : Direction.Down, 
			_ => (vector.z < 0f) ? Direction.Back : Direction.Forward, 
		};
	}

	public void InvalidDescription()
	{
		_description = null;
	}

	protected virtual string BuildDescription()
	{
		return PELocalization.GetString(_descriptionID);
	}

	protected virtual void Awake()
	{
		base.enabled = false;
	}
}
