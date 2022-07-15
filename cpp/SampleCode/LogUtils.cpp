#include "LogUtils.h"
using namespace std;

void LogUtils::fswrite()
{
	ofstream ofs("ufx_log", ios_base::trunc);

	if (ofs.is_open())
	{
		ofs << CurrentDateTime() << endl;
	}

	ofs.close();
}
