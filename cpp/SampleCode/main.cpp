// FreCode.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include "CharStringConvert.h"
#include "PtrTest.h"
#include "StringConcat.h"
#include "CheckCheck.h"

int main()
{
	std::cout << "Hello World!\n";

	CheckCheck cc(100);
	//cc.Test();

	//CheckCheck::isStringEmptyCheck();

	StringConcat sc;
	//sc.test();

	//CharStringConvert::Sample();

	PtrTest pt;
	pt.TestDelegate();
}

void PtrTestTest() {

	PtrTest test;

	UfxError err;
	test.Test1(err);
	cout << "ptr test 1:" << err.errcode << "," << err.errmsg << endl;

	test.Test11(err);
	cout << "ptr test 11:" << err.errcode << "," << err.errmsg << endl;

	//报错:临时变量无效
	//UfxError* err2 = test.Test2();
	//cout << "ptr test 2:" << err2->errcode << "," << err2->errmsg << endl;

	UfxError* err3 = test.Test3();
	cout << "ptr test 3:" << err3->errcode << "," << err3->errmsg << endl;

	UfxError* err4 = test.Test4();
	cout << "ptr test 4:" << err4->errcode << "," << err4->errmsg << endl;

	//报错:临时变量无效
	//UfxError* err5 = test.Test5();
	//cout << "ptr test 5:" << err5->errcode << "," << err5->errmsg << endl;
}