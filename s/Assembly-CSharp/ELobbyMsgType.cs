public enum ELobbyMsgType
{
	AccountLogin = 100,
	AccountLogout,
	RepeatLogin,
	TestLogin,
	EnterLobby,
	EnterLobbySuccess,
	EnterLobbyFailed,
	RoleLogin,
	RoleInfoAllGot,
	RoleInfoNone,
	DeleteRole,
	DeleteRoleSuccess,
	DeleteRoleFailed,
	RoleCreate,
	RoleCreateSuccess,
	RoleCreateFailed,
	RolesInLobby,
	SteamLogin,
	SteamInvite,
	SteamInviteData,
	ShopData,
	ShopDataAll,
	BuyItems,
	UseItems,
	CreateItem,
	QueryLobbyExp,
	AddLobbyExp,
	UploadISO,
	UploadISOSuccess,
	SendMsg,
	CloseServer,
	ServerRegisterDebug,
	ServerRegister,
	MasterRegister,
	MasterUpdate,
	CheatingCheck,
	Max
}
