#pragma once
#include <string.h>
#include <iostream>
using namespace std;

class CheckCheck
{

public:
	int nFunc;

	CheckCheck(int nFunc) {
		this->nFunc = nFunc;
	}

	void Test() {
		cout << "nfunc: " << nFunc << endl;
	}

	static void isStringEmptyCheck() {
		CheckCheck::isStringEmpty(nullptr);
		CheckCheck::isStringEmpty("");
		CheckCheck::isStringEmpty("1");
	}

private:
	static void isStringEmpty(const char* cc) {
		if (!cc || strlen(cc) < 1) {
			cout << "is empty string" << endl;
		}
		else {
			cout << "is not empty string: " << cc << endl;
		}

		if (!cc || *cc == '\0') {
			cout << "is empty string2" << endl;
		}
		else {
			cout << "is not empty string2: " << cc << endl;
		}
	}
};

