//C# sourcecode
/**********************************************************************************************************************
//Author: Param Varun Reddy (varunreddyparam@rexstudios.io)
//Created: 2022-02-11
//Description: Constants Used fo the core base class development, can be expanded based on the Messages Used in development
***********************************************************************************************************************/

namespace RexStudios.PluginDevelopment
{
    public static partial class Constants
    {
        /// <summary>
        /// Plugin related Constants
        /// </summary>
        public static class Plugin
        {
            /// <summary>
            /// plugin messages
            /// </summary>
            public static class Messages
            {
                public const string UPDATE = "Update";
                public const string CREATE = "Create";
                public const string ASSOCIATE = "Associate";
                public const string DISASSOCIATE = "Disassociate";
                public const string Assign = "Assign";
                public const string RETRIEVEMULTIPLE = "RetrieveMultiple";
                public const string SETSTATE = "SetState";
            }

            public static class Status
            {
                public const string STATECODE = "statecode";
                public const string STATUSCODE = "statuscode";
            }

            /// <summary>
            /// Custom Exception Messages related to plugin
            /// </summary>
            public static class ExceptionMessages
            {
                public const string LOCALCONTEXT = "localContext";
                public const string SERVICEPROVIDER = "No service provider exist";
                public const string UNABLE_TO_PROCESS_THE_REQUEST = "Unable to process your request. Please try after some time. if problem persists please contact administrator";
            }

            /// <summary>
            /// parameters used to access Plugin Context Value
            /// </summary>
            public static class Parameters
            {
                public const string TARGET = "Target";
                public const string ENTITYMONIKER = "EntityMoniker";
                public const string ASSIGNEE = "Assignee";
                public const string RELATED_ENTITIES = "RelatedEntities";
                public const string RELATIONSHIP = "Relationship";
                public const string BUSINESSENTITYCOLLECTION = "BusinessEntityCollection";
                public const string EMAILID = "EmailID";
                public const string QUEUEID = "QueueId";
                public const string QUERY = "query";
            }

            /// <summary>
            /// P;lugin Context Properties
            /// </summary>
            public static class ContextProperties
            {
                public const string STAGE = "Stage";
                public const string STAGENAME = "StageName";
                public const string DEPTH = "Depth";
                public const string MESSAGENAME = "MessageName";
                public const string PRIMARYENTITYID = "PrimaryEntityId";
                public const string PRIMARYENTITYNAME = "PrimaryEntityName";
                public const string USERID = "UserId";
                public const string INITIATINGUSERID = "initiatingUserId";
                public const string BUSINESSUNITID = "BusinessUnitId";
                public const string NAME = "name";

            }

            /// <summary>
            /// Properties of plugin Image
            /// </summary>
            public static class Images
            {
                public const string IMAGE = "Image";
                public const string PRE_IMAGE = "PreImage";
                public const string POST_IMAGE = "PostImage";
            }

            /// <summary>
            /// Plugin Event Pipeline Stage
            /// </summary>
            public enum SdkMessageProcessingStepStage
            {
                Prevalidate = 10,
                Preoperation = 20,
                Postoperation = 40
            }

            /// <summary>
            /// SDK Message Processing Step Mode
            /// </summary>
            public enum SdkMessageProcessingStepMode
            {
                Synchronus = 0,
                Asynchronus = 1
            }

        }

        public static class Messages
        {
            public const string UPDATE = "Update";
            public const string CREATE = "Create";
            public const string ASSOCIATE = "Associate";
            public const string DISASSOCIATE = "Disassociate";
            public const string Assign = "Assign";
            public const string RETRIEVEMULTIPLE = "RetrieveMultiple";
            public const string SETSTATE = "SetState";
            public const string UPSERT = "Upsert";
            
        }
    }
}

