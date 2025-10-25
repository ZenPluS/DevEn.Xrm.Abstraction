using Microsoft.Xrm.Sdk;
using System;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs
{
    public class FakeOrganizationServiceFactory
        : IOrganizationServiceFactory
    {
        private readonly IOrganizationService _service;

        public FakeOrganizationServiceFactory(IOrganizationService service)
        {
            _service = service;
        }

        public IOrganizationService CreateOrganizationService(Guid? userId)
        {
            return _service;
        }
    }
}