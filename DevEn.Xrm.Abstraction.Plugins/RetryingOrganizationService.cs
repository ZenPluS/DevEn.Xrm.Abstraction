using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;
using System.Threading;

namespace DevEn.Xrm.Abstraction.Plugins
{
    /// <summary>
    /// Simple retry decorator for <see cref="IOrganizationService"/> targeting common transient Dataverse faults
    /// (throttling / timeouts). Implements linear backoff: delay = attempt * baseDelay.
    /// Transient codes handled: -2147220970 (Throttle) and -2147012744 (Timeout).
    /// </summary>
    public class RetryingOrganizationService : IOrganizationService
    {
        private readonly IOrganizationService _inner;
        private readonly int _maxRetries;
        private readonly TimeSpan _baseDelay;

        public RetryingOrganizationService(IOrganizationService inner, int maxRetries = 3, int baseDelayMs = 200)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _maxRetries = maxRetries;
            _baseDelay = TimeSpan.FromMilliseconds(baseDelayMs);
        }

        private T Exec<T>(Func<T> op)
        {
            for (var attempt = 1; ; attempt++)
            {
                try { return op(); }
                catch (FaultException<OrganizationServiceFault> f) when (IsTransient(f) && attempt <= _maxRetries)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * attempt));
                }
            }
        }

        private void Exec(Action op) => Exec<object>(() => { op(); return null; });

        private static bool IsTransient(FaultException<OrganizationServiceFault> fault)
        {
            var code = fault.Detail?.ErrorCode;
            return code == -2147220970 || code == -2147012744;
        }

        public Guid Create(Entity entity)
            => Exec(() => _inner.Create(entity));

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
            => Exec(() => _inner.Retrieve(entityName, id, columnSet));

        public void Update(Entity entity)
            => Exec(() => _inner.Update(entity));

        public void Delete(string entityName, Guid id)
            => Exec(() => _inner.Delete(entityName, id));

        public OrganizationResponse Execute(OrganizationRequest request)
            => Exec(() => _inner.Execute(request));

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
            => Exec(() => _inner.Associate(entityName, entityId, relationship, relatedEntities));

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
            => Exec(() => _inner.Disassociate(entityName, entityId, relationship, relatedEntities));

        public EntityCollection RetrieveMultiple(QueryBase query)
            => Exec(() => _inner.RetrieveMultiple(query));
    }
}