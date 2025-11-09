using DevEn.Xrm.Abstraction.Workflows.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace DevEn.Xrm.Abstraction.Workflows
{
    /// <summary>
    /// Provides a lazy-access façade over WF <see cref="CodeActivityContext"/> exposing:
    /// Dataverse <see cref="IWorkflowContext"/>, tracing, service factory, user & system scoped organization services,
    /// plus helpers for reading inputs and setting outputs.
    /// </summary>
    /// <remarks>
    /// All dependent services are resolved lazily the first time they are accessed.
    /// This avoids unnecessary overhead when an activity is skipped by validation.
    /// The user and system organization services are instantiated using their respective Ids
    /// from the current <see cref="IWorkflowContext"/> (so that context must be resolvable).
    /// </remarks>
    public class WorkflowActivityContext
        : IWorkflowActivityContext
    {
        /// <summary>Underlying workflow context for current execution (lazy).</summary>
        public IWorkflowContext WorkflowContext => _context?.Value;

        /// <summary>Organization service scoped to the current (running) user (lazy).</summary>
        public IOrganizationService UserOrganizationService => _userOrganizationService?.Value;

        /// <summary>Factory used to create organization services (lazy).</summary>
        public IOrganizationServiceFactory ServiceFactory => _serviceFactory?.Value;

        /// <summary>Organization service scoped to initiating user (system-level) (lazy).</summary>
        public IOrganizationService SystemOrganizationService => _organizationService?.Value;

        /// <summary>Tracing service for diagnostic output (lazy).</summary>
        public ITracingService TracingService => _tracingService?.Value;

        private readonly CodeActivityContext _codeContext;
        private readonly Lazy<IWorkflowContext> _context;
        private readonly Lazy<IOrganizationServiceFactory> _serviceFactory;
        private readonly Lazy<IOrganizationService> _userOrganizationService;
        private readonly Lazy<IOrganizationService> _organizationService;
        private readonly Lazy<ITracingService> _tracingService;

        /// <summary>
        /// Initializes the wrapper and prepares lazy resolution of all required extensions.
        /// </summary>
        /// <param name="codeContext">Underlying WF execution context.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="codeContext"/> is null.</exception>
        public WorkflowActivityContext(CodeActivityContext codeContext)
        {
            _codeContext = codeContext ?? throw new ArgumentNullException(nameof(codeContext));

            _context = InitializeExtensionAsLazy<IWorkflowContext>();
            _serviceFactory = InitializeExtensionAsLazy<IOrganizationServiceFactory>();
            _tracingService = InitializeExtensionAsLazy<ITracingService>();
            // These two depend on WorkflowContext having been resolved before Value is read.
            _userOrganizationService = InitializeLazyOrganizationService(WorkflowContext.UserId);
            _organizationService = InitializeLazyOrganizationService(WorkflowContext.InitiatingUserId);
        }

        private Lazy<T> InitializeExtensionAsLazy<T>()
            where T : class
            => new Lazy<T>(() => _codeContext.GetExtension<T>());

        private Lazy<IOrganizationService> InitializeLazyOrganizationService(Guid id)
            => new Lazy<IOrganizationService>(() => ServiceFactory.CreateOrganizationService(id));

        /// <summary>
        /// Reads an <see cref="InArgument{T}"/> safely. Returns default(T) if argument is null.
        /// </summary>
        public T GetInput<T>(InArgument<T> argument, CodeActivityContext ctx)
            => argument == null ? default : argument.Get(ctx);

        /// <summary>
        /// Assigns a value to an <see cref="OutArgument{T}"/> only if it is not null.
        /// </summary>
        public void SetOutput<T>(OutArgument<T> argument, CodeActivityContext ctx, T value)
            => argument?.Set(ctx, value);
    }
}