using System.Runtime.InteropServices;

namespace Pathea;

public struct Layer
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Mask
	{
		public const int Default = 1;

		public const int TransparentFX = 2;

		public const int IgnoreRaycast = 4;

		public const int Layer3 = 8;

		public const int Water = 16;

		public const int UI = 32;

		public const int Layer6 = 64;

		public const int Layer7 = 128;

		public const int AIPlayer = 256;

		public const int ShowModel = 512;

		public const int Player = 1024;

		public const int SceneStatic = 2048;

		public const int VFVoxelTerrain = 4096;

		public const int Building = 8192;

		public const int TreeStatic = 16384;

		public const int Ragdoll = 32768;

		public const int Unwalkable = 65536;

		public const int Wheel = 131072;

		public const int GIELayer = 262144;

		public const int GIEProductLayer = 524288;

		public const int ShowModelCreation = 1048576;

		public const int NearTreePhysics = 2097152;

		public const int EnergyShield = 4194304;

		public const int ProxyPlayer = 8388608;

		public const int MetalScan = 16777216;

		public const int Damage = 33554432;

		public const int OutlineHighlight = 67108864;

		public const int VCEMatGen = 134217728;

		public const int VCEGUI = 268435456;

		public const int VCEScene = 536870912;

		public const int GUI = 1073741824;

		public const int PEEnvironment = int.MinValue;
	}

	public const int Default = 0;

	public const int TransparentFX = 1;

	public const int IgnoreRaycast = 2;

	public const int Layer3 = 3;

	public const int Water = 4;

	public const int UI = 5;

	public const int Layer6 = 6;

	public const int Layer7 = 7;

	public const int AIPlayer = 8;

	public const int ShowModel = 9;

	public const int Player = 10;

	public const int SceneStatic = 11;

	public const int VFVoxelTerrain = 12;

	public const int Building = 13;

	public const int TreeStatic = 14;

	public const int Ragdoll = 15;

	public const int Unwalkable = 16;

	public const int Wheel = 17;

	public const int GIELayer = 18;

	public const int GIEProductLayer = 19;

	public const int ShowModelCreation = 20;

	public const int NearTreePhysics = 21;

	public const int EnergyShield = 22;

	public const int ProxyPlayer = 23;

	public const int MetalScan = 24;

	public const int Damage = 25;

	public const int OutlineHighlight = 26;

	public const int VCEMatGen = 27;

	public const int VCEGUI = 28;

	public const int VCEScene = 29;

	public const int GUI = 30;

	public const int PEEnvironment = 31;
}
