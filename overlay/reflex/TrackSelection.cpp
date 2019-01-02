#include "TrackSelection.h"
#include "grpc/TrackManagementClient.h"
#include "imgui/imgui.h"

TrackSelection::TrackSelection(std::shared_ptr<TrackManagementClient> client)
	: m_trackManagementClient(client)
	, m_selectedTrack("")
{
}

TrackSelection::~TrackSelection()
{
}

void TrackSelection::render()
{
	ImGui::SetNextWindowSize(ImVec2(500, 440), ImGuiCond_FirstUseEver);
	if (ImGui::Begin("Track Selection"))
	{
		auto tracks = m_trackManagementClient->GetTracks();

		ImGui::Columns(7, "availabletracks"); // 4-ways, with border
		ImGui::Separator();		
		ImGui::Text("Name"); ImGui::NextColumn();
		ImGui::Text("Slot"); ImGui::NextColumn();
		ImGui::Text("Type"); ImGui::NextColumn();
		ImGui::Text("Author"); ImGui::NextColumn();
		ImGui::Text("Date Created"); ImGui::NextColumn();
		ImGui::Text("Downloads"); ImGui::NextColumn();
		ImGui::Text("Favorite"); ImGui::NextColumn();
		ImGui::Separator();

		for (auto& track : tracks)
		{
			if (ImGui::Selectable(track.name().c_str(), m_selectedTrack == track.name(), ImGuiSelectableFlags_SpanAllColumns))
			{
				m_selectedTrack = track.name();
			}
			bool hovered = ImGui::IsItemHovered();
			ImGui::NextColumn();

			ImGui::Text("%d", track.slot()); ImGui::NextColumn();
			ImGui::Text(track.type().c_str()); ImGui::NextColumn();
			ImGui::Text(track.author().c_str()); ImGui::NextColumn();
			ImGui::Text(track.date().c_str()); ImGui::NextColumn();
			ImGui::Text("%d", track.downloads()); ImGui::NextColumn();
			ImGui::Text("<3"); ImGui::NextColumn();
		}
		ImGui::Columns(1);
		ImGui::Separator();
	}
	ImGui::End();
}