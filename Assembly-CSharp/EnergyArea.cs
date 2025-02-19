using UnityEngine;

public class EnergyArea : MonoBehaviour
{
	[SerializeField]
	private EnergyAreaHandler _handler;

	[SerializeField]
	private Projector _projector;

	public float radius
	{
		get
		{
			return _projector.orthographicSize;
		}
		set
		{
			_projector.orthographicSize = value;
		}
	}

	public float energyScale
	{
		get
		{
			return _handler.m_EnergyScale;
		}
		set
		{
			_handler.m_EnergyScale = value;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
