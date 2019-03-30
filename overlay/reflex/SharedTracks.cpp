#include "SharedTracks.h"
#include "grpc/TrackManagementClient.h"
#include "imgui/imgui.h"
#include "GameWindow.h"


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
	ImVec2 windowDimensions = game_window::ScaleWindowSize(ImVec2(466, 244), m_trackManagementClient->getGameWindowRect());
	ImGui::SetNextWindowSize(windowDimensions, ImGuiCond_Always);
	ImGui::SetNextWindowPos(game_window::ScaleWindowSize(ImVec2(976, 49), m_trackManagementClient->getGameWindowRect()), ImGuiCond_Always);
	bool adjustContentSize = m_trackManagementClient->getGameWindowRect().x < 1920;
	static float offset = 5;
	float contentHeight = windowDimensions.y + offset;

	if (adjustContentSize)
	{
		ImGui::SetNextWindowContentSize(ImVec2(0, contentHeight));
	}
	if (ImGui::Begin("Publicly Shared Track Lists", nullptr, ImGuiWindowFlags_NoResize))
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

	ImVec2 tableSize = game_window::ScaleWindowSize(ImVec2(400, 14), m_trackManagementClient->getGameWindowRect());
	tableSize.x = 400;
	ImGui::SetNextWindowContentSize(ImVec2(tableSize.x, 0.0f));
	ImGui::BeginChild("installed tracks body", ImVec2(0, ImGui::GetFontSize() * tableSize.y), true, ImGuiWindowFlags_HorizontalScrollbar);
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
	if (ImGui::Button("Install Selected Track Set"))
	{
		if (m_selectedListName.size() > 0)
		{
			m_trackManagementClient->installSharedTracks(m_selectedListName.c_str());
		}
	}
	ImGui::EndChild();
}

