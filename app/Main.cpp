#include "Injector.h"

#include <iostream>
#include <string>
#include <vector>

#include "BuildDefines.h"

std::string GetCurrentDirectory()
{
	const int MaxDirectoryLength = 512;
	char filePath[MaxDirectoryLength]{ 0 };	
	GetModuleFileNameA(NULL, filePath, MaxDirectoryLength);

	std::string currentDirectory(filePath);
	for (int i = currentDirectory.size(); i > 0; --i)
	{
		if (currentDirectory[i] == '\\')
		{
			currentDirectory = currentDirectory.substr(0, i + 1);
			break;
		}
	}

	return currentDirectory;
}

void main(int argc, char *argv[])
{
	try
	{
#if !defined(GNARLY_DEBUG)
		//The user didn't pass any custom arguments so we load the dlls sitting next to the executeable
		std::string currentDirectory = GetCurrentDirectory();
		std::vector<std::string> modules{ currentDirectory + "Indicium-Supra.dll", currentDirectory + "TrackManager.dll" };
#else
		//Load our debug dll's from the folder where they are being compiled
		std::string buildFolder(CMAKE_BINARY_DIR);
		std::vector<std::string> modules
		{
			buildFolder + "/Indicium-Supra/Debug/x86/Indicium-Supra.dll"
			, buildFolder + "/trackmanager/Debug/cares.dll"
			, buildFolder + "/trackmanager/Debug/libprotobufd.dll"
			, buildFolder + "/trackmanager/Debug/zlibd1.dll"
			, buildFolder + "/trackmanager/Debug/TrackManager.dll"
		};
#endif

		// Get privileges required to perform the injection
		Injector::Get()->GetSeDebugPrivilege();

		DWORD ProcID = Injector::Get()->GetProcessIdByName("MXReflex.exe");

		for (auto& module : modules)
		{
			// Inject module
			Injector::Get()->InjectLib(ProcID, module);
			// If we get to this point then no exceptions have been thrown so we
			// assume success.
			std::cout << "Successfully injected module!" << std::endl;
		}
	}
	// Catch STL-based exceptions.
	catch (const std::exception& e)
	{
		std::string TempError(e.what());
		std::string Error(TempError.begin(), TempError.end());
		std::cerr << "General Error:" << std::endl
			<< Error << std::endl;
	}
	catch (...)
	{
		std::cerr << "Unknown error!" << std::endl;
	}
}
