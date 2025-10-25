using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace DevEn.Xrm.Abstraction.Workflows.UnitTests.Stubs
{
    public class FakeOrganizationService : IOrganizationService
    {
        public Guid Create(Entity entity) => entity.Id != Guid.Empty ? entity.Id : Guid.NewGuid();
        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet) => new Entity(entityName) { Id = id };
        public void Update(Entity entity) { }
        public void Delete(string entityName, Guid id) { }
        public OrganizationResponse Execute(OrganizationRequest request) => new OrganizationResponse();
        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { }
        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities) { }
        public EntityCollection RetrieveMultiple(QueryBase query) => new EntityCollection();
    }
}