using UnityEngine;

public class VCParticlePlayer : MonoBehaviour
{
	public const int ftDamaged = 1;

	public const int ftExplode = 2;

	public int FunctionTag;

	public float ReferenceValue;

	public Vector3 LocalPosition = Vector3.zero;

	[SerializeField]
	private GameObject PlayingObject;

	private GameObject ResourceObject;

	public GameObject Effect
	{
		get
		{
			return PlayingObject;
		}
		set
		{
			if (value == ResourceObject)
			{
				return;
			}
			ResourceObject = value;
			if (PlayingObject != null)
			{
				Object.Destroy(PlayingObject);
			}
			if (value == null)
			{
				PlayingObject = null;
				return;
			}
			PlayingObject = Object.Instantiate(value);
			if ((bool)PlayingObject)
			{
				PlayingObject.transform.parent = base.transform;
				PlayingObject.transform.localPosition = LocalPosition;
				PlayingObject.transform.localRotation = Quaternion.identity;
				PlayingObject.transform.localScale = Vector3.one;
				ParticleSystem[] componentsInChildren = PlayingObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
				if (PlayingObject.GetComponent<ParticleSystem>() != null)
				{
					PlayingObject.GetComponent<ParticleSystem>().Play();
				}
				ParticleSystem[] array = componentsInChildren;
				foreach (ParticleSystem particleSystem in array)
				{
					particleSystem.Play();
				}
			}
		}
	}

	private void Start()
	{
		Effect = null;
		ReferenceValue = Random.value;
	}
}
