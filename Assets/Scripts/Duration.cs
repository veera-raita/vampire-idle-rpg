using System;

[Serializable]
public class Duration
{
    public int days;
    public int hours;
    public int minutes;

    public TimeSpan AsTimeSpan()
    {
        return new TimeSpan(days, hours, minutes, 0);
    }
}