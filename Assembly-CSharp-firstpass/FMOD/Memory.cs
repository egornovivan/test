using System;
using System.Runtime.InteropServices;

namespace FMOD;

public class Memory
{
	public static RESULT Initialize(IntPtr poolmem, int poollen, MEMORY_ALLOC_CALLBACK useralloc, MEMORY_REALLOC_CALLBACK userrealloc, MEMORY_FREE_CALLBACK userfree, MEMORY_TYPE memtypeflags)
	{
		return FMOD5_Memory_Initialize(poolmem, poollen, useralloc, userrealloc, userfree, memtypeflags);
	}

	public static RESULT GetStats(out int currentalloced, out int maxalloced)
	{
		return GetStats(out currentalloced, out maxalloced, blocking: false);
	}

	public static RESULT GetStats(out int currentalloced, out int maxalloced, bool blocking)
	{
		return FMOD5_Memory_GetStats(out currentalloced, out maxalloced, blocking);
	}

	[DllImport("fmod")]
	private static extern RESULT FMOD5_Memory_Initialize(IntPtr poolmem, int poollen, MEMORY_ALLOC_CALLBACK useralloc, MEMORY_REALLOC_CALLBACK userrealloc, MEMORY_FREE_CALLBACK userfree, MEMORY_TYPE memtypeflags);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_Memory_GetStats(out int currentalloced, out int maxalloced, bool blocking);
}
