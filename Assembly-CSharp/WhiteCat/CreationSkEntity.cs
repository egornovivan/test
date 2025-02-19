using Pathea;
using Pathea.Projectile;
using SkillSystem;

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
		if (inst.Target != null)
		{
		}
		Singleton<ProjectileBuilder>.Instance.Register(emitId, base.transform, inst);
	}
}
