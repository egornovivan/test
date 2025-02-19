public enum EEntityState
{
	None = 0,
	EEntityState_Death = 1,
	EEntityState_EquipmentHold = 2,
	EEntityState_HoldShield = 4,
	EEntityState_GunHold = 8,
	EEntityState_BowHold = 0x10,
	EEntityState_AimEquipHold = 0x20,
	EEntityState_HoldFlashLight = 0x40,
	EEntityState_TwoHandSwordHold = 0x80,
	EEntityState_Sit = 0x100,
	EEntityState_SLeep = 0x200,
	EEntityState_Cure = 0x400,
	EEntityState_Operation = 0x800
}
