namespace MASA.Contrib.DDD.Domain;

public class DomainEventBus : IDomainEventBus
{
    protected readonly IEventBus _eventBus;
    protected readonly IIntegrationEventBus _integrationEventBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DispatcherOptions _options;

    private readonly ConcurrentQueue<IDomainEvent> _eventQueue = new ConcurrentQueue<IDomainEvent>();

    public DomainEventBus(IEventBus eventBus, IIntegrationEventBus integrationEventBus, IUnitOfWork unitOfWork, IOptions<DispatcherOptions> options)
    {
        _eventBus = eventBus;
        _integrationEventBus = integrationEventBus;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
    {
        if (@event is IDomainEvent @domainEvent && !IsAssignableFromDomainQuery(@event.GetType()))
        {
            @domainEvent.UnitOfWork = _unitOfWork;
        }
        if (@event is IIntegrationEvent integrationEvent)
        {
            await _integrationEventBus.PublishAsync((TEvent)integrationEvent);
        }
        else
        {
            await _eventBus.PublishAsync(@event);
        }

        bool IsAssignableFromDomainQuery(Type? type)
        {
            if (type == null)
                return false;

            if (!type.IsGenericType)
            {
                return IsAssignableFromDomainQuery(type.BaseType);
            }
            return type.GetInterfaces().Any(type => type.GetGenericTypeDefinition() == typeof(IDomainQuery<>));
        }
    }

    public Task Enqueue<TDomentEvent>(TDomentEvent @event) where TDomentEvent : IDomainEvent
    {
        _eventQueue.Enqueue(@event);
        return Task.CompletedTask;
    }

    public async Task PublishQueueAsync()
    {
        while (_eventQueue.TryDequeue(out IDomainEvent? @event))
        {
            await PublishAsync(@event);
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
        => await _unitOfWork.CommitAsync(cancellationToken);

    public IEnumerable<Type> GetAllEventTypes() => _options.AllEventTypes.Concat(_eventBus.GetAllEventTypes()).Distinct();
}