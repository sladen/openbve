#ifdef __cplusplus
extern "C" {
#endif
#ifdef __WIN32__
#include "stdafx.h"
#else
#include <stdio.h>
#include <stdlib.h>
#include <stddef.h> // wchar
#include <dlfcn.h>
#include <unistd.h> // get_current_dir_name()
#include <string.h> // strdup()
#define _stdcall
#define __stdcall
#define FARPROC void *
#define LPCWSTR wchar_t *
#define LPCSTR char *

// dlopen() equivalency
#define HMODULE void *
#define LoadLibraryW(x) NULL
#define LoadLibraryA(pathname) dlopen(pathname,RTLD_LAZY)
#define GetProcAddress(handle,function) dlsym(handle,function)
#define FreeLibrary(handle) dlclose(handle)
#define __declspec(x)
#endif

#ifdef __WIN32__
// --- main ---
BOOL APIENTRY DllMain(HANDLE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    return TRUE;
}
#endif

// --- structures ---
struct ATS_VEHICLESPEC {
	int BrakeNotches;
	int PowerNotches;
	int AtsNotch;
	int B67Notch;
	int Cars;
};
struct ATS_VEHICLESTATE {
	double Location;
	float Speed;
	int Time;
	float BcPressure;
	float MrPressure;
	float ErPressure;
	float BpPressure;
	float SapPressure;
	float Current;
};
struct ATS_BEACONDATA {
	int Type;
	int Signal;
	float Distance;
	int Optional;
};
struct ATS_HANDLES {
	int Brake;
	int Power;
	int Reverser;
	int ConstantSpeed;
};

#define ATS_API __declspec(dllimport)

// --- handles ---
HMODULE dllhandle = NULL;
typedef ATS_API void (__stdcall *LOAD) (); static LOAD load = NULL;
typedef ATS_API void (__stdcall *DISPOSE) (); static DISPOSE dispose = NULL;
typedef ATS_API int (__stdcall *GETPLUGINVERSION) (); static GETPLUGINVERSION getpluginversion = NULL;
typedef ATS_API void (__stdcall *SETVEHICLESPEC) (ATS_VEHICLESPEC vehicleSpec); static SETVEHICLESPEC setvehiclespec = NULL;
typedef ATS_API void (__stdcall *INITIALIZE) (int brake); static INITIALIZE initialize = NULL;
typedef ATS_API ATS_HANDLES (__stdcall *ELAPSE) (ATS_VEHICLESTATE vehicleState, int* panel, int* sound); static ELAPSE elapse = NULL;
typedef ATS_API void (__stdcall *SETPOWER) (int setpower); static SETPOWER setpower = NULL;
typedef ATS_API void (__stdcall *SETBRAKE) (int setbrake); static SETBRAKE setbrake = NULL;
typedef ATS_API void (__stdcall *SETREVERSER) (int setreverser); static SETREVERSER setreverser = NULL;
typedef ATS_API void (__stdcall *KEYDOWN) (int atsKeyCode); static KEYDOWN keydown = NULL;
typedef ATS_API void (__stdcall *KEYUP) (int atsKeyCode); static KEYUP keyup = NULL;
typedef ATS_API void (__stdcall *HORNBLOW) (int hornType); static HORNBLOW hornblow = NULL;
typedef ATS_API void (__stdcall *DOOROPEN) (); static DOOROPEN dooropen = NULL;
typedef ATS_API void (__stdcall *DOORCLOSE) (); static DOORCLOSE doorclose = NULL;
typedef ATS_API void (__stdcall *SETSIGNAL) (int signal); static SETSIGNAL setsignal = NULL;
typedef ATS_API void (__stdcall *SETBEACONDATA) (ATS_BEACONDATA beaconData); static SETBEACONDATA setbeacondata = NULL;

static char *plugin_path;

// --- load the plugin ---
int _stdcall LoadDLL(LPCWSTR fileUnicode, LPCSTR fileAnsi) {
	plugin_path = strdup(fileAnsi);
	fprintf(stderr, "LoadDLL(\"%s\") called\n", fileAnsi);
	dllhandle = LoadLibraryW(fileUnicode);
	if (dllhandle == NULL) {
		dllhandle = LoadLibraryA(fileAnsi);
		fprintf(stderr, "LoadDLL() dllhandle=%p\n", dllhandle);
		if (dllhandle == NULL) return 0;
	}
	{ // --- Load ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "Load");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"Load\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			load = (LOAD)functionhandle;
		}
	}
	{ // --- Dispose ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "Dispose");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"Dispose\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			dispose = (DISPOSE)functionhandle;
		}
	}
	{ // --- GetPluginVersion ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "GetPluginVersion");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"GetPluginVersion\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			getpluginversion = (GETPLUGINVERSION)functionhandle;
		}
	}
		{ // --- SetVehicleSpec ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetVehicleSpec");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"SetVehicleSpec\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			setvehiclespec = (SETVEHICLESPEC)functionhandle;
		}
	}
	{ // --- Initialize ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "Initialize");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"Initialize\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			initialize = (INITIALIZE)functionhandle;
		}
	}
	{ // --- Elapse ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "Elapse");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"Elapse\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			elapse = (ELAPSE)functionhandle;
		}
	}
	{ // --- SetPower ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetPower");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"SetPower\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			setpower = (SETPOWER)functionhandle;
		}
	}
	{ // --- SetBrake ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetBrake");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"SetBrake\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			setbrake = (SETBRAKE)functionhandle;
		}
	}
	{ // --- SetReverser ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetReverser");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"SetReverser\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			setreverser = (SETREVERSER)functionhandle;
		}
	}
	{ // --- KeyDown ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "KeyDown");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"KeyDown\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			keydown = (KEYDOWN)functionhandle;
		}
	}
	{ // --- KeyUp ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "KeyUp");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"KeyUp\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			keyup = (KEYUP)functionhandle;
		}
	}
	{ // --- HornBlow ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "HornBlow");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"HornBlow\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			hornblow = (HORNBLOW)functionhandle;
		}
	}
	{ // --- DoorOpen ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "DoorOpen");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"DoorOpen\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			dooropen = (DOOROPEN)functionhandle;
		}
	}
	{ // --- DoorClose ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "DoorClose");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"DoorClose\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			doorclose = (DOORCLOSE)functionhandle;
		}
	}
	{ // --- SetSignal ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetSignal");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"SetSignal\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			setsignal = (SETSIGNAL)functionhandle;
		}
	}
	{ // --- SetBeaconData ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetBeaconData");
		fprintf(stderr, "LoadDLL() GetProcAddress(\"SetBeaconData\")=%p\n", functionhandle);
		if (functionhandle != NULL) {
			setbeacondata = (SETBEACONDATA)functionhandle;
		}
	}
	return 1;
}

