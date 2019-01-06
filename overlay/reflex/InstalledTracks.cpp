#include "InstalledTracks.h"
#include "imgui/imgui.h"


InstalledTracks::InstalledTracks(std::shared_ptr<TrackManagementClient> client, const char* trackType)
	: m_trackManagementClient(client)
	, m_trackType(trackType)
	, m_selectedTrackName("")
{
}

InstalledTracks::~InstalledTracks()
{
}

void InstalledTracks::render()
{
	const int bufferLength = 32;
	char windowTitle[bufferLength];	
	sprintf_s(windowTitle, bufferLength, "Installed %s Tracks", m_trackType.c_str());

	trackmanagement::InstalledTrackRequest request;
	request.set_tracktype(m_trackType.c_str());
	
	auto installedTracks = m_trackManagementClient->getInstalledTracks(request);
	if (m_selectedTrackName.size() == 0 && installedTracks.size() > 0)
	{
		m_selectedTrackName = installedTracks[0].name();
	}

	if (ImGui::Begin(windowTitle, nullptr))
	{
		drawTable(installedTracks);

		bool selectedIsFavorite = false;
		std::string selected = m_selectedTrackName;
		auto trackIter = std::find_if(installedTracks.begin(), installedTracks.end(), [&selected](const trackmanagement::Track& obj) {return obj.name() == selected; });
		if (trackIter != installedTracks.end())
		{
			selectedIsFavorite = trackIter->favorite();
		}
		drawActions(selectedIsFavorite);
	}
	ImGui::End();
}

void InstalledTracks::drawTable(const std::vector<trackmanagement::Track>& tracks)
{
	static float height = 14.0f;
	static float tableWidth = 830.0f;
	ImGui::SetNextWindowContentSize(ImVec2(tableWidth, 0.0f));
	ImGui::BeginChild("installed tracks body", ImVec2(0, ImGui::GetFontSize() * height), true, ImGuiWindowFlags_HorizontalScrollbar);
	ImGui::Columns(6, "installedTracks");

	setTableColumnWidth();

	ImGui::Text("Name"); ImGui::NextColumn();
	ImGui::Text("Slot"); ImGui::NextColumn();
	ImGui::Text("Favorite"); ImGui::NextColumn();
	ImGui::Text("Author"); ImGui::NextColumn();
	ImGui::Text("Date Created"); ImGui::NextColumn();
	ImGui::Text("Downloads"); ImGui::NextColumn();
	ImGui::Separator();

	for (auto& track : tracks)
	{
		if (ImGui::Selectable(track.name().c_str(), m_selectedTrackName == track.name(), ImGuiSelectableFlags_SpanAllColumns))
		{
			m_selectedTrackName = track.name();
		}
		bool hovered = ImGui::IsItemHovered();
		ImGui::NextColumn();

		ImGui::Text("%d", track.slot()); ImGui::NextColumn();
		ImGui::Text(track.favorite() ? "true" : "false"); ImGui::NextColumn();
		ImGui::Text(track.author().c_str()); ImGui::NextColumn();
		ImGui::Text(track.date().c_str()); ImGui::NextColumn();
		ImGui::Text("%d", track.downloads()); ImGui::NextColumn();
		
	}
	ImGui::EndChild();
}
void InstalledTracks::drawActions(bool favorite)
{
	ImGui::BeginChild("actions");
	
	if (ImGui::Button(favorite ? "Remove from Favorites" : "Add to Favorites"))
	{
		if (m_selectedTrackName.size() > 0)
		{
			m_trackManagementClient->toggleFavorite(m_selectedTrackName.c_str());
		}
	}
	ImGui::SameLine();
	if (ImGui::Button("Share Track List"))
	{
		m_trackManagementClient->shareTrackList(m_trackType.c_str());
	}
	ImGui::EndChild();
}

void InstalledTracks::setTableColumnWidth()
{
	static const float nameWidth = 350.0f;
	static const float slotWidth = 54.0f;
	static const float authorWidth = 128.0f;
	static const float dateWidth = 128.0f;
	static const float downloadsWidth = 84.0f;
	static const float favoriteWidth = 84.0f;

	ImGui::SetColumnWidth(0, nameWidth);
	ImGui::SetColumnWidth(1, slotWidth);
	ImGui::SetColumnWidth(2, favoriteWidth);
	ImGui::SetColumnWidth(3, authorWidth);
	ImGui::SetColumnWidth(4, dateWidth);
	ImGui::SetColumnWidth(5, downloadsWidth);
	
}
