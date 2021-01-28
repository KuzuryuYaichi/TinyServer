#pragma once

typedef void (WINAPI *PDATA_CALLBACK)(void* buffer, int packnum);

#define EXPORT extern "C" __declspec(dllexport)

EXPORT int OpenDevice();

EXPORT int CloseDevice();

EXPORT int WriteData_AIS_Interrupt(unsigned char* data, int len);//指令暂时没有

EXPORT int WriteData_ADS_Interrupt(unsigned char* data, int len);//指令暂时没有

EXPORT void RegisterCallBackAIS(PDATA_CALLBACK pfunc, unsigned char* pref);

EXPORT void RegisterCallBackADS(PDATA_CALLBACK pfunc, unsigned char* pref);

EXPORT void StopCallbackFunc();

EXPORT void WriteUserRegister(long addr, unsigned int value);

EXPORT unsigned int ReadUserRegister(long addr);