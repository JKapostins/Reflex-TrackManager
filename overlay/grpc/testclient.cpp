/*
 *
 * Copyright 2015 gRPC authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

#include "testclient.h"

TrackManagementClient::TrackManagementClient(std::shared_ptr<Channel> channel)
	: stub_(TrackManager::NewStub(channel))
{
}

// Assembles the client's payload, sends it and presents the response back
// from the server.
std::vector<Track> TrackManagementClient::GetTracks()
{
	Empty request;
	// Container for the data we expect from the server.
	TrackResponse reply;

	// Context for the client. It could be used to convey extra information to
	// the server and/or tweak certain RPC behaviors.
	ClientContext context;

	// The actual RPC.
	Status status = stub_->GetTracks(&context, request, &reply);

	// Act upon its status.
	std::vector<Track> tracks;
	if (status.ok()) {
		auto trackResponse = reply.tracks();
		tracks.reserve(trackResponse.size());

		for (auto& track : trackResponse) {
			tracks.push_back(track);
		}

	}
	return tracks;
}
