#pragma once
#include <grpcpp/grpcpp.h>
#include <memory>
#include "trackmanagement.grpc.pb.h"

class TrackManagementClient 
{
 public:
	 TrackManagementClient(std::shared_ptr<grpc::Channel> channel);
	 ~TrackManagementClient();
	 std::vector<trackmanagement::Track> GetTracks(const trackmanagement::TrackRequest& request);

 private:
  trackmanagement::TrackManager::Stub* m_stub;
};
