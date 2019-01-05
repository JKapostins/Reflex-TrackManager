#include "SharedTracks.h"
#include "grpc/TrackManagementClient.h"
#include "imgui/imgui.h"


SharedTracks::SharedTracks(std::shared_ptr<TrackManagementClient> client)
	: m_trackManagementClient(client)

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
	static float height = 14.0f;
	static float tableWidth = 400.0f;
	ImGui::SetNextWindowContentSize(ImVec2(tableWidth, 0.0f));
	ImGui::BeginChild("installed tracks body", ImVec2(0, ImGui::GetFontSize() * height), true, ImGuiWindowFlags_HorizontalScrollbar);
	ImGui::Columns(3, "installedTracks");

	ImGui::Text("Track Set Name"); ImGui::NextColumn();
	ImGui::Text("Type"); ImGui::NextColumn();
	ImGui::Text("Expires"); ImGui::NextColumn();

	ImGui::Separator();

	ImGui::Text("%d", "PlaceHolderName"); ImGui::NextColumn();
	ImGui::Text("Supercross"); ImGui::NextColumn();
	ImGui::Text("4:38"); ImGui::NextColumn();

	ImGui::EndChild();
}
void SharedTracks::drawActions()
{
	ImGui::BeginChild("actions");
	if (ImGui::Button("Installed selected"))
	{
	}
	ImGui::EndChild();
}

