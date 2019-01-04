#include "TrackSelection.h"
#include <turbojpeg.h>
#include <fstream>
#include "imgui/imgui.h"


const char* g_trackTypeComboItems[] =
{
	  "All Track Types"
	, "National"
	, "Supercross"
	, "FreeRide"
};

const char* g_slotComboItems[] =
{
	  "All Slots"
	, "1"
	, "2"
	, "3"
	, "4"
	, "5"
	, "6"
	, "7"
	, "8"
};

const char* g_sortByComboItems[] =
{
	  "Name"
	, "Slot"
	, "Type"
	, "Author"
	, "Date Created"
	, "Downloads"
	, "Favorite"
};

TrackSelection::TrackSelection(std::shared_ptr<TrackManagementClient> client)
	: m_trackManagementClient(client)
	, m_selectedTrackName("")
	, m_previouslySelectedTrackName("")
	, m_previewImage(nullptr)
	, m_previeImageWidth(0)
	, m_previewImageHeight(0)
	, m_trackTypeFilterIndex(0)
	, m_slotFilterIndex(0)
	, m_sortByIndex(1) // sort by slot by default
	, m_loadNewImage(false)
{
}

TrackSelection::~TrackSelection()
{
	if (m_previewImage != nullptr)
	{
		m_previewImage->Release();
		m_previewImage = nullptr;
	}
}

void TrackSelection::render(LPDIRECT3DDEVICE9 device)
{
	static float windowHeight = 948;
	static float windowWidth = 960;
	ImGui::SetNextWindowSize(ImVec2(windowWidth, windowHeight), ImGuiCond_Always);

	if (ImGui::Begin("Track Selection", nullptr, ImGuiWindowFlags_NoResize))
	{
		trackmanagement::TrackRequest request;
		request.set_tracktype(g_trackTypeComboItems[m_trackTypeFilterIndex]);
		request.set_slot(g_slotComboItems[m_slotFilterIndex]);
		request.set_sortby(g_sortByComboItems[m_sortByIndex]);
		auto tracks = m_trackManagementClient->GetTracks(request);

		drawPreviewImage(device, m_selectedTrack);
		drawComboBoxes();
		drawTableHeader();
		drawTableBody(tracks);
		
		std::string selected = m_selectedTrackName;
		auto trackIter = std::find_if(tracks.begin(), tracks.end(), [&selected](const trackmanagement::Track& obj) {return obj.name() == selected; });
		if (trackIter != tracks.end())
		{
			m_selectedTrack = *trackIter;
			if (m_previouslySelectedTrackName != m_selectedTrackName)
			{
				m_loadNewImage = true;
			}
		}

		drawActionButtons();

	}
	ImGui::End();

	m_previouslySelectedTrackName = m_selectedTrackName;
}

void TrackSelection::drawPreviewImage(LPDIRECT3DDEVICE9 device, const trackmanagement::Track& selected)
{
	if (m_loadNewImage)
	{
		if (m_previewImage != nullptr)
		{
			m_previewImage->Release();
			m_previewImage = nullptr;
		}
		m_previewImage = createTextureFromFile(device, selected.image().c_str());
		m_loadNewImage = false;
	}

	ImVec2 windowSize = ImGui::GetWindowSize();
	static int imagePreviewWindowWidth = 656;
	static int imagePreviewWindowHeight = 376;

	ImGui::SetCursorPosX((windowSize.x / 2) - (imagePreviewWindowWidth / 2));
	ImGui::BeginChild("preview image", ImVec2(imagePreviewWindowWidth, imagePreviewWindowHeight), true);
	if (m_previewImage != nullptr)
	{
		ImGui::Image(m_previewImage, ImVec2(m_previeImageWidth, m_previewImageHeight));
	}
	ImGui::EndChild();

}

void TrackSelection::drawComboBoxes()
{
	ImVec2 windowSize = ImGui::GetWindowSize();
	static int filtersWidth = 656;
	static float filtersHeight = 75;
	ImGui::SetCursorPosX((windowSize.x / 2) - (filtersWidth / 2));
	ImGui::BeginChild("filters", ImVec2(filtersWidth, filtersHeight));

	ImGui::Combo("Track Type Filter", &m_trackTypeFilterIndex, g_trackTypeComboItems, IM_ARRAYSIZE(g_trackTypeComboItems));

	ImGui::Combo("Slot Filter", &m_slotFilterIndex, g_slotComboItems, IM_ARRAYSIZE(g_slotComboItems));

	ImGui::Combo("Sort By", &m_sortByIndex, g_sortByComboItems, IM_ARRAYSIZE(g_sortByComboItems));
	ImGui::EndChild();
}

void TrackSelection::drawTableHeader()
{
	static float headerHeight = 30;
	ImGui::BeginChild("header", ImVec2(0, headerHeight), true);
	
	ImGui::Columns(7, "tracksHeader");
	setTableColumnWidth();

	ImGui::Text("Name"); ImGui::NextColumn();
	ImGui::Text("Slot"); ImGui::NextColumn();
	ImGui::Text("Type"); ImGui::NextColumn();
	ImGui::Text("Author"); ImGui::NextColumn();
	ImGui::Text("Date Created"); ImGui::NextColumn();
	ImGui::Text("Downloads"); ImGui::NextColumn();
	ImGui::Text("Favorite"); 

	ImGui::EndChild();
}

