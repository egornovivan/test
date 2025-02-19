using System;

namespace FMOD.Studio;

internal struct COMMAND_INFO_INTERNAL
{
	public IntPtr commandName;

	public int parentCommandIndex;

	public int frameNumber;

	public float frameTime;

	public INSTANCETYPE instanceType;

	public INSTANCETYPE outputType;

	public uint instanceHandle;

	public uint outputHandle;

	public COMMAND_INFO createPublic()
	{
		COMMAND_INFO result = default(COMMAND_INFO);
		result.commandName = MarshallingHelper.stringFromNativeUtf8(commandName);
		result.parentCommandIndex = parentCommandIndex;
		result.frameNumber = frameNumber;
		result.frameTime = frameTime;
		result.instanceType = instanceType;
		result.outputType = outputType;
		result.instanceHandle = instanceHandle;
		result.outputHandle = outputHandle;
		return result;
	}
}
