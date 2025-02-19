using Pathea;
using Pathea.Projectile;
using SkillSystem;
using UnityEngine;

namespace WhiteCat;

public class CreationSkEntity : PESkEntity
{
	private BehaviourController _controller;

	public PeEntity driver => (!(_controller is CarrierController)) ? null : (_controller as CarrierController).driver;

	public void Init(BehaviourController controller)
	{
		_controller = controller;
	}

	public override void ApplyEmission(int emitId, SkRuntimeInfo inst)
	{
		if (inst.Para is SkCarrierCanonPara)
		{
			_controller.GetWeapon(inst.Para).PlayEffects();
		}
		Transform transform = null;
		if (inst.Target != null)
		{
			PeTrans component = inst.Target.GetComponent<PeTrans>();
			if (component != null)
			{
				transform = component.trans;
			}
		}
		Singleton<ProjectileBuilder>.Instance.Register(emitId, base.transform, inst);
	}
}
