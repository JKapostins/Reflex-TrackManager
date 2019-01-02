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

		static const float contentWidth = 900.0f;
		static const float height = 40.0f;

		ImGui::SetNextWindowContentSize(ImVec2(contentWidth, 0.0f));
		ImGui::BeginChild("##ScrollingRegion", ImVec2(0, ImGui::GetFontSize() * height), false, ImGuiWindowFlags_HorizontalScrollbar);

		ImGui::Columns(7, "availabletracks"); // 4-ways, with border

		static const float nameWidth = 350.0f;
		static const float slotWidth = 54.0f;
		static const float typeWidth = 100.0f;
		static const float authorWidth = 128.0f;
		static const float dateWidth = 128.0f;
		static const float downloadsWidth = 84.0f;
		static const float favoriteWidth = 84.0f;

		ImGui::SetColumnWidth(0, nameWidth);
		ImGui::SetColumnWidth(1, slotWidth);
		ImGui::SetColumnWidth(2, typeWidth);
		ImGui::SetColumnWidth(3, authorWidth);
		ImGui::SetColumnWidth(4, dateWidth);
		ImGui::SetColumnWidth(5, downloadsWidth);
		ImGui::SetColumnWidth(6, favoriteWidth);

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
		ImGui::EndChild();
		ImGui::Columns(1);
		ImGui::Separator();
	}
	ImGui::End();
}