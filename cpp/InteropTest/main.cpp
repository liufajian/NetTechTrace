// InteropTest.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include "Interop.h"
#include <string>
#include "MyApi_Extern.h"

using namespace std;

int main()
{
	std::cout << "Hello World!\n";

	struct s2
	{
		char q1;
		char q2;
		int i;
	};

	printf("%llu\n", sizeof(struct s2));

	TapAPIPositionProfitNoticeM* ttt = TestTapAPIPositionProfitNotice();

	string str = string(1, ttt->IsLast);

	printf("isLast:%s,PositionNo:%s,PositionStreamId:%u,PositionProfit:%f,FloatingPL:%f \n", str.c_str(), ttt->PositionNo, ttt->PositionStreamId, ttt->PositionProfit, ttt->FloatingPL);

	system("pause");
}
