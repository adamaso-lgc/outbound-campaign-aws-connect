public record StartCallRequest(
    string DestinationPhoneNumber,
    string ContactFlowId,
    string InstanceId,
    string SourcePhoneNumber,
    string QueueId,
    string CampaignId);