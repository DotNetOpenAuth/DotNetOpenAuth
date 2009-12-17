//-------------------------------------------------------------------------------------------------
// <copyright file="locutil.h" company="Microsoft">
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
//    Header for localization helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------
#pragma once

#ifdef __cplusplus
extern "C" {
#endif

struct LOC_STRING
{
    LPWSTR wzID;
    LPWSTR wzText;
    BOOL bOverridable;
};

struct LOC_STRINGSET
{
    DWORD cLocStrings;
    LOC_STRING* rgLocStrings;
};

HRESULT DAPI LocLoadFromFile(
    __in LPCWSTR wzWxlFile,
    __out LOC_STRINGSET** ppLocStringSet
    );

HRESULT DAPI LocLocalizeString(
    __in LOC_STRINGSET* pLocStringSet,
    __inout LPWSTR* ppInput
    );

void DAPI LocFree(
    __in_opt LOC_STRINGSET* pLocStringSet
    );

#ifdef __cplusplus
}
#endif