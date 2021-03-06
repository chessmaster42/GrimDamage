#pragma once
#include <windows.h>
#include "DataQueue.h"
#include "BaseMethodHook.h"

/************************************************************************
/************************************************************************/
class DetectPlayerId : public BaseMethodHook {
public:
	DetectPlayerId();
	DetectPlayerId(DataQueue* dataQueue, HANDLE hEvent);
	void EnableHook();
	void DisableHook();

	static int GetPlayerId(int* player);

private:
	typedef void* (__thiscall *OriginalMethodPtr)(void*, bool b);
	typedef int (__thiscall *GetObjectIdMethodPtr)(void*);
	static HANDLE m_hEvent;
	static OriginalMethodPtr originalMethod;
	static GetObjectIdMethodPtr GetObjectId;
	static DataQueue* m_dataQueue;
	static bool DetectOffset();

	static int m_offset;

	static void* __fastcall HookedMethod(void* This, void* notUsed, bool b);
	
};