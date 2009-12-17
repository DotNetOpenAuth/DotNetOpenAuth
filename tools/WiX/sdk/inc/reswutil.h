//-------------------------------------------------------------------------------------------------
// <copyright file="reswutil.h" company="Microsoft">
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
//    Resource writer helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#pragma once


#ifdef __cplusplus
extern "C" {
#endif

HRESULT DAPI ResWriteString(
    __in LPCWSTR wzResourceFile,
    __in DWORD dwDataId,
    __in LPCWSTR wzData,
    __in WORD wLangId
    );

HRESULT DAPI ResWriteData(
    __in LPCWSTR wzResourceFile,
    __in LPCSTR szDataName,
    __in PVOID pData,
    __in DWORD cbData
    );

HRESULT DAPI ResImportDataFromFile(
    __in LPCWSTR wzTargetFile,
    __in LPCWSTR wzSourceFile,
    __in LPCSTR szDataName
    );

#ifdef __cplusplus
}
#endif
