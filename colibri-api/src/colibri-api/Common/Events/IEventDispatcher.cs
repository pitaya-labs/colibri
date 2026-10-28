namespace colibri_api.Common.Events;

public interface IEventDispatcher
{
    Task Publish<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class;
}

public interface IEventHandler<in TEvent> where TEvent : class
{
    Task Handle(TEvent @event, CancellationToken ct);
}
