using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace InControl;

public struct VersionInfo : IComparable<VersionInfo>
{
	public int Major;

	public int Minor;

	public int Patch;

	public int Build;

	public VersionInfo(int major, int minor = 0, int patch = 0, int build = 0)
	{
		Major = major;
		Minor = minor;
		Patch = patch;
		Build = build;
	}

	public static VersionInfo InControlVersion()
	{
		VersionInfo result = default(VersionInfo);
		result.Major = 1;
		result.Minor = 4;
		result.Patch = 4;
		result.Build = 3839;
		return result;
	}

	public static VersionInfo UnityVersion()
	{
		Match match = Regex.Match(Application.unityVersion, "^(\\d+)\\.(\\d+)\\.(\\d+)");
		int build = 0;
		VersionInfo result = default(VersionInfo);
		result.Major = Convert.ToInt32(match.Groups[1].Value);
		result.Minor = Convert.ToInt32(match.Groups[2].Value);
		result.Patch = Convert.ToInt32(match.Groups[3].Value);
		result.Build = build;
		return result;
	}

	public int CompareTo(VersionInfo other)
	{
		if (Major < other.Major)
		{
			return -1;
		}
		if (Major > other.Major)
		{
			return 1;
		}
		if (Minor < other.Minor)
		{
			return -1;
		}
		if (Minor > other.Minor)
		{
			return 1;
		}
		if (Patch < other.Patch)
		{
			return -1;
		}
		if (Patch > other.Patch)
		{
			return 1;
		}
		if (Build < other.Build)
		{
			return -1;
		}
		if (Build > other.Build)
		{
			return 1;
		}
		return 0;
	}

	public override string ToString()
	{
		if (Build == 0)
		{
			return $"{Major}.{Minor}.{Patch}";
		}
		return $"{Major}.{Minor}.{Patch} build {Build}";
	}

	public string ToShortString()
	{
		if (Build == 0)
		{
			return $"{Major}.{Minor}.{Patch}";
		}
		return $"{Major}.{Minor}.{Patch}b{Build}";
	}

	public override bool Equals(object other)
	{
		if (other is VersionInfo)
		{
			return this == (VersionInfo)other;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Major.GetHashCode() ^ Minor.GetHashCode() ^ Patch.GetHashCode() ^ Build.GetHashCode();
	}

	public static bool operator ==(VersionInfo a, VersionInfo b)
	{
		return a.CompareTo(b) == 0;
	}

	public static bool operator !=(VersionInfo a, VersionInfo b)
	{
		return a.CompareTo(b) != 0;
	}

	public static bool operator <=(VersionInfo a, VersionInfo b)
	{
		return a.CompareTo(b) <= 0;
	}

	public static bool operator >=(VersionInfo a, VersionInfo b)
	{
		return a.CompareTo(b) >= 0;
	}

	public static bool operator <(VersionInfo a, VersionInfo b)
	{
		return a.CompareTo(b) < 0;
	}

	public static bool operator >(VersionInfo a, VersionInfo b)
	{
		return a.CompareTo(b) > 0;
	}
}
