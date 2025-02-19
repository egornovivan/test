using System;

namespace FMOD;

[Flags]
public enum MEMORY_TYPE : uint
{
	NORMAL = 0u,
	STREAM_FILE = 1u,
	STREAM_DECODE = 2u,
	SAMPLEDATA = 4u,
	DSP_BUFFER = 8u,
	PLUGIN = 0x10u,
	XBOX360_PHYSICAL = 0x100000u,
	PERSISTENT = 0x200000u,
	SECONDARY = 0x400000u,
	ALL = uint.MaxValue
}
