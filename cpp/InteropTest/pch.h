#pragma once 

//#pragma pack(push, 1)

#ifdef _WIN32
// Windows
#define FTDC2C_API extern __declspec(dllexport)
#define MYDECL	__stdcall
#else
// Linux
#define FTDC2C_API extern
#define MYDECL  __attribute__((stdcall))
#endif

#ifdef _WIN32
#define TAP_CDECL  __cdecl
#define TAP_DLLEXPORT __declspec(dllexport)
#else
#define TAP_CDECL
#define TAP_DLLEXPORT
#endif

//! 单字符定义，可用于定义其他类型
typedef char	TAPICHAR;

//! 长度为70的字符串
typedef char	TAPISTR_70[71];

//=============================================================================
/**
 *	\addtogroup G_DATATYPE_NUMBER	基本数值类型定义
 *	@{
 */
 //=============================================================================

//! int 32
typedef int					TAPIINT32;
//! unsigned 32
typedef unsigned int		TAPIUINT32;
//! int 64
typedef long long			TAPIINT64;
//! unsigned 64
typedef unsigned long long	TAPIUINT64;
//! unsigned 16
typedef unsigned short		TAPIUINT16;
//! unsigned 8
typedef unsigned char		TAPIUINT8;
//! real 64
typedef double				TAPIREAL64;

//=============================================================================
/**
 *	\addtogroup G_DATATYPE_YNFLAG	是否标示
 *	@{
 */
 //=============================================================================
 //! 是否标示
typedef TAPICHAR			TAPIYNFLAG;
//! 是
const TAPIYNFLAG			APIYNFLAG_YES = 'Y';
//! 否
const TAPIYNFLAG			APIYNFLAG_NO = 'N';

//函数指针定义

enum class EventType
{
	Test1 = 1,
	Test2 = 2,
	Test3 = 3,
};

#ifdef __cplusplus
extern "C" {
#endif
	typedef void(MYDECL* TapQuoteAPIEvent)(EventType type, TAPIINT32 errorCode, TAPIYNFLAG isLast, const void* info);
#ifdef __cplusplus
}
#endif

//! 客户持仓盈亏
struct TapAPIPositionProfit
{
	const char* PositionNo;						///< 本地持仓号，服务器编写
	TAPIUINT32					PositionStreamId;				///< 持仓流号
	TAPIREAL64					PositionProfit;					///< 持仓盈亏
	TAPIREAL64					LMEPositionProfit;				///< LME持仓盈亏
	TAPIREAL64					OptionMarketValue;				///< 期权市值
	TAPIREAL64					CalculatePrice;					///< 计算价格
	TAPIREAL64					FloatingPL;						///< 逐笔浮盈

	explicit inline TapAPIPositionProfit()
		: PositionNo("")
		, PositionStreamId(1)
		, LMEPositionProfit(0)
		, OptionMarketValue(0)
		, CalculatePrice(0)
		, FloatingPL(0) {}
};

//! 客户持仓盈亏通知
struct TapAPIPositionProfitNotice
{
	TAPIYNFLAG			IsLast;					///< 是否最后一包
	TapAPIPositionProfit* Data;					///< 客户持仓盈亏信息
};

//! 客户持仓盈亏通知M
struct TapAPIPositionProfitNoticeM
{
	TAPIYNFLAG	IsLast;							///< 是否最后一包
	TAPISTR_70	PositionNo;						///< 本地持仓号，服务器编写
	TAPIUINT32	PositionStreamId;				///< 持仓流号
	TAPIREAL64	PositionProfit;					///< 持仓盈亏
	TAPIREAL64	LMEPositionProfit;				///< LME持仓盈亏
	TAPIREAL64	OptionMarketValue;				///< 期权市值
	TAPIREAL64	CalculatePrice;					///< 计算价格
	TAPIREAL64	FloatingPL;						///< 逐笔浮盈
};

struct TempStruct
{
	char* str;
	int num;

	// Note the strdup. You don't know the source of str.
	// For example if the source is "Foo", then you can't free it.
	// Using strdup solves this problem.
	inline explicit TempStruct(const char* str, int num)
		: str(_strdup(str)), num(num)
	{
	}

	~TempStruct()
	{
		free(str);
	}
};
