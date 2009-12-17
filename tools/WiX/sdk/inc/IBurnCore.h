//-------------------------------------------------------------------------------------------------
// <copyright file="IBurnCore.h" company="Microsoft">
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
//-------------------------------------------------------------------------------------------------

#pragma once

#define IDERROR -1
#define IDNOACTION 0


enum BURN_ACTION
{
    BURN_ACTION_UNKNOWN,
    BURN_ACTION_HELP,
    BURN_ACTION_UNINSTALL,
    BURN_ACTION_INSTALL,
    BURN_ACTION_MODIFY,
    BURN_ACTION_REPAIR,
};

enum ACTION_STATE
{
    ACTION_STATE_NONE,
    ACTION_STATE_UNINSTALL,
    ACTION_STATE_INSTALL,
    ACTION_STATE_ADMIN_INSTALL,
    ACTION_STATE_MAINTENANCE,
    ACTION_STATE_RECACHE,
    ACTION_STATE_MINOR_UPGRADE,
    ACTION_STATE_MAJOR_UPGRADE,
    ACTION_STATE_PATCH,
};

enum PACKAGE_STATE
{
    PACKAGE_STATE_UNKNOWN,
    PACKAGE_STATE_ABSENT,
    PACKAGE_STATE_CACHED,
    PACKAGE_STATE_PRESENT,
};

enum REQUEST_STATE
{
    REQUEST_STATE_NONE,
    REQUEST_STATE_ABSENT,
    REQUEST_STATE_CACHE,
    REQUEST_STATE_PRESENT,
    REQUEST_STATE_REPAIR,
};


DECLARE_INTERFACE_IID_(IBurnCore, IUnknown, "e1e09b80-3fca-11dd-8291-001d09081dd9")
{
    STDMETHOD(GetPackageCount)(
        __out DWORD* pcPackages
        ) PURE;

    STDMETHOD(GetCommandLineParameters)(
        __out LPWSTR* psczCommandLine
        ) PURE;
/*
    STDMETHOD(SetFeatureActionState)(
        __in DWORD dwPackageIndex,
        __in LPCWSTR wzFeature,
        __in FEATURE_STATE fsState
        );

    STDMETHOD(SetPackageActionState)(
        __in DWORD dwPackageIndex,
        __in PACKAGE_STATE state
        );
*/

    STDMETHOD(GetPropertyNumeric)(
        __in_z LPCWSTR wzProperty,
        __out LONGLONG* pllValue
        ) PURE;

    STDMETHOD(GetPropertyString)(
        __in_z LPCWSTR wzProperty,
        __out_z LPWSTR* psczValue
        ) PURE;

    STDMETHOD(GetPropertyVersion)(
        __in_z LPCWSTR wzProperty,
        __in DWORD64* pqwValue
        ) PURE;

    STDMETHOD(SetPropertyNumeric)(
        __in_z LPCWSTR wzProperty,
        __in LONGLONG llValue
        ) PURE;

    STDMETHOD(SetPropertyString)(
        __in_z LPCWSTR wzProperty,
        __in_z_opt LPCWSTR wzValue
        ) PURE;

    STDMETHOD(SetPropertyVersion)(
        __in_z LPCWSTR wzProperty,
        __in DWORD64 qwValue
        ) PURE;

    STDMETHOD(FormatPropertyString)(
        __in_z LPCWSTR wzIn,
        __inout_z LPWSTR* psczOut
        ) PURE;

    STDMETHOD(EvaluateCondition)(
        __in_z LPCWSTR wzCondition,
        __out BOOL* pf
        ) PURE;

    STDMETHOD(Elevate)(
        __in_opt HWND hwndParent
        ) PURE;

    STDMETHOD(Detect)() PURE;

    STDMETHOD(Plan)(
        __in BURN_ACTION action
        ) PURE;

    STDMETHOD(Apply)(
        __in_opt HWND hwndParent
        ) PURE;
};
