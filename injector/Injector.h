//Credit to Benjamin Höglinger: https://github.com/nefarius/Injector 

#pragma once
// Windows API
#include <Windows.h>

// C++ Standard Library
#include <string>

// Class to manage DLL injection into a remote process
class Injector
{
public:
	// Get singleton
	static Injector* Get();

	// Inject library
	void InjectLib(DWORD ProcID, const std::wstring& Path);
	void InjectLib(DWORD ProcID, const std::string& Path);

	// Eject library
	void EjectLib(DWORD ProcID, const std::wstring& Path);
	void EjectLib(DWORD ProcID, const std::string& Path);

	// Get fully qualified path from module name
	std::string GetPath(const std::string& ModuleName);

	// Get process id by name
	DWORD GetProcessIdByName(const std::string& Name);
	// Get proces id by window
	DWORD GetProcessIdByWindow(const std::string& Name);

	// Get SeDebugPrivilege. Needed to inject properly.
	void GetSeDebugPrivilege();

protected:
	// Enforce singleton
	Injector();
	~Injector();
	Injector(const Injector&);
	Injector& operator= (const Injector&);
private:
	// Singleton
	static Injector* m_pSingleton;
};
