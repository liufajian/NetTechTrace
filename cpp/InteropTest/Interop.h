#pragma once
#include "pch.h"
#include <string.h>

using namespace std;

#ifdef __cplusplus
extern "C" {
#endif

	FTDC2C_API void MYDECL GetVersion(char* buf, int n) {
		const char* version = "Hello World";
		strncpy_s(buf, strlen(version) + 1, version, n);
	}

	__declspec(dllexport) void GetArrResult(TempStruct*** outPtr, int* size)
	{
		*outPtr = new TempStruct * [2];

		(*outPtr)[0] = new TempStruct("sdf", 123);
		(*outPtr)[1] = new TempStruct("abc", 456);

		*size = 2;
	}

	__declspec(dllexport) void FreeArrSomeData(TempStruct** ptr, int size)
	{
		for (int i = 0; i < size; i++)
		{
			delete ptr[i];
		}

		delete[] ptr;
	}

#ifdef __cplusplus
}
#endif


