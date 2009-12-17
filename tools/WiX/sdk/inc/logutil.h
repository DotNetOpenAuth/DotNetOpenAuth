#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="logutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//    
//    The use and distribution terms for this software are covered by the
//    Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
//    which can be found in the file CPL.TXT at the root of this distribution.
//    By using this software in any fashion, you are agreeing to be bound by
//    the terms of this license.
//    
//    You must not remove this notice, or any other, from this software.
// </copyright>
// 
// <summary>
//    Header for string helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

#define LogExitOnFailure(x, i, f) if (FAILED(x)) { LogErrorId(x, i, NULL, NULL, NULL); ExitTrace(x, f); goto LExit; }
#define LogExitOnFailure1(x, i, f, s) if (FAILED(x)) { LogErrorId(x, i, s, NULL, NULL); ExitTrace1(x, f, s); goto LExit; }
#define LogExitOnFailure2(x, i, f, s, t) if (FAILED(x)) { LogErrorId(x, i, s, t, NULL); ExitTrace2(x, f, s, t); goto LExit; }
#define LogExitOnFailure3(x, i, f, s, t, u) if (FAILED(x)) { LogErrorId(x, i, s, t, u); ExitTrace3(x, f, s, t, u); goto LExit; }

// enums

// structs

// functions
BOOL DAPI IsLogInitialized();

HRESULT DAPI LogInitialize(
    IN HMODULE hModule,
    IN LPCWSTR wzLog,
    IN LPCWSTR wzExt,
    IN BOOL fAppend,
    IN BOOL fHeader
    );

void DAPI LogUninitialize(
    IN BOOL fFooter
    );

BOOL DAPI LogIsOpen();

REPORT_LEVEL DAPI LogSetLevel(
    IN REPORT_LEVEL rl,
    IN BOOL fLogChange
    );

REPORT_LEVEL DAPI LogGetLevel();

HRESULT DAPI LogGetPath(
    __out_ecount(cchLogPath) LPWSTR pwzLogPath, 
    __in DWORD cchLogPath
    );

HANDLE DAPI LogGetHandle();

HRESULT DAPIV LogString(
    IN REPORT_LEVEL rl,
    IN LPCWSTR wzFormat,
    ...
    );

HRESULT DAPI LogStringArgs(
    IN REPORT_LEVEL rl,
    IN LPCWSTR wzFormat,
    IN va_list args
    );

HRESULT DAPIV LogStringLine(
    IN REPORT_LEVEL rl,
    IN LPCWSTR wzFormat,
    ...
    );

HRESULT DAPI LogStringLineArgs(
    IN REPORT_LEVEL rl,
    IN LPCWSTR wzFormat,
    IN va_list args
    );

HRESULT DAPI LogIdModuleArgs(
    IN REPORT_LEVEL rl,
    IN DWORD dwLogId,
    IN HMODULE hModule,
    va_list args
    );

/* 
 * Wraps LogIdModuleArgs, so inline to save the function call
 */

inline HRESULT LogId(
    IN REPORT_LEVEL rl,
    IN DWORD dwLogId,
    ...
    )
{
    HRESULT hr = S_OK;
    va_list args;
    
    va_start(args, dwLogId);
    hr = LogIdModuleArgs(rl, dwLogId, NULL, args);
    va_end(args);
    
    return hr;
}


/* 
 * Wraps LogIdModuleArgs, so inline to save the function call
 */
 
inline HRESULT LogIdArgs(
    IN REPORT_LEVEL rl,
    IN DWORD dwLogId,
    va_list args
    )
{
    return LogIdModuleArgs(rl, dwLogId, NULL, args);
}

HRESULT DAPIV LogErrorString(
    IN HRESULT hrError,
    IN LPCWSTR wzFormat,
    ...
    );

HRESULT DAPI LogErrorStringArgs(
    IN HRESULT hrError,
    IN LPCWSTR wzFormat,
    IN va_list args
    );

HRESULT DAPI LogErrorIdModule(
    IN HRESULT hrError,
    IN DWORD dwLogId,
    IN HMODULE hModule,
    IN LPCWSTR wzString1,
    IN LPCWSTR wzString2,
    IN LPCWSTR wzString3
    );

inline HRESULT LogErrorId(
    IN HRESULT hrError,
    IN DWORD dwLogId,
    IN LPCWSTR wzString1,
    IN LPCWSTR wzString2,
    IN LPCWSTR wzString3
    )
{
    return LogErrorIdModule(hrError, dwLogId, NULL, wzString1, wzString2, wzString3);
}

HRESULT DAPI LogHeader();

HRESULT DAPI LogFooter();

// begin the switch of LogXXX to LogStringXXX
#define Log LogString
#define LogLine LogStringLine

#ifdef __cplusplus
}
#endif

