using SSS.CommonLib.Interfaces;

namespace WebApi.Sample
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
