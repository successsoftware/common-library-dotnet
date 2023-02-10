using DotNetCore.CAP;

namespace SSS.AspNetCore.CAP.Event
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler, ICapSubscribe
        where TIntegrationEvent : IntegrationEvent
    {
        Task HandleAsync(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler
    {
    }
}
