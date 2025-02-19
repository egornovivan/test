using System;

namespace FMOD.Studio;

internal struct USER_PROPERTY_INTERNAL
{
	private IntPtr name;

	private USER_PROPERTY_TYPE type;

	private Union_IntBoolFloatString value;

	public USER_PROPERTY createPublic()
	{
		USER_PROPERTY result = default(USER_PROPERTY);
		result.name = MarshallingHelper.stringFromNativeUtf8(name);
		result.type = type;
		switch (type)
		{
		case USER_PROPERTY_TYPE.INTEGER:
			result.intValue = value.intValue;
			break;
		case USER_PROPERTY_TYPE.BOOLEAN:
			result.boolValue = value.boolValue;
			break;
		case USER_PROPERTY_TYPE.FLOAT:
			result.floatValue = value.floatValue;
			break;
		case USER_PROPERTY_TYPE.STRING:
			result.stringValue = MarshallingHelper.stringFromNativeUtf8(value.stringValue);
			break;
		}
		return result;
	}
}
