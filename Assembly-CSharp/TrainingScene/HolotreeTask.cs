using UnityEngine;

namespace TrainingScene;

public class HolotreeTask : MonoBehaviour
{
	private static HolotreeTask s_instance;

	[SerializeField]
	private HolotreeCutter cutter;

	[SerializeField]
	private HolotreeAppearance appearance;

	[SerializeField]
	private GameObject treeBase;

	[SerializeField]
	private int MaxHP;

	private int HP;

	public static HolotreeTask Instance => s_instance;

	private void Awake()
	{
		s_instance = this;
	}

	public void InitScene()
	{
		appearance.gameObject.SetActive(value: true);
		base.transform.GetComponent<Collider>().enabled = true;
		HP = MaxHP;
		HoloCameraControl.Instance.renderObjs1.Add(treeBase);
		appearance.produce = true;
	}

	public void DestroyScene()
	{
		appearance.destroy = true;
		Invoke("CloseMission", appearance.fadeTime + 0.8f);
	}

	private void CloseMission()
	{
		HoloCameraControl.Instance.renderObjs1.Clear();
		appearance.gameObject.SetActive(value: false);
		base.transform.GetComponent<Collider>().enabled = false;
	}

	public bool SubHP()
	{
		if (--HP == 0)
		{
			appearance.destroy = true;
			DestroyScene();
			return true;
		}
		return false;
	}
}
