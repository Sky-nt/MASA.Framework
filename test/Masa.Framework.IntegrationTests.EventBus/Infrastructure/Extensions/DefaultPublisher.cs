// Copyright (c) MASA Stack All rights reserved.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.


namespace Masa.Framework.IntegrationTests.EventBus.Infrastructure.Extensions;

public class DefaultPublisher : IPublisher
{
    public Task BulkPublishAsync<T>(string topicName, List<(T @event, IntegrationEventExpand? eventMessageExpand)> events, CancellationToken stoppingToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync<T>(string topicName, T @event, IntegrationEventExpand? eventMessageExpand, CancellationToken stoppingToken = default)
    {
        return Task.CompletedTask;
    }
}
