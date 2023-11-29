using Amazon.Connect;
using Amazon.Connect.Model;
using Amazon.ConnectCampaignService;
using Amazon.ConnectCampaignService.Model;
using Microsoft.AspNetCore.Mvc;
using AnswerMachineDetectionConfig = Amazon.Connect.Model.AnswerMachineDetectionConfig;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSService<IAmazonConnect>();
builder.Services.AddAWSService<IAmazonConnectCampaignService>();
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
    var describeContactRequest = new DescribeContactRequest
    {
        InstanceId = instanceId,
        ContactId = contactId
    };

    var contactDetails = await connectClient.DescribeContactAsync(describeContactRequest);
    return Results.Ok(contactDetails);
});

app.MapGet("/contacts/attributes/{instanceId}/{contactId}",
    async (string instanceId, string contactId, IAmazonConnect connectClient) =>
    {
        var getContactAttributesRequest = new GetContactAttributesRequest
        {
            InstanceId = instanceId,
            InitialContactId = contactId
        };

        var contactAttributes = await connectClient.GetContactAttributesAsync(getContactAttributesRequest);
        return Results.Ok(contactAttributes);
    });

app.MapGet("/metrics/{instanceId}/", async (string instanceId, IAmazonConnect connectClient) =>
{
    var resourceArn = $"arn:aws:connect:us-east-1:274182154370:instance/{instanceId}";
    var getMetricDataRequest = new GetMetricDataV2Request
    {
        ResourceArn = resourceArn,
        StartTime = DateTime.UtcNow.AddDays(-30),
        EndTime =  DateTime.Today,
        Interval = new IntervalDetails
        {
            IntervalPeriod = "TOTAL" ,
            TimeZone = "UTC"
        },
        //Groupings = new List<string>(){""},
        Filters = new List<FilterV2>()
        {
            new FilterV2()
            {
                FilterKey = "CHANNEL",
                FilterValues = new List<string>(){"VOICE"}
            },
            new FilterV2
            {
                FilterKey = "QUEUE",
                FilterValues = new List<string>{ "b463cbd6-f128-4769-9b40-25b97abf466f" }
            }
        },
        Metrics = new List<MetricV2>
        {
            new()
            {
                Name = "CONTACTS_HANDLED",
                MetricFilters = new List<MetricFilterV2>
                {
                    new MetricFilterV2
                    {
                        MetricFilterKey = "INITIATION_METHOD",
                        MetricFilterValues = new List<string> { "OUTBOUND" },
                    },
                },
            }
        }
    };

    var metrics = await connectClient.GetMetricDataV2Async(getMetricDataRequest);
    return Results.Ok(metrics);
});

app.MapGet("/campaign", async (IAmazonConnectCampaignService connectCampaign) =>
{
    var listCampaignsRequest = new ListCampaignsRequest();
    return await connectCampaign.ListCampaignsAsync(listCampaignsRequest);
});

app.MapGet("/campaign/{campaignId}/", async (string campaignId, IAmazonConnectCampaignService connectCampaign) =>
{

    var request = new GetCampaignStateRequest { Id = campaignId };
    var response = await connectCampaign.GetCampaignStateAsync(request);
    return Results.Ok(response.State);
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

