using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests
{
    [TestClass]
    public class RetryingOrganizationServiceTests
    {
        private class TransientFailingService : IOrganizationService
        {
            private readonly int _failCount;
            private int _attempts;
            public int Attempts => _attempts;

            public TransientFailingService(int failCount) => _failCount = failCount;

            private void MaybeThrow()
            {
                _attempts++;
                if (_attempts <= _failCount)
                {
                    var fault = new OrganizationServiceFault { ErrorCode = -2147220970, Message = "SQL timeout simulated" };
                    throw new FaultException<OrganizationServiceFault>(fault, "Transient");
                }
            }

            public Guid Create(Entity entity)
            {
                MaybeThrow();
                return entity.Id != Guid.Empty ? entity.Id : Guid.NewGuid();
            }

            public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
            {
                MaybeThrow();
                return new Entity(entityName) { Id = id };
            }

            public void Update(Entity entity) => MaybeThrow();
            public void Delete(string entityName, Guid id) => MaybeThrow();
            public OrganizationResponse Execute(OrganizationRequest request)
            {
                MaybeThrow();
                return new OrganizationResponse();
            }
            public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) => MaybeThrow();
            public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) => MaybeThrow();
            public EntityCollection RetrieveMultiple(QueryBase query)
            {
                MaybeThrow();
                return new EntityCollection();
            }
        }

        private class NonTransientFailingService : IOrganizationService
        {
            public Guid Create(Entity entity) => throw BuildFault(-12345);
            public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) => throw BuildFault(-12345);
            public void Update(Entity entity) => throw BuildFault(-12345);
            public void Delete(string entityName, Guid id) => throw BuildFault(-12345);
            public OrganizationResponse Execute(OrganizationRequest request) => throw BuildFault(-12345);
            public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) => throw BuildFault(-12345);
            public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) => throw BuildFault(-12345);
            public EntityCollection RetrieveMultiple(QueryBase query) => throw BuildFault(-12345);

            private FaultException<OrganizationServiceFault> BuildFault(int code)
            {
                var fault = new OrganizationServiceFault { ErrorCode = code, Message = "Non transient" };
                return new FaultException<OrganizationServiceFault>(fault, "Permanent");
            }
        }

        [TestMethod]
        public void RetryingService_Should_Retry_And_Succeed()
        {
            var inner = new TransientFailingService(failCount: 2);
            var retry = new RetryingOrganizationService(inner, maxRetries: 3, baseDelayMs: 1);

            var id = retry.Create(new Entity("account"));
            Assert.AreNotEqual(Guid.Empty, id);
            Assert.AreEqual(3, inner.Attempts, "Should have attempted initial + retries.");
        }

        [TestMethod]
        public void RetryingService_Should_Stop_On_Permanent_Fault()
        {
            var retry = new RetryingOrganizationService(new NonTransientFailingService(), maxRetries: 3, baseDelayMs: 1);
            Assert.Throws<FaultException<OrganizationServiceFault>>(
                () => retry.Retrieve("account", Guid.NewGuid(), new ColumnSet(true)));
        }
    }
}