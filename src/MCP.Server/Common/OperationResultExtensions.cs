using ModelContextProtocol.Protocol;

namespace MCP.Server.Common;

public static class OperationResultExtensions
{
    public static CallToolResult ToCallToolResult(this OperationResult result)
    {
        return result.IsSuccess
            ? new CallToolResult
            {
                IsError = false
            }
            : new CallToolResult
            {
                IsError = true,
                Content = new List<ContentBlock>
                {
                    new TextContentBlock
                    {
                        Text = result.ErrorMessage!
                    }
                }
            };
    }

    public static CallToolResult ToCallToolResult(this OperationResult<string> result)
    {
        return result.IsSuccess
            ? new CallToolResult
            {
                IsError = false,
                Content = new List<ContentBlock>
                {
                    new TextContentBlock
                    {
                        Text = result.Data!
                    }
                }
            }
            : new CallToolResult
            {
                IsError = true,
                Content = new List<ContentBlock>
                {
                    new TextContentBlock
                    {
                        Text = result.ErrorMessage!
                    }
                }
            };
    }
}