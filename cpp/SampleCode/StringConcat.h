#pragma once
#include <iostream>
#include <string>
#include <cstdlib>
#include <sstream>
#include <cstring> 
#include <stdio.h>
using namespace std;

class StringConcat
{
public:

	void test() {
		test1();
		test2();
		test3();
		test4();
		test5();
	};

private:
	//第一种C风格的转化(以前一直喜欢的  sprintf 功能强大)
	void test1()
	{
		const char* s = "dong";
		int a = 52;
		float b = .1314;
		char* buf = new char[strlen(s) + sizeof(a) + 100];
		sprintf_s(buf, strlen(s) + sizeof(a) + 100, "%s%d%.4f", s, a, b);
		printf("test1: %s\n", buf);
	};

	//半C半C++风格
	void test2()
	{
		string s = "dong";
		int a = 520;
		char* buf = new char[10];//2147483647 int最大值
		_itoa_s(a, buf, 10, 10);      //itoa虽然可以转化为各种进制，但是注意不能是float或者double
		cout << "test2: " << s + buf << " | ";
		_itoa_s(a, buf, 10, 16);
		cout << "test2: " << s + buf << endl;
	};

	//纯C++风格
	void test3()
	{
		string s = "陈明东";
		int a = 52;
		double b = .1314;
		ostringstream oss;
		oss << s << a << b;
		cout << "test3: " << oss.str() << endl;
	};

	//C++11新特性
	void test4()
	{
		int a = 520;
		float b = 5.20;
		string str = "dong";
		string res = str + to_string(a);
		cout << "test4: " << res << endl;
		res = str + to_string(b);
		res.resize(8);
		cout << "test4: " << res << endl;
	};

	void test5() {
		int nfun = 10001;
		string str = "dong " + to_string(nfun) + " ni hao ma";
		cout << "test5: " << str << endl;

		const char* cc = str.c_str();

		cout << "test5: " << cc << endl;
	};
};

