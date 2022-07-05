#pragma once
#include <string>
#include <iostream>
#include <sstream>
using namespace std;

//from: https://www.cnblogs.com/kean0048/p/11060559.html

class CharStringConvert
{
private:

	// string转char*,把string转换为char* 有3种方法：data(); c_str(); copy();
	static void StringToCharPtr() {

		//1) 调用string的data()函数
		string str1 = "hello";
		const char* p = str1.data();//加const或者用char *p=(char*)str1.data();
		cout << "string to char*,use string.data: " << str1 << endl;

		//2) 调用string的c_str()函数
		string str2 = "world";
		const char* p2 = str2.c_str();//同上，要加const或者等号右边用char*

		//一定要使用strcpy()函数等来操作方法c_str()返回的指针 
		//比如：最好不要这样: 
		//char* c;
		//string s = "1234";
		//c = s.c_str(); //c最后指向的内容是垃圾，因为s对象被析构，其内容被处理
		//应该这样用： 
		char c[20];
		string str21 = "1234";
		strcpy_s(c, str21.c_str());

		//再举个例子
		//c_str() 以 char* 形式传回 string 内含字符串
		//如果一个函数要求char*参数，可以使用c_str()方法： 
		string str22 = "Hello World!";
		printf("string to char*,use c_str: %s \n", str22.c_str()); //输出 "Hello World!"

		//3) 调用string的copy()函数
		size_t length;
		char buffer[8];
		string str3("Test string......");

		length = str3.copy(buffer, 7, 5);//这里7代表复制几个字符，5代表复制的位置，
		buffer[length] = '\0'; //注意手动加结束符！！！
		cout << "string to char*,use string.copy(buffer,7,5): " << buffer << endl;

		//更多例子

		//string转const char*
		std::string str4 = "HelloWorld!"; //初始化string类型，并具体赋值
		const char* constc4 = nullptr;//初始化const char*类型，并赋值为空
		constc4 = str4.c_str();             //string类型转const char*类型
		printf_s("string to const char*: %s\n", str4.c_str());   //打印string类型数据 .c_str()
		printf_s("string to const char*: %s\n", constc4);        //打印const char*类型数据

		//string转char*
		std::string str5 = "HelloWorld!"; //初始化string类型，并具体赋值
		char* c5 = nullptr;      //初始化char*类型，并赋值为空
		const char* constc5 = nullptr;//初始化const char*类型，并赋值为空
		constc5 = str5.c_str();             //string类型转const char*类型
		c5 = const_cast<char*>(constc5);    //const char*类型转char*类型
		printf_s("string to char*: %s\n", str5.c_str());   //打印string类型数据 .c_str()
		printf_s("string to char*: %s\n", c);              //打印char*类型数据
	}

	//char*转string,可以直接赋值
	static void CharPtrToString() {
		string s;
		const char* p = "hello";
		s = p;
		cout << "char* to string:" << s << endl;
	}

	//string转char[]
	static void StringToCharArray() {

		//不可以直接赋值，可以循环char* 字符串逐个字符赋值, 也可以使用strcpy_s等函数

		string pp = "HelloWorld";
		char p[100];
		unsigned int i;
		for (i = 0;i < pp.length();i++)
			p[i] = pp[i];
		p[i] = '\0';
		printf("string to char[]:%s\n", p);
	}

	//char[] 转string,可以直接赋值
	static void CharArrayToString() {
		string s;
		char ch[] = "hello world";
		s = ch;
		cout << "char[] to string:" << s << endl;
	}

	//char[]转char*,可以直接赋值
	static void CharArrayToCharPt() {
		char arrc[20] = "HelloWorld!";  //初始化char[] 类型，并具体赋值
		std::string str;                //初始化string
		const char* constc = nullptr;   //初始化const char*
		char* c = nullptr;               //初始化char*

		str = arrc;                      //char[]类型转string类型
		constc = arrc;                   //char[]类型转const char* 类型
		c = arrc;                        //char[]类型转char*类型

		printf_s("char[]转char*: %s\n", arrc);         //打印char[]类型数据
		printf_s("char[]转char*: %s\n", str.c_str());  //打印string类型数据
		printf_s("char[]转char*: %s\n", constc);       //打印const char* 类型数据
		printf_s("char[]转char*: %s\n", c);            //打印char*类型数据
	}

	//char*转char[]
	static void CharPtrToCharArray() {
		//不能直接赋值，可以循环char*字符串逐个字符赋值,也可以使用strcpy_s等函数。

		const char* c = "HelloWorld!";  //初始化char* 类型，并具体赋值
		char arrc[20] = { 0 };    //初始化char[] 类型，并具体赋值
		strncpy_s(arrc, c, 20);     //char*类型转char[] 类型
		printf_s("char*转char: %s\n", c);      //打印char* 类型数据
		printf_s("char*转char: %s\n", arrc);   //打印char[]类型数据
	}

	//char转string
	static void CharToString() {
		//1. 使用string()构造函数方法
		char c = 'F';
		string s = string(1, c);
		cout << "char to string:" << s << endl;

		//2. 使用stringstream字符流
		char c1 = 'F';
		stringstream ss;
		ss << c1;
		string s2;
		ss >> s2;
		cout << "char to string:" << s2 << endl;

		//3. 使用springf()函数
		char c2 = 'F';
		char s3[10];
		sprintf_s(s3, "%c", c2);
		string s4 = string(s3);
		cout << "char to string:" << s4 << endl;
	}

public:
	static void Sample() {

		//string to char*
		StringToCharPtr();

		//char* to string
		CharPtrToString();

		//string to char[]
		StringToCharArray();

		//char[] to string
		CharArrayToString();

		//char[] to char*
		CharArrayToCharPt();

		//char* to char[]
		CharPtrToCharArray();

		//char to string
		CharToString();
	}

	static void Sample2() {
		size_t length;
		char buffer[8];
		string str("Test string......");
		cout << "str:" << str << endl;

		length = str.copy(buffer, 7, 5);
		buffer[length] = '\0';
		cout << "str.copy(buffer,7,5),buffer contains: " << buffer << endl;

		length = str.copy(buffer, str.size(), 5);
		buffer[length] = '\0';
		cout << "str.copy(buffer,str.size(),5),buffer contains:" << buffer << endl;
		length = str.copy(buffer, 7, 0);
		buffer[length] = '\0';
		cout << "str.copy(buffer,7,0),buffer contains:" << buffer << endl;
		length = str.copy(buffer, 7); //缺省参数pos，默认pos=0
		buffer[length] = '\0';
		cout << "str.copy(buffer,7),buffer contains:" << buffer << endl;

		length = str.copy(buffer, string::npos, 5);    //copy函数第2个参数，除了可以是长度，也可以是一个位置，string::npos表示字符串的结束位置
		buffer[length] = '\0';
		cout << "string::npos:" << (int)(string::npos) << endl;
		cout << "buffer[string::npos]:" << buffer[string::npos] << endl;
		cout << "buffer[length-1]:" << buffer[length - 1] << endl;
		cout << "str.copy(buffer,string::npos,5),buffer contains:" << buffer << endl;

		length = str.copy(buffer, string::npos);
		buffer[length] = '\0';
		cout << "str.copy(buffer,string::npos),buffer contains:" << buffer << endl;
		cout << "buffer[string::npos]:" << buffer[string::npos] << endl;
		cout << "buffer[length-1]:" << buffer[length - 1] << endl;
	}
};

