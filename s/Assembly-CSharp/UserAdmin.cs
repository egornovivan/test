public class UserAdmin
{
	private int _id;

	private ulong _privileges;

	private string _roleName;

	public int Id => _id;

	public ulong Privileges => _privileges;

	public string RoleName => _roleName;

	public UserAdmin(int id, string roleName, ulong privileges)
	{
		_id = id;
		_roleName = roleName;
		_privileges = privileges;
	}

	public void AddPrivileges(AdminMask mask)
	{
		_privileges |= (ulong)mask;
	}

	public void RemovePrivileges(AdminMask mask)
	{
		if (HasPrivileges(mask))
		{
			_privileges ^= (ulong)mask;
		}
	}

	public void Reset()
	{
		_privileges = 0uL;
	}

	public bool HasPrivileges(AdminMask mask)
	{
		return (_privileges & (ulong)mask) != 0;
	}
}
