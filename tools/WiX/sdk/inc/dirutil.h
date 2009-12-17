//-------------------------------------------------------------------------------------------------
// <copyright file="dirutil.h" company="Microsoft">
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
//    Directory helper funtions.
// </summary>
//-------------------------------------------------------------------------------------------------

#pragma once

#ifdef __cplusplus
extern "C" {
#endif

BOOL DAPI DirExists(
    __in LPCWSTR wzPath, 
    __out_opt DWORD *pdwAttributes
    );

HRESULT DAPI DirCreateTempPath(
    __in LPCWSTR wzPrefix,
    __in LPWSTR wzPath,
    __in DWORD cchPath
    );

HRESULT DAPI DirEnsureExists(
    __in LPCWSTR wzPath, 
    __in_opt LPSECURITY_ATTRIBUTES psa
    );

HRESULT DAPI DirEnsureDelete(
    __in LPCWSTR wzPath,
    __in BOOL fDeleteFiles,
    __in BOOL fRecurse
    );

#ifdef __cplusplus
}
#endif

