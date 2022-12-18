//C# sourcecode
/**********************************************************************************************************************
//Author: Param Varun Reddy (varunreddyparam@rexstudios.io)
//Created: 2022-02-11
//Description: Base Class Used for p;ugin development, 
  IPlugin Interface is implemented and Plugin related IServicePovider and plugin context will be embeded under one object
***********************************************************************************************************************/

namespace RexStudios.PluginDevelopment
{
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Collections.Generic;
    using System.Globalization;


    public abstract class PluginBase : IPlugin
    {

        public class LocalPluginContext
        {

            public IServiceProvider ServiceProvider { get; private set; }

            public IOrganizationService OrganizationService { get; private set; }

            public ServiceContext ServiceContext { get; private set; }

            public IPluginExecutionContext PluginExecutionContext { get; private set; }

            public IServiceEndpointNotificationService EndpointNotificationService { get; private set; }

            public ITracingService TracingService { get; private set; }

            private LocalPluginContext() { }

            public LocalPluginContext(IServiceProvider serviceProvider)
            {
                if (serviceProvider == null)
                {
                    throw new InvalidPluginExecutionException(Constants.Plugin.ExceptionMessages.SERVICEPROVIDER);
                }

                // Get execution Context service from the service Provider
                PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Get tracing service from the service Provider
                TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                // Get Notification service from the service Provider
                EndpointNotificationService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));

                // Get the Organization service Factory from the service provider
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                OrganizationService = serviceFactory.CreateOrganizationService(PluginExecutionContext.UserId);

                ServiceContext = new ServiceContext(OrganizationService);
            }

            /// <summary>
            /// Writes a trace Messageto the CRM trace log.
            /// </summary>
            /// <param name="message">Message name to trace</param>
            public void Trace(string message)
            {
                if (string.IsNullOrWhiteSpace(message) || TracingService == null)
                {
                    return;
                }

                if (PluginExecutionContext == null)
                {
                    TracingService.Trace(message);
                }
                else
                {
                    TracingService.Trace("{0}, Correlation Id: {1}, Initiating User: {2}",
                        message,
                        PluginExecutionContext.CorrelationId,
                        PluginExecutionContext.InitiatingUserId);
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
        public PluginBase(Type childClassName)
        {
            ChildClassName = childClassName.ToString();
        }


        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException(Constants.Plugin.ExceptionMessages.SERVICEPROVIDER);
            }

            LocalPluginContext localContext = new LocalPluginContext(serviceProvider);
            localContext.Trace(string.Format(CultureInfo.InvariantCulture, "Entered {0}.Execute()", this.ChildClassName));

            try
            {
                //Invoke the custom plugin code implementation
                ExecuteCrmPlugin(localContext);
                //now exit - if the derived plugin has incorrectly registered or overlapping event registration.
                //guard against multiple executions.
                return;
            }
            catch (InvalidPluginExecutionException exp)
            {
                throw new InvalidPluginExecutionException(exp.Message);
            }

            catch (Exception ex)
            {
                localContext.Trace(string.Format("{0}: {1}", Constants.Plugin.ContextProperties.STAGE, localContext.PluginExecutionContext.Stage));
                localContext.Trace(string.Format("{0}: {1}", Constants.Plugin.ContextProperties.DEPTH, localContext.PluginExecutionContext.Depth));
                localContext.Trace(string.Format("{0}: {1}", Constants.Plugin.ContextProperties.MESSAGENAME, localContext.PluginExecutionContext.MessageName));
                localContext.Trace(string.Format("{0}: {1}", Constants.Plugin.ContextProperties.PRIMARYENTITYID, localContext.PluginExecutionContext.PrimaryEntityId));
                localContext.Trace(string.Format("{0}: {1}", Constants.Plugin.ContextProperties.PRIMARYENTITYNAME, localContext.PluginExecutionContext.PrimaryEntityName));
                localContext.Trace(string.Format("{0}: {1}", Constants.Plugin.ContextProperties.USERID, localContext.PluginExecutionContext.UserId));
                localContext.Trace(string.Format("{0}: {1}", Constants.Plugin.ContextProperties.INITIATINGUSERID, localContext.PluginExecutionContext.InitiatingUserId));
                localContext.Trace(string.Format("{0}: {1}", "Stacktrace", ex.ToString()));
                throw new InvalidPluginExecutionException(Constants.Plugin.ExceptionMessages.UNABLE_TO_PROCESS_THE_REQUEST);

            }
            finally
            {
                localContext.Trace(string.Format(CultureInfo.InvariantCulture, "Exiting{0}.Execute()", this.ChildClassName));
            }
        }

        /// <summary>
        /// Place holder to custom plugin implementation
        /// </summary>
        /// <param name="localContext"></param>
        protected abstract void ExecuteCrmPlugin(LocalPluginContext localContext);

        /// <summary>
        /// Get Input Parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <param name="pluginContext"></param>
        /// <returns></returns>
        protected T GetinputParameter<T>(string keyName, IPluginExecutionContext pluginContext)
        {
            if (!pluginContext.InputParameters.ContainsKey(keyName))
                throw new KeyNotFoundException($"Key: {keyName} is not found in plugin input parameters");

            if (!(pluginContext.InputParameters[keyName] is T))
                throw new InvalidCastException($"invalid Type is Provide for key : {keyName}");

            return (T)pluginContext.InputParameters[keyName];

        }

        /// <summary>
        /// Get Plugin image
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="isPreImage"></param>
        /// <param name="pluginContext"></param>
        /// <returns></returns>
        protected Entity GetImageEntity(string keyName, bool isPreImage, IPluginExecutionContext pluginContext)
        {
            Entity entity;
            if (!ValidatePluginImage(keyName, isPreImage, pluginContext))
                throw new InvalidPluginExecutionException($"Key: {keyName} is not found in plugin images");

            if (isPreImage)
                entity = pluginContext.PreEntityImages[keyName];

            else
                entity = pluginContext.PostEntityImages[keyName];

            if (entity == null)
                throw new InvalidPluginExecutionException($"Key: {keyName} image is null");
            return entity;
        }

        /// <summary>
        /// Validate Image for plugin
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="isPreImageCheck"></param>
        /// <param name="pluginContext"></param>
        /// <returns></returns>
        protected bool ValidatePluginImage(string imageName, bool isPreImageCheck, IPluginExecutionContext pluginContext)
        {
            if (isPreImageCheck)
            {
                return pluginContext.PreEntityImages.ContainsKey(imageName);
            }
            else

                return pluginContext.PostEntityImages.ContainsKey(imageName);
        }

        /// <summary>
        /// Validate Input Parameters for create or Update
        /// </summary>
        /// <param name="pluginContext"></param>
        /// <returns></returns>
        protected bool ValidateInputParametersForCreateorUpdate(IPluginExecutionContext pluginContext)
        {
            if (pluginContext.InputParameters.ContainsKey(Constants.Plugin.Parameters.TARGET)
                && pluginContext.InputParameters[Constants.Plugin.Parameters.TARGET] is Entity)
                return true;
            return false;
        }
    }
}
