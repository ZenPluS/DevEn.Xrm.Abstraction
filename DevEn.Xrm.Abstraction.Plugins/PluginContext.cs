using System;
using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins
{
    /// <summary>
    /// Provides lazy access to Dataverse plugin execution context, tracing, user/system organization services,
    /// and convenience helpers for parameters, images and shared variables.
    /// Converts a Target <see cref="EntityReference"/> automatically into an <see cref="Entity"/> shell with Id + LogicalName.
    /// </summary>
    public class PluginContext
        : IPluginContext
    {
        /// <summary>Underlying pipeline execution context (lazy).</summary>
        public IPluginExecutionContext Context => _context?.Value;

        /// <summary>Factory used to create organization services (lazy).</summary>
        public IOrganizationServiceFactory ServiceFactory => _serviceFactory?.Value;

        /// <summary>User-scoped organization service (lazy).</summary>
        public IOrganizationService UserOrganizationService => _userOrganizationService?.Value;

        /// <summary>Initiating user (system) scoped organization service (lazy).</summary>
        public IOrganizationService SystemOrganizationService => _organizationService?.Value;

        /// <summary>Tracing service for diagnostic messages (lazy).</summary>
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

        private Lazy<T> InitializeLazyServiceProvider<T>() => new Lazy<T>(GetService<T>);
        private Lazy<IOrganizationService> InitializeLazyOrganizationService(Guid id)
            => new Lazy<IOrganizationService>(() => ServiceFactory.CreateOrganizationService(id));
        private T GetService<T>() => (T)_serviceProvider.GetService(typeof(T));

        public T GetInputParameter<T>(string name)
            => Context.InputParameters?.Contains(name) == true ? (T)Context.InputParameters[name] : default;

        public T GetOutputParameter<T>(string name)
            => Context.OutputParameters?.Contains(name) == true ? (T)Context.OutputParameters[name] : default;

        public void SetOutputParameter(string name, object value)
        {
            if (Context.OutputParameters == null)
                return;
            Context.OutputParameters[name] = value;
        }

        /// <summary>
        /// Returns the Target entity (Entity or converted from EntityReference).
        /// Null if absent or unsupported type.
        /// </summary>
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
                default:
                    return null;
            }
        }

        public Entity GetPreImage(string name)
            => Context.PreEntityImages?.Contains(name) == true ? Context.PreEntityImages[name] : null;

        public Entity GetPostImage(string name)
            => Context.PostEntityImages?.Contains(name) == true ? Context.PostEntityImages[name] : null;

        public T GetSharedVariable<T>(string name)
            => Context.SharedVariables?.Contains(name) == true ? (T)Context.SharedVariables[name] : default;
    }
}