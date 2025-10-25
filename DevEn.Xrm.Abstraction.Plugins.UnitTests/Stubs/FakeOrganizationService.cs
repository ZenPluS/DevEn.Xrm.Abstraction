using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs
{
    public class FakeOrganizationService
        : IOrganizationService
    {
        public Entity LastUpdatedEntity { get; private set; }
        public Entity RetrievedEntity { get; set; }

        public Guid Create(Entity entity)
        {
            throw new NotImplementedException();
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            // Return preset entity or a default with requested id
            if (RetrievedEntity != null)
                return RetrievedEntity;

            var e = new Entity(entityName) { Id = id };
            if (columnSet?.Columns.Contains("description") == true)
            {
                e["description"] = "Original description";
            }
            return e;
        }

        public void Update(Entity entity)
        {
            LastUpdatedEntity = entity;
        }

        public void Delete(string entityName, Guid id)
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            throw new NotImplementedException();
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            throw new NotImplementedException();
        }
    }
}