// --- unload the plugin ---
int _stdcall UnloadDLL () {
	if (dllhandle != NULL) {
		load = NULL;
		dispose = NULL;
		getpluginversion = NULL;
		setvehiclespec = NULL;
		initialize = NULL;
		elapse = NULL;
		setpower = NULL;
		setbrake = NULL;
		setreverser = NULL;
		keydown = NULL;
		keyup = NULL;
		hornblow = NULL;
		dooropen = NULL;
		doorclose = NULL;
		setsignal = NULL;
		setbeacondata = NULL;
		return FreeLibrary(dllhandle);
	} else {
		return 1;
	}
}

// --- Load ---
void _stdcall Load () {
	fprintf(stderr, "AtsPluginProxy: Load()\n");
#ifndef __WIN32__
	// Strip the directory and change to it so that the
	// plugin has some hope of finding its config file
	// If it fails, it's no worse than before
	*rindex(plugin_path, '/') = '\0';
	chdir(plugin_path);
#endif
	if (load != NULL) load();
	fprintf(stderr, "AtsPluginProxy: Load() returned\n");
}

// --- Dispose ---
void _stdcall Dispose () {
	fprintf(stderr, "AtsPluginProxy: Dispose()\n");
	if (dispose != NULL) dispose();
}

// --- GetPluginVersion ---
int _stdcall GetPluginVersion () {
	fprintf(stderr, "AtsPluginProxy: GetPluginVersion()\n");
	if (getpluginversion != NULL) {
		fprintf(stderr, "AtsPluginProxy: GetPluginVersion() = %d\n", getpluginversion());
		return getpluginversion();
	} else {
		return 0;
	}
}

