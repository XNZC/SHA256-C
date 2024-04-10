#pragma once

#ifdef SHA256_EXPORTS
#define SHA256_API __declspec(dllexport)
#else
#define SHA256_API __declspec(dllimport)
#endif


extern "C" SHA256_API void __cdecl ToSHA256(const char* FilePath, char* buf);