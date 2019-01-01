#pragma once
#include <grpcpp/grpcpp.h>
#include <memory>
#include "trackmanagement.grpc.pb.h"

class TrackManagementClient 
{
 public:
	 TrackManagementClient(std::shared_ptr<grpc::Channel> channel);
	 std::vector<trackmanagement::Track> GetTracks();

 private:
  std::unique_ptr<trackmanagement::TrackManager::Stub> m_stub;
};
