
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

static int* gAccessCode;
static int BonusRefCodeGet_Patch()
{
	return *gAccessCode;
}

static int (__cdecl *orgPersistentDataDeSerialize)(void*);
static int __cdecl PersistentDataDeSerialize_ReRollAccessCode(void* obj)
{
	const int result = orgPersistentDataDeSerialize(obj);

	if (*gAccessCode == -1)
	{
		*gAccessCode = 1 + (rand() % 9999);
	}

	return result;
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
	
		try
		{
			// RD3
			auto dont_reset_access_code = get_pattern("74 13 33 C9 89 0D");
			Patch<uint8_t>(dont_reset_access_code, 0xEB);
		}
		catch (hook::txn_exception&)
		{
			// RD2
			auto dont_reset_access_code = get_pattern("74 0D 8D 53 F4");

			// RD2 also needs to patch one of the calls to Bonus_GenerateReferenceCode,
			// and re-generate the access code if -1 is deserialized
			// (RD3 does it out of the box)
			auto bonus_uid_from_unlock_code = pattern("83 3D ? ? ? ? FF 56 57 74 57 33 FF 8B FF").get_one();

			uintptr_t player_profile_initialize;
			ReadCall(get_pattern("E8 ? ? ? ? 8B 15 ? ? ? ? 68 ? ? ? ? 52 E8 ? ? ? ? 89 2D ? ? ? ? E8 ? ? ? ? A1 ? ? ? ? 68 ? ? ? ? 50 E8 ? ? ? ? E8"), player_profile_initialize);

			orgPersistentDataDeSerialize = std::exchange(*reinterpret_cast<decltype(orgPersistentDataDeSerialize)*>(player_profile_initialize + 0x16 + 6),
									&PersistentDataDeSerialize_ReRollAccessCode);

			gAccessCode = *bonus_uid_from_unlock_code.get<int*>(2);
			InjectHook(bonus_uid_from_unlock_code.get<void>(0x10), &BonusRefCodeGet_Patch);

			Patch<uint8_t>(dont_reset_access_code, 0xEB);

			srand(static_cast<unsigned int>(time(nullptr)));
		}

		InjectHook(generate_reference_code, GenerateReferenceCode_Stub, HookType::Jump);
	}
	TXN_CATCH();


	// Allow plaintext cheats for every cheat code
	if (GetPrivateProfileIntW(L"RDAccessCodeFix", L"PlainCheats", 0, pathToIni.c_str()) != 0) try
	{
		// RD3
		try
		{
			auto cheat_encrypted_only = get_pattern("33 C0 83 F9 0C 73 14", 2);
			Patch<uint32_t>(cheat_encrypted_only, 0xC3); // put retn after xor eax, eax
		}
		catch (hook::txn_exception&)
		{
			// RD2
			auto cheat_encrypted_only = get_pattern("0F BE 47 FF 85 C0 75 5B", 6);
			Nop(cheat_encrypted_only, 2);
		}
	}
	TXN_CATCH();
}
