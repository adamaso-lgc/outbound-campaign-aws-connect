using Amazon.Connect;
using Amazon.Connect.Model;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSService<IAmazonConnect>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/start-call", async ([FromBody] StartCallRequest request, IAmazonConnect connectClient) =>
{
    try
    {
        var response = await StartOutboundVoiceContactAsync(request, connectClient);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/contacts/{instanceId}/{contactId}", async (string instanceId, string contactId, IAmazonConnect connectClient) =>
{

    var contactDetails = await GetContactDetails(instanceId, contactId, connectClient);
    return Results.Ok(contactDetails);
});


app.Run();

async Task<StartOutboundVoiceContactResponse> StartOutboundVoiceContactAsync(StartCallRequest request, IAmazonConnect connectClient)
{
    var startOutboundVoiceContactRequest = new StartOutboundVoiceContactRequest
    {
        DestinationPhoneNumber = request.DestinationPhoneNumber,
        ContactFlowId = request.ContactFlowId,
        InstanceId = request.InstanceId,
        SourcePhoneNumber = request.SourcePhoneNumber,
        QueueId = request.QueueId,
        AnswerMachineDetectionConfig = new AnswerMachineDetectionConfig()
        {
            EnableAnswerMachineDetection = true,
            AwaitAnswerMachinePrompt = true
        },
        CampaignId = request.CampaignId,
        TrafficType = TrafficType.CAMPAIGN
    };
    
    return await connectClient.StartOutboundVoiceContactAsync(startOutboundVoiceContactRequest);
}

async Task<DescribeContactResponse> GetContactDetails(string instanceId, string contactId, IAmazonConnect connectClient)
{
    var describeContactRequest = new DescribeContactRequest
    {
        InstanceId = instanceId,
        ContactId = contactId
    };

    return await connectClient.DescribeContactAsync(describeContactRequest);
}

