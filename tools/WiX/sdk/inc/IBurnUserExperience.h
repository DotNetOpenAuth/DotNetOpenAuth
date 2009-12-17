//-------------------------------------------------------------------------------------------------
// <copyright file="IBurnUserExperience.h" company="Microsoft">
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


enum BURN_DISPLAY
{
    BURN_DISPLAY_UNKNOWN,
    BURN_DISPLAY_NONE,
    BURN_DISPLAY_PASSIVE,
    BURN_DISPLAY_FULL,
};


enum BURN_RESTART
{
    BURN_RESTART_UNKNOWN,
    BURN_RESTART_NEVER,
    BURN_RESTART_PROMPT,
    BURN_RESTART_AUTOMATIC,
    BURN_RESTART_ALWAYS,
};


struct BURN_COMMAND
{
    BURN_ACTION action;
    BURN_DISPLAY display;
    BURN_RESTART restart;

    BOOL fResumed;
};


DECLARE_INTERFACE_IID_(IBurnUserExperience, IUnknown, "e1e09b81-3fca-11dd-8291-001d09081dd9")
{
    STDMETHOD(Initialize)(
        __in IBurnCore* pCore,
        __in int nCmdShow
        ) PURE;

    STDMETHOD(Run)() PURE;

    STDMETHOD_(void, Uninitialize)() PURE;

    STDMETHOD_(int, OnDetectBegin)(
        __in DWORD cPackages
        );

    STDMETHOD_(int, OnDetectPackageBegin)(
        __in_z LPCWSTR wzPackageId
        ) PURE;

    STDMETHOD_(void, OnDetectPackageComplete)(
        __in LPCWSTR wzPackageId,
        __in HRESULT hrStatus,
        __in PACKAGE_STATE state
        ) PURE;

    STDMETHOD_(void, OnDetectComplete)(
        __in HRESULT hrStatus
        ) PURE;

    STDMETHOD_(int, OnPlanBegin)(
        __in DWORD cPackages
        );

    STDMETHOD_(int, OnPlanPackageBegin)(
        __in_z LPCWSTR wzPackageId
        ) PURE;

    STDMETHOD_(void, OnPlanPackageComplete)(
        __in LPCWSTR wzPackageId,
        __in HRESULT hrStatus,
        __in PACKAGE_STATE state,
        __in REQUEST_STATE requested,
        __in ACTION_STATE execute,
        __in ACTION_STATE rollback
        ) PURE;

    STDMETHOD_(void, OnPlanComplete)(
        __in HRESULT hrStatus
        ) PURE;

    STDMETHOD_(int, OnApplyBegin)();

    STDMETHOD_(int, OnRegisterBegin)();

    STDMETHOD_(void, OnRegisterComplete)(
        __in HRESULT hrStatus
        );

    STDMETHOD_(void, OnUnregisterBegin)();

    STDMETHOD_(void, OnUnregisterComplete)(
        __in HRESULT hrStatus
        );

    STDMETHOD_(void, OnCacheComplete)(
        __in HRESULT hrStatus
        ) PURE;

    STDMETHOD_(int, OnExecuteBegin)(
        __in DWORD cExecutingPackages
        );

    STDMETHOD_(int, OnExecutePackageBegin)(
        __in LPCWSTR wzPackageId,
        __in BOOL fExecute
        ) PURE;

    STDMETHOD_(int, OnError)(
        __in DWORD dwCode,
        __in_z LPCWSTR wzError,
        __in DWORD dwUIHint
        ) PURE;

    STDMETHOD_(int, OnProgress)(
        __in DWORD dwProgressPercentage,
        __in DWORD dwOverallPercentage
        ) PURE;

    STDMETHOD_(void, OnExecutePackageComplete)(
        __in LPCWSTR wzPackageId,
        __in HRESULT hrExitCode
        ) PURE;

    STDMETHOD_(void, OnExecuteComplete)(
        __in HRESULT hrStatus
        ) PURE;

    STDMETHOD_(BOOL, OnRestartRequired)() PURE;

    STDMETHOD_(void, OnApplyComplete)(
        __in HRESULT hrStatus
        );

    STDMETHOD(GetControlText)(
        __in DWORD dwControlID,
        __out LPWSTR* psczText
        ) PURE;
};


extern "C" typedef HRESULT (WINAPI *PFN_CREATE_USER_EXPERIENCE)(
    __in BURN_COMMAND* pCommand,
    __out IBurnUserExperience** ppUX
    );
