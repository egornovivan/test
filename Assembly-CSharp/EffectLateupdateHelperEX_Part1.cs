using PETools;
using UnityEngine;

[RequireComponent(typeof(EffectLateupdateHelperEX_Part2))]
public class EffectLateupdateHelperEX_Part1 : EffectLateupdateHelper
{
	[SerializeField]
	private string m_CenterBoneName = "Bip01 Spine3";

	public ParticleSystem[] particleSystems;

	[HideInInspector]
	public Transform centerBone;

	[HideInInspector]
	public Vector3 centerToParentLocal;

	[HideInInspector]
	public Vector3 parentForwardLocal;

	public Transform parentTrans => m_ParentTrans;

	private void Reset()
	{
		particleSystems = GetComponentsInChildren<ParticleSystem>();
	}

	public override void Init(Transform parentTrans)
	{
		base.Init(parentTrans);
		centerBone = PEUtil.GetChild(parentTrans, m_CenterBoneName);
	}

	protected override void Update()
	{
	}

	protected override void LateUpdate()
	{
		if (null == m_ParentTrans || null == centerBone)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		base.transform.position = centerBone.position + centerBone.TransformDirection(centerToParentLocal);
		parentForwardLocal = centerBone.TransformDirection(parentForwardLocal);
		base.transform.rotation = Quaternion.LookRotation(parentForwardLocal, Vector3.up);
	}
}
