using System;

namespace FMOD.Studio;

internal struct PARAMETER_DESCRIPTION_INTERNAL
{
	public IntPtr name;

	public float minimum;

	public float maximum;

	public PARAMETER_TYPE type;

	public void assign(out PARAMETER_DESCRIPTION publicDesc)
	{
		publicDesc.name = MarshallingHelper.stringFromNativeUtf8(name);
		publicDesc.minimum = minimum;
		publicDesc.maximum = maximum;
		publicDesc.type = type;
	}
}
