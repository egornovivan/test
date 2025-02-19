public class TerrainUtil
{
	public static float VolumeToHeight(byte volumeUp, byte volumeDown)
	{
		if ((float)(int)volumeUp > 127.5f && (float)(int)volumeDown > 127.5f)
		{
			return -1f;
		}
		if ((float)(int)volumeUp < 127.5f && (float)(int)volumeDown < 127.5f)
		{
			return -1f;
		}
		if (volumeUp == 128)
		{
			return 1f;
		}
		if (volumeDown == 128)
		{
			return 0f;
		}
		return ((float)(int)volumeDown * 1f - 128f) / (float)(128 - volumeUp) / (1f + ((float)(int)volumeDown * 1f - 128f) / (float)(128 - volumeUp));
	}

	public static byte HeightToVolume(float height)
	{
		byte b = byte.MaxValue;
		if (height == 1f)
		{
			return 128;
		}
		return (byte)(128f - (float)(b - 128) * (1f - height) / height);
	}
}
