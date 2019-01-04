#pragma once
#include <grpcpp/grpcpp.h>
#include <memory>
#include "trackmanagement.grpc.pb.h"

class TrackManagementClient 
{
 public:
	 TrackManagementClient(std::shared_ptr<grpc::Channel> channel);
	 ~TrackManagementClient();
	 std::vector<trackmanagement::Track> getTracks(const trackmanagement::TrackRequest& request) const;
	 std::vector<trackmanagement::LogMessage> getLogMessages() const;
	 std::string getInstallStatus() const;
	 void installRandomNationals();
	 void installRandomSupercross();
	 void installRandomFreeRides();
	 void installSelectedTrack(const char* trackName);

 private:
  trackmanagement::TrackManager::Stub* m_stub;
};
