#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="strutil.h" company="Microsoft">
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

#define ReleaseStr(pwz) if (pwz) { StrFree(pwz); }
#define ReleaseNullStr(pwz) if (pwz) { StrFree(pwz); pwz = NULL; }
#define ReleaseBSTR(bstr) if (bstr) { ::SysFreeString(bstr); }
#define ReleaseNullBSTR(bstr) if (bstr) { ::SysFreeString(bstr); bstr = NULL; }

#define DeclareConstBSTR(bstr_const, wz) const WCHAR bstr_const[] = { 0x00, 0x00, sizeof(wz)-sizeof(WCHAR), 0x00, wz }
#define UseConstBSTR(bstr_const) const_cast<BSTR>(bstr_const + 4)

HRESULT DAPI StrAlloc(
    __deref_out_ecount_part(cch, 0) LPWSTR* ppwz,
    __in DWORD_PTR cch
    );
HRESULT DAPI StrAnsiAlloc(
    __deref_out_ecount_part(cch, 0) LPSTR* ppz,
    __in DWORD_PTR cch
    );
HRESULT DAPI StrAllocString(
    __deref_out_ecount_z(cchSource+1) LPWSTR* ppwz,
    __in LPCWSTR wzSource,
    __in DWORD_PTR cchSource
    );
HRESULT DAPI StrAnsiAllocString(
    __deref_out_ecount_z(cchSource+1) LPSTR* ppsz,
    __in LPCWSTR wzSource,
    __in DWORD_PTR cchSource,
    __in UINT uiCodepage
    );
HRESULT DAPI StrAllocStringAnsi(
    __deref_out_ecount_z(cchSource+1) LPWSTR* ppwz,
    __in LPCSTR szSource,
    __in DWORD_PTR cchSource,
    __in UINT uiCodepage
    );
HRESULT DAPI StrAllocPrefix(
    __deref_out_z LPWSTR* ppwz,
    __in LPCWSTR wzPrefix,
    __in DWORD_PTR cchPrefix
    );
HRESULT DAPI StrAllocConcat(
    __deref_out_z LPWSTR* ppwz,
    __in LPCWSTR wzSource,
    __in DWORD_PTR cchSource
    );
HRESULT DAPI StrAnsiAllocConcat(
    __deref_out_z LPSTR* ppz,
    __in LPCSTR pzSource,
    __in DWORD_PTR cchSource
    );
HRESULT __cdecl StrAllocFormatted(
    __deref_out_z LPWSTR* ppwz,
    __in __format_string LPCWSTR wzFormat,
    ...
    );
HRESULT __cdecl StrAnsiAllocFormatted(
    __deref_out_z LPSTR* ppsz,
    __in __format_string LPCSTR szFormat,
    ...
    );
HRESULT DAPI StrAllocFormattedArgs(
    __deref_out_z LPWSTR* ppwz,
    __in __format_string LPCWSTR wzFormat,
    __in va_list args
    );
HRESULT DAPI StrAnsiAllocFormattedArgs(
    __deref_out_z LPSTR* ppsz,
    __in __format_string LPCSTR szFormat,
    __in va_list args
    );

HRESULT DAPI StrMaxLength(
    __in LPVOID p,
    __out DWORD_PTR* pcch
    );
HRESULT DAPI StrSize(
    __in LPVOID p,
    __out DWORD_PTR* pcb
    );

HRESULT DAPI StrFree(
    __in LPVOID p
    );

HRESULT DAPI StrCurrentTime(
    __deref_out_z LPWSTR* ppwz,
    __in BOOL fGMT
    );
HRESULT DAPI StrCurrentDateTime(
    __deref_out_z LPWSTR* ppwz,
    __in BOOL fGMT
    );

HRESULT DAPI StrReplaceStringAll(
    __inout LPWSTR* ppwzOriginal,
    __in LPCWSTR wzOldSubString,
    __in LPCWSTR wzNewSubString
    );
HRESULT DAPI StrReplaceString(
    __inout LPWSTR* ppwzOriginal,
    __inout DWORD* dwStartIndex,
    __in LPCWSTR wzOldSubString,
    __in LPCWSTR wzNewSubString
    );

HRESULT DAPI StrHexEncode(
    __in_ecount(cbSource) const BYTE* pbSource,
    __in DWORD_PTR cbSource,
    __out_ecount(cchDest) LPWSTR wzDest,
    __in DWORD_PTR cchDest
    );
HRESULT DAPI StrHexDecode(
    __in LPCWSTR wzSource,
    __out_bcount(cbDest) BYTE* pbDest,
    __in DWORD_PTR cbDest
    );

HRESULT DAPI StrAllocBase85Encode(
    __in_bcount(cbSource) const BYTE* pbSource,
    __in DWORD_PTR cbSource,
    __deref_out_z LPWSTR* pwzDest
    );
HRESULT DAPI StrAllocBase85Decode(
    __in LPCWSTR wzSource,
    __deref_out_bcount(*pcbDest) BYTE** hbDest,
    __out DWORD_PTR* pcbDest
    );

HRESULT DAPI MultiSzLen(
    __in LPCWSTR pwzMultiSz,
    __out DWORD_PTR* pcch
    );
HRESULT DAPI MultiSzPrepend(
    __deref_inout_z LPWSTR* ppwzMultiSz,
    __inout_opt DWORD_PTR *pcchMultiSz,
    __in LPCWSTR pwzInsert
    );
HRESULT DAPI MultiSzFindSubstring(
    __in LPCWSTR pwzMultiSz,
    __in LPCWSTR pwzSubstring,
    __out_opt DWORD_PTR* pdwIndex,
    __deref_opt_out_z LPCWSTR* ppwzFoundIn
    );
HRESULT DAPI MultiSzFindString(
    __in LPCWSTR pwzMultiSz,
    __in LPCWSTR pwzString,
    __out_opt DWORD_PTR* pdwIndex,
    __deref_opt_out_z LPCWSTR* ppwzFound
    );
HRESULT DAPI MultiSzRemoveString(
    __deref_inout_z LPWSTR* ppwzMultiSz,
    __in DWORD_PTR dwIndex
    );
HRESULT DAPI MultiSzInsertString(
    __deref_inout_z LPWSTR* ppwzMultiSz,
    __inout_opt DWORD_PTR *pcchMultiSz,
    __in DWORD_PTR dwIndex,
    __in LPCWSTR pwzInsert
    );
HRESULT DAPI MultiSzReplaceString(
    __deref_inout_z LPWSTR* ppwzMultiSz,
    __in DWORD_PTR dwIndex,
    __in LPCWSTR pwzString
    );

LPCWSTR wcsistr(
    __in LPCWSTR wzString,
    __in LPCWSTR wzCharSet
    );

HRESULT DAPI StrStringToUInt16(
    __in_z LPCWSTR wzIn,
    __in DWORD cchIn,
    __out USHORT* pusOut
    );
HRESULT DAPI StrStringToInt64(
    __in_z LPCWSTR wzIn,
    __in DWORD cchIn,
    __out LONGLONG* pllOut
    );
void DAPI StrStringToUpper(
    __inout_z LPWSTR wzIn
    );
void DAPI StrStringToLower(
    __inout_z LPWSTR wzIn
    );

#ifdef __cplusplus
}
#endif
