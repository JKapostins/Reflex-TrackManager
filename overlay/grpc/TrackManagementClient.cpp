#include "TrackManagementClient.h"

TrackManagementClient::TrackManagementClient(std::shared_ptr<grpc::Channel> channel)
	: m_stub(trackmanagement::TrackManager::NewStub(channel))
{
}

std::vector<trackmanagement::Track> TrackManagementClient::GetTracks()
{
	trackmanagement::Empty request;
	trackmanagement::TrackResponse reply;
	grpc::ClientContext context;
	std::vector<trackmanagement::Track> tracks;

	grpc::Status status = m_stub->GetTracks(&context, request, &reply);

	if (status.ok())
	{
		auto trackResponse = reply.tracks();
		tracks.reserve(trackResponse.size());

		for (auto& track : trackResponse)
		{
			tracks.push_back(track);
		}
	}
	return tracks;
}
