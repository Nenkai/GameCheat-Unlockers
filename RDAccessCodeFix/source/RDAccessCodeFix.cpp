
#include "Utils/MemoryMgr.h"
#include "Utils/Patterns.h"
#include "Utils/ScopedUnprotect.hpp"

#include <cstdint>
#include <filesystem>

#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <Windows.h>
#include <wil/win32_helpers.h>

LSTATUS WINAPI RegCreateKeyEx_LicenseKey(HKEY /*hKey*/, LPCSTR /*lpSubKey*/, DWORD /*Reserved*/, LPSTR /*lpClass*/, DWORD /*dwOptions*/, REGSAM /*samDesired*/,
				const LPSECURITY_ATTRIBUTES /*lpSecurityAttributes*/, PHKEY phkResult, LPDWORD /*lpdwDisposition*/)
{
	return RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", 0, KEY_QUERY_VALUE|KEY_WOW64_64KEY, phkResult);
}
static auto* const pRegCreateKeyEx_LicenseKey = &RegCreateKeyEx_LicenseKey;

static int GenerateReferenceCode_Stub()
{
	return -1;
}

static std::wstring GetPathToIni()
{
	std::wstring result{L".\\" rsc_Name ".ini"};

	wil::unique_cotaskmem_string pathToAsi;
	if (SUCCEEDED(wil::GetModuleFileNameW(wil::GetModuleInstanceHandle(), pathToAsi)))
	{
		try
		{
			result = std::filesystem::path(pathToAsi.get()).replace_extension(L"ini").wstring();
		}
		catch (const std::filesystem::filesystem_error&)
		{
		}
	}
	return result;
}

void OnInitializeHook()
{
	static_assert(std::string_view(__FUNCSIG__).find("__stdcall") != std::string_view::npos, "This codebase must default to __stdcall, please change your compilation settings.");

	auto Protect = ScopedUnprotect::UnprotectSectionOrFullModule( GetModuleHandle( nullptr ), ".text" );

	using namespace Memory;
	using namespace hook::txn;

	const std::wstring pathToIni = GetPathToIni();

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

	
	// Restore rand()-based access code generation, as many people
	// nowadays share the same ProductId after upgrading to Win10/11
	try
	{
		auto generate_reference_code = get_pattern("56 89 84 24 ? ? ? ? 8D 44 24 0C", -11);
		auto dont_reset_access_code = get_pattern("74 13 33 C9 89 0D");

		InjectHook(generate_reference_code, GenerateReferenceCode_Stub, HookType::Jump);
		Patch<uint8_t>(dont_reset_access_code, 0xEB);
	}
	TXN_CATCH();


	// Allow plaintext cheats for every cheat code
	if (GetPrivateProfileIntW(L"RDAccessCodeFix", L"PlainCheats", 0, pathToIni.c_str()) != 0) try
	{
		auto cheat_encrypted_only = get_pattern("33 C0 83 F9 0C 73 14", 2);
		Patch<uint32_t>(cheat_encrypted_only, 0xC3); // put retn after xor eax, eax
	}
	TXN_CATCH();
}
