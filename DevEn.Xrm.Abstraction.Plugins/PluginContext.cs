using System;
using DevEn.Xrm.Abstraction.Plugins.Core;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Abstraction.Plugins
{
    /// <summary>
    /// Wraps Dataverse service provider to lazily expose execution context, organization services (user & system),
    /// tracing service, and helper methods for common parameter/image retrieval operations.
    /// </summary>
    public class PluginContext
        : IPluginContext
    {
        /// <summary>Underlying pipeline execution context.</summary>
        public IPluginExecutionContext Context => _context?.Value;

        /// <summary>Factory used to create organization services.</summary>
        public IOrganizationServiceFactory ServiceFactory => _serviceFactory?.Value;

        /// <summary>Organization service scoped to current user.</summary>
        public IOrganizationService UserOrganizationService => _userOrganizationService?.Value;

        /// <summary>Organization service scoped to initiating user (system-level).</summary>
        public IOrganizationService SystemOrganizationService => _organizationService?.Value;

        /// <summary>Tracing service for logging diagnostic messages.</summary>
        public ITracingService TracingService => _tracingService?.Value;

        private readonly IServiceProvider _serviceProvider;
        private readonly Lazy<IPluginExecutionContext> _context;
        private readonly Lazy<IOrganizationServiceFactory> _serviceFactory;
        private readonly Lazy<IOrganizationService> _userOrganizationService;
        private readonly Lazy<IOrganizationService> _organizationService;
        private readonly Lazy<ITracingService> _tracingService;

        /// <summary>
        /// Initializes the plugin context wrapper and prepares lazy service access.
        /// </summary>
        /// <param name="serviceProvider">Service provider received in plugin Execute.</param>
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

        /// <summary>
        /// Retrieves an input parameter by name, returning default if missing.
        /// </summary>
        public T GetInputParameter<T>(string name)
        {
            if (Context.InputParameters?.Contains(name) == true)
                return (T)Context.InputParameters[name];
            return default;
        }

        /// <summary>
        /// Retrieves an output parameter by name, returning default if missing.
        /// </summary>
        public T GetOutputParameter<T>(string name)
        {
            if (Context.OutputParameters?.Contains(name) == true)
                return (T)Context.OutputParameters[name];
            return default;
        }

        /// <summary>
        /// Sets (adds or overwrites) an output parameter value by name.
        /// </summary>
        public void SetOutputParameter(string name, object value)
        {
            if (Context.OutputParameters == null)
                return;
            Context.OutputParameters[name] = value;
        }

        /// <summary>
        /// Obtains the Target entity or converts a Target EntityReference into an entity shell with id and LogicalName.
        /// Returns null if Target is absent or unsupported type.
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
            }
            return null;
        }

        /// <summary>
        /// Retrieves a pre-image entity by name or null if not available.
        /// </summary>
        public Entity GetPreImage(string name)
        {
            return Context.PreEntityImages?.Contains(name) == true
                ? Context.PreEntityImages[name]
                : null;
        }

        /// <summary>
        /// Retrieves a post-image entity by name or null if not available.
        /// </summary>
        public Entity GetPostImage(string name)
        {
            return Context.PostEntityImages?.Contains(name) == true
                ? Context.PostEntityImages[name]
                : null;
        }

        /// <summary>
        /// Retrieves a shared variable by name, casting to the requested type or default if missing.
        /// </summary>
        public T GetSharedVariable<T>(string name)
        {
            if (Context.SharedVariables?.Contains(name) == true)
                return (T)Context.SharedVariables[name];
            return default;
        }
    }
}