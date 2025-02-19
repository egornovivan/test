namespace Pathea;

public enum EMsg
{
	Null = 0,
	Lod_Collider_Created = 1,
	Lod_Collider_Destroying = 2,
	View_FirstPerson = 3,
	View_Prefab_Build = 4,
	View_Prefab_Destroy = 5,
	View_Injured_Build = 6,
	View_Injured_Destroy = 7,
	View_Model_Build = 8,
	View_Model_Destroy = 9,
	View_Model_AttachJoint = 10,
	View_Model_ReattachJoint = 11,
	View_Model_DeatchJoint = 12,
	View_Ragdoll_Build = 13,
	View_Ragdoll_Destroy = 14,
	View_Ragdoll_AttachJoint = 15,
	View_Ragdoll_ReattachJoint = 16,
	View_Ragdoll_DeatchJoint = 17,
	View_Ragdoll_Fall_Begin = 18,
	View_Ragdoll_Fall_Finished = 19,
	View_Ragdoll_Getup_Begin = 20,
	View_Ragdoll_Getup_Finished = 21,
	View_Trigger_Add = 22,
	View_Trigger_Remove = 23,
	Trans_Real = 24,
	Trans_Simulator = 25,
	Trans_Pos_set = 26,
	Action_Whacked = 27,
	Action_Repulsed = 28,
	Action_Wentfly = 29,
	Action_Knocked = 30,
	Action_GetOnVehicle = 31,
	Action_DurabilityDeficiency = 32,
	Battle_EnterShootMode = 33,
	Battle_ExitShootMode = 34,
	Battle_PauseShootMode = 35,
	Battle_ContinueShootMode = 36,
	Battle_OnShoot = 37,
	Battle_HPChange = 38,
	Battle_Attack = 39,
	Battle_OnAttack = 40,
	Battle_BeAttacked = 41,
	Battle_EquipAttack = 42,
	Battle_AttackHit = 43,
	Battle_TargetSkill = 44,
	Camera_ChangeMode = 45,
	State_Die = 46,
	State_Revive = 47,
	state_Water = 48,
	Build_BuildMode = 49,
	UI_ShowChange = 50,
	Skill_CheckLoop = 51,
	Skill_Event = 52,
	Skill_Interrupt = 53,
	Net_Begin = 54,
	Net_Hit = 54,
	Net_End = 55,
	Net_Instantiate = 56,
	Net_Controller = 57,
	Net_Proxy = 58,
	Net_Destroy = 59,
	Max = 60
}
