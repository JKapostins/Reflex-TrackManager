#include "SharedTracks.h"
#include "grpc/TrackManagementClient.h"
#include "imgui/imgui.h"


SharedTracks::SharedTracks(std::shared_ptr<TrackManagementClient> client)
	: m_trackManagementClient(client)
	, m_selectedListName("")
{
}

SharedTracks::~SharedTracks()
{
}

void SharedTracks::render()
{
	ImGui::SetNextWindowSize(ImVec2(466, 244), ImGuiCond_FirstUseEver);
	ImGui::SetNextWindowPos(ImVec2(976, 49), ImGuiCond_FirstUseEver);
	if (ImGui::Begin("Publicly Shared Track Lists", nullptr))
	{
		drawTable();
		drawActions();
	}
	ImGui::End();
}

void SharedTracks::drawTable()
{
	auto sharedList = m_trackManagementClient->getSharedTracks();
	if (sharedList.size() == 0)
	{
		m_selectedListName = "";
	}
	else if (m_selectedListName.size() == 0)
	{
		m_selectedListName = sharedList[0].name();
	}


	static float height = 14.0f;
	static float tableWidth = 400.0f;
	ImGui::SetNextWindowContentSize(ImVec2(tableWidth, 0.0f));
	ImGui::BeginChild("installed tracks body", ImVec2(0, ImGui::GetFontSize() * height), true, ImGuiWindowFlags_HorizontalScrollbar);
	ImGui::Columns(3, "installedTracks");

	ImGui::Text("Track Set Name"); ImGui::NextColumn();
	ImGui::Text("Type"); ImGui::NextColumn();
	ImGui::Text("Expires"); ImGui::NextColumn();
	ImGui::Separator();

	for (auto& track : sharedList)
	{
		if (ImGui::Selectable(track.name().c_str(), m_selectedListName == track.name(), ImGuiSelectableFlags_SpanAllColumns))
		{
			m_selectedListName = track.name();
		}
		bool hovered = ImGui::IsItemHovered();
		ImGui::NextColumn();

		ImGui::Text(track.type().c_str()); ImGui::NextColumn();
		ImGui::Text(track.expires().c_str()); ImGui::NextColumn();

	}
	ImGui::EndChild();
}
void SharedTracks::drawActions()
{
	ImGui::BeginChild("actions");
	if (ImGui::Button("Installed selected"))
	{
		if (m_selectedListName.size() > 0)
		{
			m_trackManagementClient->installSharedTracks(m_selectedListName.c_str());
		}
	}
	ImGui::EndChild();
}

