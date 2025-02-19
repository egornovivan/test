using System;

public class UTimer
{
	public struct TimeStruct
	{
		public int Year;

		public float Season;

		public int Month;

		public int Date;

		public int Hour;

		public int Minute;

		public int Second;

		public int Millisecond;
	}

	protected const long c_Sec2Tick = 100000L;

	protected const long c_Millisec2Tick = 100L;

	protected const double c_Tick2Sec = 1E-05;

	protected long m_Tick;

	protected float m_ElapseSpeed;

	public float ElapseSpeedBak = -1f;

	protected long c_Min2Sec = 60L;

	protected long c_Hour2Min = 60L;

	protected long c_Day2Hour = 24L;

	public long Tick
	{
		get
		{
			return m_Tick;
		}
		set
		{
			m_Tick = value;
		}
	}

	public double Day
	{
		get
		{
			return (double)m_Tick * 1E-05 / (double)Day2Sec;
		}
		set
		{
			m_Tick = (long)(value * (double)Day2Sec * 100000.0);
		}
	}

	public double Hour
	{
		get
		{
			return (double)m_Tick * 1E-05 / (double)Hour2Sec;
		}
		set
		{
			m_Tick = (long)(value * (double)Hour2Sec * 100000.0);
		}
	}

	public double Minute
	{
		get
		{
			return (double)m_Tick * 1E-05 / (double)c_Min2Sec;
		}
		set
		{
			m_Tick = (long)(value * (double)c_Min2Sec * 100000.0);
		}
	}

	public double Second
	{
		get
		{
			return (double)m_Tick * 1E-05;
		}
		set
		{
			m_Tick = (long)(value * 100000.0);
		}
	}

	public double TimeInDay
	{
		get
		{
			double day = Day;
			return day - Math.Floor(day);
		}
	}

	public double HourInDay => TimeInDay * (double)c_Day2Hour;

	public double MinuteInDay => TimeInDay * (double)Day2Min;

	public double SecondInDay => TimeInDay * (double)Day2Sec;

	public double CycleInDay => 0.0 - Math.Cos(TimeInDay * Math.PI * 2.0);

	public float ElapseSpeed
	{
		get
		{
			return m_ElapseSpeed;
		}
		set
		{
			m_ElapseSpeed = value;
		}
	}

	public long Min2Sec => c_Min2Sec;

	public long Hour2Min => c_Hour2Min;

	public long Day2Hour => c_Day2Hour;

	public long Hour2Sec => c_Hour2Min * c_Min2Sec;

	public long Day2Min => c_Day2Hour * c_Hour2Min;

	public long Day2Sec => c_Day2Hour * Hour2Sec;

	public UTimer(long day2Hour = 24, long hour2Min = 60, long min2Sec = 60)
	{
		c_Day2Hour = day2Hour;
		c_Hour2Min = hour2Min;
		c_Min2Sec = min2Sec;
	}

	public void Reset()
	{
		m_Tick = 0L;
		m_ElapseSpeed = 0f;
	}

	public void Update(float dt)
	{
		m_Tick += (long)(m_ElapseSpeed * dt * 100000f);
	}

	public void Import(byte[] buffer)
	{
		if (buffer != null && buffer.Length == 8)
		{
			m_Tick = BitConverter.ToInt64(buffer, 0);
		}
		else
		{
			m_Tick = 0L;
		}
	}

	public byte[] Export()
	{
		return BitConverter.GetBytes(m_Tick);
	}

	public void SetTime(int year, int month, int date, int hour, int minute, int second, int millisecond)
	{
		TimeStruct ts = default(TimeStruct);
		ts.Year = year;
		ts.Month = month;
		ts.Date = date;
		ts.Hour = hour;
		ts.Minute = minute;
		ts.Second = second;
		ts.Millisecond = millisecond;
		m_Tick = TimeStructToTick(ts);
	}

	public void SetTime(int daycount, int second)
	{
		m_Tick = (daycount * Day2Sec + second) * 100000;
	}

	public void AddTime(UTimer timer)
	{
		m_Tick += timer.m_Tick;
	}

	public void MinusTime(UTimer timer)
	{
		m_Tick -= timer.m_Tick;
	}

	public virtual TimeStruct DayToCalendar(int day)
	{
		TimeStruct result = default(TimeStruct);
		result.Date = day;
		return result;
	}

	public virtual int CalendarToDay(TimeStruct ts)
	{
		return ts.Date;
	}

	public TimeStruct TickToTimeStruct(long tick)
	{
		PETimer tmpTimer = PETimerUtil.GetTmpTimer();
		tmpTimer.Tick = tick;
		double day = tmpTimer.Day;
		TimeStruct result = DayToCalendar((int)Math.Floor(day));
		double timeInDay = tmpTimer.TimeInDay;
		result.Millisecond = (int)(timeInDay * (double)Day2Sec * 1000.0);
		result.Second = result.Millisecond / 1000;
		result.Millisecond %= 1000;
		result.Minute = result.Second / (int)c_Min2Sec;
		result.Second %= (int)c_Min2Sec;
		result.Hour = result.Minute / (int)c_Hour2Min;
		result.Minute %= (int)c_Hour2Min;
		result.Hour %= (int)c_Day2Hour;
		return result;
	}

	public long TimeStructToTick(TimeStruct ts)
	{
		int num = CalendarToDay(ts);
		return (num * Day2Sec + ts.Hour * Hour2Sec + ts.Minute * c_Min2Sec + ts.Second) * 100000 + (long)ts.Millisecond * 100L;
	}

	public string FormatString()
	{
		return FormatString(string.Empty);
	}

	public string FormatString(string format)
	{
		if (format == null || format.Trim().Length == 0)
		{
			format = "YY-MM-DD hh:mm:ss AP";
		}
		TimeStruct timeStruct = TickToTimeStruct(m_Tick);
		format = format.Replace("YYYY", timeStruct.Year.ToString("0000"));
		format = format.Replace("YYY", timeStruct.Year.ToString("000"));
		format = format.Replace("YY", timeStruct.Year.ToString("00"));
		format = format.Replace("Y", timeStruct.Year.ToString("0"));
		format = format.Replace("MM", timeStruct.Month.ToString("00"));
		format = format.Replace("M", timeStruct.Month.ToString("0"));
		format = format.Replace("DD", timeStruct.Date.ToString("00"));
		format = format.Replace("D", timeStruct.Date.ToString("0"));
		format = format.Replace("hh", timeStruct.Hour.ToString("00"));
		format = format.Replace("h", timeStruct.Hour.ToString("0"));
		format = format.Replace("mm", timeStruct.Minute.ToString("00"));
		format = format.Replace("ss", timeStruct.Second.ToString("00"));
		format = format.Replace("AP", (timeStruct.Hour >= c_Day2Hour / 2) ? string.Empty : string.Empty);
		return format;
	}
}
