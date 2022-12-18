

namespace RexStudios.WorkflowDevelopment
{
    using Microsoft.Xrm.Sdk.Workflow;
    using System.Activities;
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.Reflection;
    using System.Linq;
    using System.Text;
    using Microsoft.Xrm.Sdk;

    public abstract class WorkFlowActivityBase : CodeActivity
    {
        protected class LocalWorkflowContext
        {
            /// <summary>
            /// 
            /// </summary>
            internal IServiceProvider ServiceProvider { get; private set; }

            /// <summary>
            /// Microsoft Dynamics Organization Service
            /// </summary>
            internal IOrganizationService service { get; }

            /// <summary>
            /// Provides Logging Runtime information for plugins
            /// </summary>
            internal IWorkflowContext WorkflowContext { get; }

            /// <summary>
            /// it contains the information that describes the runtime environment in which work flow executes, information Related Entity to the executed , and entity business information
            /// </summary>
            internal ITracingService TracingService { get; }
            private LocalWorkflowContext() { }

            internal LocalWorkflowContext(CodeActivityContext executionContext)
            {
                if (executionContext == null)
                {
                    throw new ArgumentNullException(nameof(executionContext));
                }

                //Gets the workflow context from code executionContext
                WorkflowContext = executionContext.GetExtension<IWorkflowContext>();

                //Gets the tracingService from the executionContext
                TracingService = executionContext.GetExtension<ITracingService>();

                // gets the servicefactory from the service
                IOrganizationServiceFactory factory = executionContext.GetExtension<IOrganizationServiceFactory>();

                //Gets the organization Service  from the executionContext 
                service = factory.CreateOrganizationService(WorkflowContext.UserId);

            }

            internal void trace(string Message)
            {
                if (string.IsNullOrWhiteSpace(Message) || TracingService == null)
                {
                    return;
                }

                if (WorkflowContext == null)
                {
                    TracingService.Trace(Message);
                }
                else
                {
                    TracingService.Trace("{0}, Correlation Id: {1}, Initiating User: {2}",
                        Message,
                        WorkflowContext.CorrelationId,
                        WorkflowContext.InitiatingUserId);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the child class.
        /// </summary>
        /// <value>the name of the child class.</value>
        protected string ChildClassName { get; set; }

        /// <summary>
        /// Initializes a new Instance of the <see cref="PluginBase"/> class
        /// </summary>
        /// <param name="childClassName"></param>
        internal WorkFlowActivityBase(Type childClassName)
        {
            ChildClassName = childClassName.ToString();
        }

        protected override void Execute(CodeActivityContext activityContext)
        {
            LocalWorkflowContext localWorkflowContext = new LocalWorkflowContext(activityContext);
            localWorkflowContext.trace(string.Format(CultureInfo.InvariantCulture, "Entered {0}.Execute()", this.ChildClassName));
            TraceArguments(true, activityContext, localWorkflowContext);

            try
            {
                //Invoke the custom plugin code implementation
                ExecuteCrmWorkflowActivity(activityContext, localWorkflowContext);
                //now exit - if the derived plugin has incorrectly registered or overlapping event registration.
                //guard against multiple executions.
                TraceArguments(false, activityContext, localWorkflowContext);
                return;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                localWorkflowContext.trace(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", ex.Message.ToString()));
                throw;
            }
            finally
            {
                localWorkflowContext.trace(string.Format(CultureInfo.InvariantCulture, "Exiting{0}: Execute", ChildClassName));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityContext"></param>
        /// <param name="localWorkflowContext"></param>
        protected void ExecuteCrmWorkflowActivity(CodeActivityContext activityContext, LocalWorkflowContext localWorkflowContext)
        {

        }

        private void TraceArguments(bool input, CodeActivityContext context, LocalWorkflowContext localWorkflowContext)
        {
            try
            {
                PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                properties.ToList().ForEach(p =>
                {
                    if (input)
                    {
                        if (!p.PropertyType.IsSubclassOf(typeof(InArgument)) && !p.PropertyType.IsSubclassOf(typeof(InOutArgument)))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (!p.PropertyType.IsSubclassOf(typeof(OutArgument)) && !p.PropertyType.IsSubclassOf(typeof(InOutArgument)))
                        {
                            return;
                        }
                    }

                    var propertyLabel = input
                    ? ((InputAttribute)p.GetCustomAttribute(typeof(InputAttribute))).Name
                    : ((OutputAttribute)p.GetCustomAttribute(typeof(OutputAttribute))).Name;

                    StringBuilder sb = new StringBuilder();
                    sb.Append($"{p.PropertyType.BaseType?.Name}({p.PropertyType.GenericTypeArguments[0].FullName}):{propertyLabel}:");

                    Argument property = (Argument)p.GetValue(this);
                    object propertyValue = property.Get(context);
                    switch (propertyValue)
                    {
                        case null:
                            sb.Append("Null");
                            break;
                        case string _:
                        case decimal _:
                        case int _:
                        case bool _:
                            sb.Append(propertyValue.ToString());
                            break;
                        case DateTime _:
                            sb.Append(((DateTime)propertyValue).ToString("yyyy-MM-dd HH:mm:ss \"GMT\"Z"));
                            break;
                        case EntityReference _:
                            EntityReference er = (EntityReference)propertyValue;
                            sb.Append($"Id: {er.Id}, LogicalName: {er.LogicalName}");
                            break;
                        case OptionSetValue _:
                            sb.Append(((OptionSetValue)propertyValue).Value);
                            break;
                        case Money _:
                            sb.Append(((Money)propertyValue).Value.ToString(CultureInfo.InvariantCulture));
                            break;
                        default:
                            sb.Append($"Undefined Type-{p.GetType().FullName}");
                            break;
                    }
                    localWorkflowContext.TracingService.Trace(sb.ToString());
                });
            }

            catch (Exception ex)
            {
                localWorkflowContext.TracingService.Trace($"Error Tracing arguments:{ex.Message}");
            }
        }

    }
}
