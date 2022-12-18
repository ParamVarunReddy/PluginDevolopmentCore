//C# sourcecode
/**********************************************************************************************************************
//Author: Param Varun Reddy (varunreddyparam@rexstudios.io)
//Created: 2022-02-11
//Description: Creates organizationa service context
***********************************************************************************************************************/

namespace RexStudios.PluginDevelopment
{
    public partial class ServiceContext : Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
    {
        public ServiceContext(Microsoft.Xrm.Sdk.IOrganizationService service) : base(service)
        {

        }
    }
}
