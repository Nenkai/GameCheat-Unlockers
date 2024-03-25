
#include "Utils/MemoryMgr.h"
#include "Utils/Patterns.h"
#include "Utils/ScopedUnprotect.hpp"

#include <cstdint>

LSTATUS WINAPI RegCreateKeyEx_LicenseKey(HKEY /*hKey*/, LPCSTR /*lpSubKey*/, DWORD /*Reserved*/, LPSTR /*lpClass*/, DWORD /*dwOptions*/, REGSAM /*samDesired*/,
				const LPSECURITY_ATTRIBUTES /*lpSecurityAttributes*/, PHKEY phkResult, LPDWORD /*lpdwDisposition*/)
{
	return RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", 0, KEY_QUERY_VALUE|KEY_WOW64_64KEY, phkResult);
}
static auto* const pRegCreateKeyEx_LicenseKey = &RegCreateKeyEx_LicenseKey;

void OnInitializeHook()
{
	static_assert(std::string_view(__FUNCSIG__).find("__stdcall") != std::string_view::npos, "This codebase must default to __stdcall, please change your compilation settings.");

	auto Protect = ScopedUnprotect::UnprotectSectionOrFullModule( GetModuleHandle( nullptr ), ".text" );

	using namespace Memory;
	using namespace hook::txn;

	// Allow plaintext cheats for every cheat code
	try
	{
		auto cheat_encrypted_only = get_pattern("33 C0 83 F9 0C 73 14", 2);
		Patch<uint32_t>(cheat_encrypted_only, 0xC3); // put retn after xor eax, eax
	}
	TXN_CATCH();
	
	// Query for a correct key from a 64-bit registry view
	try
	{
		void* regOpenKey[] = {
			get_pattern("83 CE FF FF 15 ? ? ? ? 85 C0", 3 + 2),
			get_pattern("89 7C 24 40 FF 15 ? ? ? ? 85 C0", 4 + 2)
		};

		for (void* addr : regOpenKey)
		{
			Patch(addr, &pRegCreateKeyEx_LicenseKey);
		}
	}
	TXN_CATCH();
}
