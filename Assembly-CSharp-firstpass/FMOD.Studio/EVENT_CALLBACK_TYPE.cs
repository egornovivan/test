using System;

namespace FMOD.Studio;

[Flags]
public enum EVENT_CALLBACK_TYPE : uint
{
	STARTED = 1u,
	RESTARTED = 2u,
	STOPPED = 4u,
	CREATE_PROGRAMMER_SOUND = 8u,
	DESTROY_PROGRAMMER_SOUND = 0x10u,
	PLUGIN_CREATED = 0x20u,
	PLUGIN_DESTROYED = 0x40u,
	ALL = uint.MaxValue
}
