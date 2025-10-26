using DevEn.Xrm.Abstraction.Workflows.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Runtime.Remoting.Contexts;

namespace DevEn.Xrm.Abstraction.Workflows
{
    /// <summary>
    /// Concrete implementation of <see cref="IWorkflowActivityContext"/> wrapping WF <see cref="CodeActivityContext"/>
    /// and exposing Dataverse services, workflow context and tracing.
    /// </summary>
    public class WorkflowActivityContext
        : IWorkflowActivityContext
    {
        /// <summary>Underlying workflow context for current execution.</summary>
        public IWorkflowContext WorkflowContext => _context?.Value;

        /// <summary>User-scoped organization service.</summary>
        public IOrganizationService UserOrganizationService => _userOrganizationService?.Value;

        /// <summary>Factory used to create organization services.</summary>
        public IOrganizationServiceFactory ServiceFactory => _serviceFactory?.Value;

        /// <summary>System-scoped (initiating user) organization service.</summary>
        public IOrganizationService SystemOrganizationService => _organizationService?.Value;

        /// <summary>Tracing service for diagnostic output.</summary>
        public ITracingService TracingService => _tracingService?.Value;

        private readonly CodeActivityContext _codeContext;
        private readonly Lazy<IWorkflowContext> _context;
        private readonly Lazy<IOrganizationServiceFactory> _serviceFactory;
        private readonly Lazy<IOrganizationService> _userOrganizationService;
        private readonly Lazy<IOrganizationService> _organizationService;
        private readonly Lazy<ITracingService> _tracingService;

        /// <summary>
        /// Constructs a new workflow activity context wrapper.
        /// Initializes workflow context, organization services and tracing service.
        /// </summary>
        /// <param name="codeContext">WF execution context provided by runtime.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="codeContext"/> is null.</exception>
        public WorkflowActivityContext(CodeActivityContext codeContext)
        {
            _codeContext = codeContext ?? throw new ArgumentNullException(nameof(codeContext));

            _context = InitializeExtensionAsLazy<IWorkflowContext>();
            _serviceFactory = InitializeExtensionAsLazy<IOrganizationServiceFactory>();
            _tracingService = InitializeExtensionAsLazy<ITracingService>();
            _userOrganizationService = InitializeLazyOrganizationService(WorkflowContext.UserId);
            _organizationService = InitializeLazyOrganizationService(WorkflowContext.InitiatingUserId);
        }

        private Lazy<T> InitializeExtensionAsLazy<T>()
            where T : class
            => new Lazy<T>(() => _codeContext.GetExtension<T>());

        private Lazy<IOrganizationService> InitializeLazyOrganizationService(Guid id)
            => new Lazy<IOrganizationService>(() => ServiceFactory.CreateOrganizationService(id));

        /// <summary>
        /// Retrieves the value of an input argument. Returns default if the argument is null.
        /// </summary>
        public T GetInput<T>(InArgument<T> argument, CodeActivityContext ctx)
            => argument == null ? default : argument.Get(ctx);

        /// <summary>
        /// Sets the value of an output argument if the argument is not null.
        /// </summary>
        public void SetOutput<T>(OutArgument<T> argument, CodeActivityContext ctx, T value)
            => argument?.Set(ctx, value);
    }
}