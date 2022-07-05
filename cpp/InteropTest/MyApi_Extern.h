#pragma once
#include "pch.h"
#include "MyApi.h"

using namespace std;

#ifdef __cplusplus
extern "C" {
#endif

	FTDC2C_API void* MYDECL CreateAPI() {
		MyApi* api = new MyApi();
		return api;
	}

	FTDC2C_API void MYDECL RegisterEventCallBack(void* pAPI, TapQuoteAPIEvent onEvent) {
		MyApi* api = static_cast<MyApi*>(pAPI);
		api->RegisterEventCallBack(onEvent);
	}

	FTDC2C_API void MYDECL API_Test1(void* pAPI) {
		MyApi* api = static_cast<MyApi*>(pAPI);
		api->Test1();
	}

	FTDC2C_API void MYDECL API_Test2(void* pAPI) {
		MyApi* api = static_cast<MyApi*>(pAPI);
		TapAPIPositionProfitNotice* info = new TapAPIPositionProfitNotice();
		info->IsLast = APIYNFLAG_YES;
		info->Data = new TapAPIPositionProfit();
		info->Data->CalculatePrice = 100.11;
		info->Data->FloatingPL = 100.12;
		info->Data->LMEPositionProfit = 100.13;
		info->Data->OptionMarketValue = 100.14;
		info->Data->PositionNo = "very good";
		//strcpy_s(info->Data->PositionNo, "very good");
		info->Data->PositionProfit = 100.15;
		info->Data->PositionStreamId = 100;
		api->Test2(info);
	}

	FTDC2C_API void MYDECL API_Test3(void* pAPI) {
		MyApi* api = static_cast<MyApi*>(pAPI);
		api->Test3();
	}

	FTDC2C_API TapAPIPositionProfitNoticeM* MYDECL TestTapAPIPositionProfitNotice() {

		TapAPIPositionProfitNoticeM* info = new TapAPIPositionProfitNoticeM();

		info->CalculatePrice = 100.10;
		info->FloatingPL = 100.11;
		info->LMEPositionProfit = 100.12;
		info->OptionMarketValue = 100.13;
		info->PositionProfit = 100.14;
		info->PositionStreamId = 100;

		strcpy_s(info->PositionNo, "very good");

		info->IsLast = APIYNFLAG_YES;

		return info;
	};

#ifdef __cplusplus
}
#endif


