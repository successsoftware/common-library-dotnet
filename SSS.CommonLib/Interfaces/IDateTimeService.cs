using System;

namespace SSS.CommonLib.Interfaces;

public interface IDateTimeService
{
    DateTime UtcNow { get; }
}