using MCP.Host.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;

namespace MCP.Host.Agents;

#pragma warning disable SKEXP0080
public class CodingAgentProcessMessageChannel(
    string implementationTaskConnectionId,
    IHubContext<CodeAgentHub, ICodeAgentHub> hubContext) : IExternalKernelProcessMessageChannel
{
    //private MyCustomClient? _customClient;

    // Example of an implementation for the process
    public ValueTask Initialize()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Uninitialize()
    {
        return ValueTask.CompletedTask;
    }

    public async Task EmitExternalEventAsync(string externalTopicEvent, KernelProcessProxyMessage message)
    {
        switch (externalTopicEvent)
        {
            case "RequestUserReview": 
                var requestDocument = message.EventData?.Content;
                if(requestDocument != null)
                {
                    await hubContext.Clients.Client(implementationTaskConnectionId).ReceiveUserReviewAsync(requestDocument);
                }

                break;
            case "PublishDocumentation":
                var publishedDocument = message.EventData?.Content;
                if (publishedDocument != null)
                {
                    await hubContext.Clients.Client(implementationTaskConnectionId).ReceiveUserReviewAsync(publishedDocument);
                    // As an example only writing the request document to the response
                    //await response.WriteAsync($"Requesting user review for document: {publishedDocument}");
                    //await response.Body.FlushAsync();
                }

                break;
        }

        //// logic used for emitting messages externally.
        //// Since all topics are received here potentially 
        //// some if else/switch logic is needed to map correctly topics with external APIs/endpoints.
        //if (this._customClient != null)
        //{
        //    switch (externalTopicEvent)
        //    {
        //        case "RequestUserReview":
        //            var requestDocument = message.EventData.ToObject() as DocumentInfo;
        //            // As an example only invoking a sample of a custom client with a different endpoint/api route
        //            this._customClient.InvokeAsync("REQUEST_USER_REVIEW", requestDocument);
        //            return;

        //        case "PublishDocumentation":
        //            var publishedDocument = message.EventData.ToObject() as DocumentInfo;
        //            // As an example only invoking a sample of a custom client with a different endpoint/api route
        //            this._customClient.InvokeAsync("PUBLISH_DOC_EXTERNALLY", publishedDocument);
        //            return;
        //    }
        //}
    }

    //public async ValueTask Initialize()
    //{
    //    // logic needed to initialize proxy step, can be used to initialize custom client
    //    this._customClient = new MyCustomClient("http://localhost:8080");
    //    this._customClient.Initialize();
    //}

    //public async ValueTask Uninitialize()
    //{
    //    // Cleanup to be executed when proxy step is uninitialized
    //    if (this._customClient != null)
    //    {
    //        await this._customClient.ShutdownAsync();
    //    }
    //}
}