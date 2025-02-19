using System;
using System.IO;
using NAudio.Flac;
using NAudio.Wave;
using UnityEngine;

public class NAudioPlayer
{
	public enum SupportFormatType
	{
		NULL,
		mp3,
		flac
	}

	public static AudioClip GetClipByType(byte[] data, SupportFormatType type)
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream(data);
			WaveStream sourceStream = null;
			switch (type)
			{
			case SupportFormatType.mp3:
				sourceStream = new Mp3FileReader(memoryStream);
				break;
			case SupportFormatType.flac:
				sourceStream = new FlacReader(memoryStream);
				break;
			}
			WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(sourceStream);
			WAV wAV = new WAV(AudioMemStream(waveStream).ToArray());
			Debug.Log(wAV);
			AudioClip audioClip = AudioClip.Create($"{type.ToString()}Sound", wAV.SampleCount, 1, wAV.Frequency, stream: false);
			audioClip.SetData(wAV.LeftChannel, 0);
			return audioClip;
		}
		catch (Exception ex)
		{
			Debug.Log("NAudioPlayer.GetClipByType() Error:" + ex.Message);
			return null;
		}
	}

	private static MemoryStream AudioMemStream(WaveStream waveStream)
	{
		MemoryStream memoryStream = new MemoryStream();
		using WaveFileWriter waveFileWriter = new WaveFileWriter(memoryStream, waveStream.WaveFormat);
		byte[] array = new byte[waveStream.Length];
		waveStream.Position = 0L;
		waveStream.Read(array, 0, Convert.ToInt32(waveStream.Length));
		waveFileWriter.Write(array, 0, array.Length);
		waveFileWriter.Flush();
		return memoryStream;
	}
}