// --- SetVehicleSpec ---
void _stdcall SetVehicleSpec (ATS_VEHICLESPEC* vehicleSpec) {
	fprintf(stderr, "AtsPluginProxy: SetVehicleSpec(vehicleSpec=%p)\n", vehicleSpec);
	if (setvehiclespec != NULL) setvehiclespec(*vehicleSpec);
}

// --- Initialize ---
void _stdcall Initialize (int brake) {
	fprintf(stderr, "AtsPluginProxy: Initialize(brake=%d)\n", brake);
	if (initialize != NULL) initialize(brake);
}

// --- Elapse ---
void _stdcall Elapse (ATS_HANDLES* atsHandles, ATS_VEHICLESTATE* vehicleState, int* panel, int* sound) {
	//fprintf(stderr, "AtsPluginProxy: Elapse(atsHandles=%p, vehicleState=%p, panel=%p, sound=%p)\n", atsHandles, vehicleState, panel, sound);
	fprintf(stderr, ".");
	if (elapse != NULL) {
		ATS_HANDLES handles = elapse(*vehicleState, panel, sound);
		atsHandles->Brake = handles.Brake;
		atsHandles->Power = handles.Power;
		atsHandles->Reverser = handles.Reverser;
		atsHandles->ConstantSpeed = handles.ConstantSpeed;
	}
}

// --- SetPower ---
void _stdcall SetPower(int notch) {
	fprintf(stderr, "AtsPluginProxy: SetPower(notch=%d)\n", notch);
	if (setpower != NULL) setpower(notch);
}

// --- SetBrake ---
void _stdcall SetBrake(int notch) {
	fprintf(stderr, "AtsPluginProxy: SetBrake(notch=%d)\n", notch);
	if (setbrake != NULL) setbrake(notch);
}

// --- SetReverser ---
void _stdcall SetReverser(int pos) {
	fprintf(stderr, "AtsPluginProxy: SetReverser(pos=%d)\n", pos);
	if (setreverser != NULL) setreverser(pos);
}

// --- KeyDown ---
void _stdcall KeyDown(int atsKeyCode) {
	fprintf(stderr, "AtsPluginProxy: KeyDown(atsKeyCode=%d)\n", atsKeyCode);
	if (keydown != NULL) keydown(atsKeyCode);
}

// --- KeyUp ---
void _stdcall KeyUp(int atsKeyCode) {
	fprintf(stderr, "AtsPluginProxy: KeyUp(atsKeyCode=%d)\n", atsKeyCode);
	if (keyup != NULL) keyup(atsKeyCode);
}

// --- HornBlow ---
void _stdcall HornBlow(int hornType) {
	fprintf(stderr, "AtsPluginProxy: HornBlow(hornType=%d)\n", hornType);
	if (hornblow != NULL) hornblow(hornType);
}

// --- DoorOpen ---
void _stdcall DoorOpen() {
	fprintf(stderr, "AtsPluginProxy: DoorOpen()\n");
	if (dooropen != NULL) dooropen();
}

// --- DoorClose ---
void _stdcall DoorClose() {
	fprintf(stderr, "AtsPluginProxy: DoorClose()\n");
	if (doorclose != NULL) doorclose();
}

// --- SetSignal ---
void _stdcall SetSignal(int signal) {
	fprintf(stderr, "AtsPluginProxy: SetSignal(signal=%d)\n", signal);
	if (setsignal != NULL) setsignal(signal);
}

// --- SetBeaconData ---
void _stdcall SetBeaconData(ATS_BEACONDATA* beaconData) {
	fprintf(stderr, "AtsPluginProxy: SetBeaconData(beaconData=%p)\n", beaconData);
	if (setbeacondata != NULL) setbeacondata(*beaconData);
}
#ifdef __cplusplus
}
#endif
