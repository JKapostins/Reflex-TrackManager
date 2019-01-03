#include "TrackManagementClient.h"

TrackManagementClient::TrackManagementClient(std::shared_ptr<grpc::Channel> channel)
	: m_stub(new trackmanagement::TrackManager::Stub(channel))
{
}
TrackManagementClient::~TrackManagementClient()
{
	//GNARLY_TODO: Send disconnect message to the server.

	//Note: Leaking on purpose here because attempting to delete grpc generated object causes hang.
	//This code only gets called when reflex is closed so the OS will clean it up.
	//delete m_stub;
}

std::vector<trackmanagement::Track> TrackManagementClient::GetTracks(const trackmanagement::TrackRequest& request)
{
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
