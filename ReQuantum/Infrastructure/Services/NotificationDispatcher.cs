using Microsoft.Extensions.DependencyInjection;
using ReQuantum.Attributes;
using System;

namespace ReQuantum.Infrastructure.Services;

public interface INotificationDispatcher
{
    void Publish<TNotification>(TNotification args) where TNotification : INotification;
}

[AutoInject(Lifetime.Singleton)]
public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Publish<TNotification>(TNotification args) where TNotification : INotification
    {
        var services = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
        foreach (var service in services)
        {
            service.Handle(args);
        }
    }
}

// ReSharper disable once TypeParameterCanBeVariant
public interface INotificationHandler<TNotification> where TNotification : INotification
{
    void Handle(TNotification notification);
}

public interface INotification;