void TrackSelection::drawTableBody(const std::vector<trackmanagement::Track>& tracks)
{
	static const float height = 30.0f;
	ImGui::BeginChild("body", ImVec2(0, ImGui::GetFontSize() * height), true);
	ImGui::Columns(7, "availabletracks");

	setTableColumnWidth();

	for (auto& track : tracks)
	{
		if (ImGui::Selectable(track.name().c_str(), m_selectedTrackName == track.name(), ImGuiSelectableFlags_SpanAllColumns))
		{
			m_selectedTrackName = track.name();
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
}

void TrackSelection::drawActionButtons()
{
	ImVec2 windowSize = ImGui::GetWindowSize();

	//Hardcoding position because window is fixed size and i'm tired
	static float offsetX = 12;
	static float offsetY = 28.0f;

	ImGui::SetCursorPosX(offsetX);
	ImGui::SetCursorPosY(windowSize.y - offsetY);

	ImGui::BeginChild("actions");
	ImGui::Button("Install Random National Tracks");
	ImGui::SameLine();
	ImGui::Button("Install Random Supercross Tracks");
	ImGui::SameLine();
	ImGui::Button("Install Random FreeRide Tracks");
	ImGui::SameLine();
	ImGui::Button("Add to Favorites");
	ImGui::SameLine();
	ImGui::Button("Install Selected");
	ImGui::EndChild();
}

void TrackSelection::setTableColumnWidth()
{
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
}

LPDIRECT3DTEXTURE9 TrackSelection::createTextureFromFile(LPDIRECT3DDEVICE9 device, const char* fileName)
{
	LPDIRECT3DTEXTURE9 d3dTexture = nullptr;
	if (INVALID_FILE_ATTRIBUTES == GetFileAttributes(fileName) && GetLastError() == ERROR_FILE_NOT_FOUND)
	{
		//File not found load default
	}
	else
	{
		auto jpegFile = fopen(fileName, "rb");
		unsigned char* jpegBuf = nullptr;
		long size = 0;
		int inSubsamp, inColorspace;
		unsigned long jpegSize;

		if (jpegFile == nullptr)
			return nullptr; //GNARLY_TODO: log error opening input file

		if (fseek(jpegFile, 0, SEEK_END) < 0 || ((size = ftell(jpegFile)) < 0) || fseek(jpegFile, 0, SEEK_SET) < 0)
			return nullptr; //GNARLY_TODO: log error determining input file size

		if (size == 0)
			return nullptr; //GNARLY_TODO: log error Input file contains no data

		jpegSize = static_cast<unsigned long>(size);

		if ((jpegBuf = (unsigned char *)tjAlloc(jpegSize)) == NULL)
			return nullptr; //GNARLY_TODO: log error allocating JPEG buffer

		if (fread(jpegBuf, jpegSize, 1, jpegFile) < 1)
		{
			tjFree(jpegBuf);
			return nullptr; //GNARLY_TODO: log error reading input file
		}

		fclose(jpegFile);
		jpegFile = nullptr;

		tjhandle tjInstance = tjInitDecompress();
		if (tjInstance == nullptr)
		{
			tjFree(jpegBuf);
			return nullptr; //GNARLY_TODO: log error initializing decompressor"
		}

		if (tjDecompressHeader3(tjInstance, jpegBuf, jpegSize, &m_previeImageWidth, &m_previewImageHeight, &inSubsamp, &inColorspace) < 0)
		{
			tjFree(jpegBuf);
			return nullptr; //GNARLY_TODO: log error reading JPEG header"
		}

		int pixelFormat = TJPF_BGRX;
		int pixelSize = tjPixelSize[pixelFormat];

		unsigned char* imgBuf = (unsigned char *)tjAlloc(m_previeImageWidth * m_previewImageHeight * pixelSize);
		if (imgBuf == nullptr)
		{
			tjFree(jpegBuf);
			return nullptr; //GNARLY_TODO: log error allocating uncompressed image buffer
		}

		if (tjDecompress2(tjInstance, jpegBuf, jpegSize, imgBuf, m_previeImageWidth, 0, m_previewImageHeight, pixelFormat, 0) < 0)
		{
			tjFree(jpegBuf);
			tjFree(imgBuf);
			return nullptr; //GNARLY_TODO: log error decompressing JPEG image
		}

		tjFree(jpegBuf);
		jpegBuf = nullptr;
		tjDestroy(tjInstance);
		tjInstance = nullptr;

		if (device->CreateTexture(m_previeImageWidth, m_previewImageHeight, 1, D3DUSAGE_DYNAMIC, D3DFMT_A8R8G8B8, D3DPOOL_DEFAULT, &d3dTexture, NULL) == D3D_OK)
		{
			D3DLOCKED_RECT destRect;
			if (d3dTexture->LockRect(0, &destRect, NULL, 0) == D3D_OK)
			{
				memcpy(destRect.pBits, imgBuf, m_previeImageWidth * m_previewImageHeight * pixelSize);
				d3dTexture->UnlockRect(0);
			}
		}

		tjFree(imgBuf);
	}
	return d3dTexture;
}