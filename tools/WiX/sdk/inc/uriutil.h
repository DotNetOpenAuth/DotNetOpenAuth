//-------------------------------------------------------------------------------------------------
// <copyright file="uriutil.h" company="Microsoft">
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
//    URI helper funtions.
// </summary>
//-------------------------------------------------------------------------------------------------

#pragma once


#ifdef __cplusplus
extern "C" {
#endif

enum URI_PROTOCOL
{
    URI_PROTOCOL_UNKNOWN,
    URI_PROTOCOL_FILE,
    URI_PROTOCOL_FTP,
    URI_PROTOCOL_HTTP,
    URI_PROTOCOL_LOCAL,
    URI_PROTOCOL_UNC
};


HRESULT DAPI UriCanonicalize(
    __inout_z LPWSTR* psczUri
    );

HRESULT DAPI UriCrack(
    __in_z LPCWSTR wzUri,
    __out_opt INTERNET_SCHEME* pScheme,
    __inout_z_opt LPWSTR* psczHostName,
    __out_opt INTERNET_PORT* pPort,
    __inout_z_opt LPWSTR* psczUser,
    __inout_z_opt LPWSTR* psczPassword,
    __inout_z_opt LPWSTR* psczPath,
    __inout_z_opt LPWSTR* psczQueryString
    );

HRESULT DAPI UriCreate(
    __inout_z LPWSTR* psczUri,
    __in INTERNET_SCHEME scheme,
    __in_z_opt LPWSTR wzHostName,
    __in INTERNET_PORT port,
    __in_z_opt LPWSTR wzUser,
    __in_z_opt LPWSTR wzPassword,
    __in_z_opt LPWSTR wzPath,
    __in_z_opt LPWSTR wzQueryString
    );

HRESULT DAPI UriCanonicalize(
    __inout_z LPWSTR* psczUri
    );

HRESULT DAPI UriFile(
    __deref_out_z LPWSTR* psczFile,
    __in LPCWSTR wzUri
    );

HRESULT DAPI UriProtocol(
    __in LPCWSTR wzUri,
    __out URI_PROTOCOL* pProtocol
    );

HRESULT DAPI UriRoot(
    __in LPCWSTR wzUri,
    __out LPWSTR* ppwzRoot,
    __out_opt URI_PROTOCOL* pProtocol
    );

HRESULT DAPI UriResolve(
    __in LPCWSTR wzUri,
    __in_opt LPCWSTR wzBaseUri,
    __out LPWSTR* ppwzResolvedUri,
    __out_opt URI_PROTOCOL* pResolvedProtocol
    );

#ifdef __cplusplus
}
#endif

