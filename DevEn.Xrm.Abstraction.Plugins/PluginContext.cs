using System;
using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins
{
    public class PluginContext
        : IPluginContext
    {
        public IPluginExecutionContext Context => _context?.Value;
        public IOrganizationServiceFactory ServiceFactory => _serviceFactory?.Value;
        public IOrganizationService UserOrganizationService => _userOrganizationService?.Value;
        public IOrganizationService SystemOrganizationService => _organizationService?.Value;
        public ITracingService TracingService => _tracingService?.Value;

        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<IPluginExecutionContext> _context;
        private readonly Lazy<IOrganizationServiceFactory> _serviceFactory;
        private readonly Lazy<IOrganizationService> _userOrganizationService;
        private readonly Lazy<IOrganizationService> _organizationService;
        private readonly Lazy<ITracingService> _tracingService;

        public PluginContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serviceFactory = InitializeLazyServiceProvider<IOrganizationServiceFactory>();
            _context = InitializeLazyServiceProvider<IPluginExecutionContext>();
            _userOrganizationService = InitializeLazyOrganizationService(Context.UserId);
            _organizationService = InitializeLazyOrganizationService(Context.InitiatingUserId);
            _tracingService = InitializeLazyServiceProvider<ITracingService>();
        }

        private Lazy<T> InitializeLazyServiceProvider<T>()
            => new Lazy<T>(GetService<T>);

        private Lazy<IOrganizationService> InitializeLazyOrganizationService(Guid id)
            => new Lazy<IOrganizationService>(() => ServiceFactory.CreateOrganizationService(id));

        private T GetService<T>()
            => (T)_serviceProvider.GetService(typeof(T));

        public T GetInputParameter<T>(string name)
        {
            if (Context.InputParameters?.Contains(name) == true)
                return (T)Context.InputParameters[name];
            return default;
        }

        public T GetOutputParameter<T>(string name)
        {
            if (Context.OutputParameters?.Contains(name) == true)
                return (T)Context.OutputParameters[name];
            return default;
        }

        public Entity GetTargetEntity()
        {
            if (Context.InputParameters?.Contains("Target") != true)
                return null;

            var target = Context.InputParameters["Target"];
            switch (target)
            {
                case Entity e:
                    return e;
                case EntityReference r:
                    return new Entity(r.LogicalName) { Id = r.Id };
            }
            return null;
        }

        public Entity GetPreImage(string name)
        {
            return Context.PreEntityImages?.Contains(name) == true
                ? Context.PreEntityImages[name]
                : null;
        }

        public Entity GetPostImage(string name)
        {
            return Context.PostEntityImages?.Contains(name) == true
                ? Context.PostEntityImages[name]
                : null;
        }

        public T GetSharedVariable<T>(string name)
        {
            if (Context.SharedVariables?.Contains(name) == true)
                return (T)Context.SharedVariables[name];
            return default;
        }
    }
}
