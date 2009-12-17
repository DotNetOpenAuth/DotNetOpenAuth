#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="fileutil.h" company="Microsoft">
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
//    Header for file helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

#define ReleaseFile(h) if (INVALID_HANDLE_VALUE != h) { ::CloseHandle(h); h = INVALID_HANDLE_VALUE; }
#define ReleaseFileHandle(h) if (INVALID_HANDLE_VALUE != h) { ::CloseHandle(h); h = INVALID_HANDLE_VALUE; }
#define ReleaseFileFindHandle(h) if (INVALID_HANDLE_VALUE != h) { ::FindClose(h); h = INVALID_HANDLE_VALUE; }

enum FILE_ARCHITECTURE
{
    FILE_ARCHITECTURE_UNKNOWN,
    FILE_ARCHITECTURE_X86,
    FILE_ARCHITECTURE_X64,
    FILE_ARCHITECTURE_IA64,
};


LPWSTR DAPI FileFromPath(
    __in LPCWSTR wzPath
    );
HRESULT DAPI FileResolvePath(
    __in LPCWSTR wzRelativePath,
    __out LPWSTR *ppwzFullPath
    );
HRESULT DAPI FileStripExtension(
    __in LPCWSTR wzFileName,
    __out LPWSTR *ppwzFileNameNoExtension
    );
HRESULT DAPI FileChangeExtension(
    __in LPCWSTR wzFileName,
    __in LPCWSTR wzNewExtension,
    __out LPWSTR *ppwzFileNameNewExtension
    );
HRESULT DAPI FileAddSuffixToBaseName(
    __in_z LPCWSTR wzFileName,
    __in_z LPCWSTR wzSuffix,
    __out_z LPWSTR* psczNewFileName
    );
HRESULT DAPI FileVersionFromString(
    __in LPCWSTR wzVersion, 
    __out DWORD *pdwVerMajor, 
    __out DWORD* pdwVerMinor
    );
HRESULT DAPI FileVersionFromStringEx(
    __in LPCWSTR wzVersion,
    __in DWORD cchVersion,
    __out DWORD64* pqwVersion
    );
HRESULT DAPI FileSetPointer(
    __in HANDLE hFile,
    __in DWORD64 dw64Move,
    __out_opt DWORD64* pdw64NewPosition,
    __in DWORD dwMoveMethod
    );
HRESULT DAPI FileSize(
    __in LPCWSTR pwzFileName,
    __out LONGLONG* pllSize
    );
HRESULT DAPI FileSizeByHandle(
    __in HANDLE hFile, 
    __out LONGLONG* pllSize
    );
BOOL DAPI FileExistsEx(
    __in LPCWSTR wzPath, 
    __out_opt DWORD *pdwAttributes
    );
HRESULT DAPI FileRead(
    __deref_out_bcount_full(*pcbDest) LPBYTE* ppbDest,
    __out DWORD* pcbDest,
    __in LPCWSTR wzSrcPath
    );
HRESULT DAPI FileReadUntil(
    __deref_out_bcount_full(*pcbDest) LPBYTE* ppbDest,
    __out_range(<=, cbMaxRead) DWORD* pcbDest,
    __in LPCWSTR wzSrcPath,
    __in DWORD cbMaxRead
    );
HRESULT DAPI FileReadPartial(
    __deref_out_bcount_full(*pcbDest) LPBYTE* ppbDest,
    __out_range(<=, cbMaxRead) DWORD* pcbDest,
    __in LPCWSTR wzSrcPath,
    __in BOOL fSeek,
    __in DWORD cbStartPosition,
    __in DWORD cbMaxRead,
    __in BOOL fPartialOK
    );
HRESULT DAPI FileWrite(
    __in_bcount(cbData) LPCBYTE pbData,
    __in DWORD cbData,
    __in LPCWSTR pwzFileName,
    __in DWORD dwFlagsAndAttributes,
    __out_opt HANDLE* pHandle
    );
HRESULT DAPI FileEnsureCopy(
    __in LPCWSTR wzSource,
    __in LPCWSTR wzTarget,
    __in BOOL fOverwrite
    );
HRESULT DAPI FileEnsureMove(
    __in LPCWSTR wzSource, 
    __in LPCWSTR wzTarget, 
    __in BOOL fOverwrite,
    __in BOOL fAllowCopy
    );
HRESULT DAPI FileCreateTemp(
    __in LPCWSTR wzPrefix,
    __in LPCWSTR wzExtension,
    __deref_opt_out_z LPWSTR* ppwzTempFile,
    __out_opt HANDLE* phTempFile
    );
HRESULT DAPI FileCreateTempW(
    __in LPCWSTR wzPrefix,
    __in LPCWSTR wzExtension,
    __deref_opt_out_z LPWSTR* ppwzTempFile,
    __out_opt HANDLE* phTempFile
    );
HRESULT DAPI FileVersion(
    __in LPCWSTR wzFilename, 
    __out DWORD *pdwVerMajor, 
    __out DWORD* pdwVerMinor
    );
HRESULT DAPI FileIsSame(
    __in LPCWSTR wzFile1,
    __in LPCWSTR wzFile2,
    __out LPBOOL lpfSameFile
    );
HRESULT DAPI FileEnsureDelete(
    __in LPCWSTR wzFile
    );
HRESULT DAPI FileGetTime(
    __in LPCWSTR wzFile,  
    __out_opt  LPFILETIME lpCreationTime,
    __out_opt  LPFILETIME lpLastAccessTime,
    __out_opt  LPFILETIME lpLastWriteTime
    );
HRESULT DAPI FileSetTime(
    __in LPCWSTR wzFile,
    __in_opt  const FILETIME *lpCreationTime,
    __in_opt  const FILETIME *lpLastAccessTime,
    __in_opt  const FILETIME *lpLastWriteTime
    );
HRESULT DAPI FileResetTime(
    __in LPCWSTR wzFile
    );
HRESULT FileExecutableArchitecture(
    __in LPCWSTR wzFile,
    __out FILE_ARCHITECTURE *pArchitecture
    );

#ifdef __cplusplus
}
#endif
