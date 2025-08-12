namespace MCP.Host.Plugins;

public static class PluginDescriptions
{
    public static class SphraivaPlugin
    {
        public const string NAME = "Sphraiva";

        public static class Functions
        {
            public const string CREATE_DEV_CONTAINER = $"{NAME}_create_dev_container";
            public const string RUN_COMMAND_IN_DEV_CONTAINER = $"{NAME}_run_command_in_dev_container";
            public const string CLEANUP_DEV_CONTAINER = $"{NAME}_cleanup_dev_container";

            public const string CLONE_REPOSITORY_IN_DEV_CONTAINER = $"{NAME}_clone_repository_in_dev_container";
            
        }
    }
}
