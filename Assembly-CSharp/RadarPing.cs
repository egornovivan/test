using C5;
using UnityEngine;

public class RadarPing : Radar
{
	[SerializeField]
	private float _detectionRadius = 10f;

	[SerializeField]
	private bool _drawGizmos;

	public float DetectionRadius
	{
		get
		{
			return _detectionRadius;
		}
		set
		{
			_detectionRadius = value;
		}
	}

	private void OnDrawGizmos()
	{
		if (_drawGizmos)
		{
			Vector3 center = ((!(base.Vehicle == null)) ? base.Vehicle.Position : base.transform.position);
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(center, _detectionRadius);
		}
	}

	protected override IList<Collider> Detect()
	{
		Collider[] items = Physics.OverlapSphere(base.Vehicle.Position, _detectionRadius, base.LayersChecked);
		ArrayList<Collider> arrayList = new ArrayList<Collider>();
		arrayList.AddAll(items);
		return arrayList;
	}
}
