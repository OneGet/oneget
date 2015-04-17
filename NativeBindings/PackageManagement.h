
typedef int bool;

typedef bool(*fnIsCanceled)();
typedef void(*fnGetMessageString)(const wchar_t* messageText, const wchar_t* defaultText, fnReturnString resultFn);
typedef bool(*fnWarning)(const char* messageText);

typedef struct HostApi {
    fnIsCanceled IsCanceled;
    fnGetMessageString GetMessageString;
    fnWarning Warning;
} IHostApi;

typedef void* HProvider;

typedef bool(*fnStringEnumerator)(const wchar_t* each);

// ---------------------------------------------------------------
// PackageManagementService
int GetVersion();
bool Initialize(IHostApi hostCallbacks);
void GetProviderNames(fnStringEnumerator strEnumerator);
void GetAvailableProviderNames(fnStringEnumerator strEnumerator);
HProvider GetPackageProvider(const wchar_t* name);
void FindProvidersWithFeature(const wchar_t* featureName, const wchar_t* value fnStringEnumerator strEnumerator);
bool RequirePackageProvider(const wchar_t* requestor, const wchar_t* packageProviderName, const wchar_t* minimumVersion, IHostApi hostCallbacks);

// ---------------------------------------------------------------
// PackageProvider 
void PP_GetName(HProvider provider, fnReturnString resultFn);
void PP_GetVersion(HProvider provider, fnReturnString resultFn);
void PP_GetFeatureKeys(HProvider provider, fnStringEnumerator eachFeatureKey);
void PP_GetFeatureValues(HProvider provider, const wchar_t* featureName, fnStringEnumerator eachFeatureValue);
void PP_GetDynamicOptions(HProvider provider, OptionCategory category, fnDynamicOptionEnumerator eachDynamicOption);
void PP_GetSupportedUriSchemes(HProvider provider, fnStringEnumerator strEnumerator);
void PP_GetSupportedFileExtensions(HProvider provider, fnStringEnumerator strEnumerator);

ASYNC_HANDLE PP_AddPackageSource(string name, string location, bool trusted, fnPackageSourceResult eachPackageSource, IHostApi requestObject)
ASYNC_HANDLE PP_RemovePackageSource(string name, fnPackageSourceResult eachPackageSource, IHostApi requestObject);

ASYNC_HANDLE PP_FindPackage(string name, string requiredVersion, string minimumVersion, string maximumVersion, int id, IHostApi requestObject)
ASYNC_HANDLE PP_FindPackageByUri(Uri uri, int id, IHostApi requestObject)
ASYNC_HANDLE PP_GetPackageDependencies(SoftwareIdentity package, IHostApi requestObject)
ASYNC_HANDLE PP_FindPackageByFile(string filename, int id, IHostApi requestObject)

int PP_StartFind(IHostApi requestObject)
ASYNC_HANDLE PP_CompleteFind(int i, IHostApi requestObject)

ASYNC_HANDLE PP_InstallPackage(SoftwareIdentity softwareIdentity, IHostApi requestObject)
ASYNC_HANDLE PP_UninstallPackage(SoftwareIdentity softwareIdentity, IHostApi requestObject)

ASYNC_HANDLE PP_ResolvePackageSources(IHostApi requestObject)
ASYNC_HANDLE PP_DownloadPackage(SoftwareIdentity softwareIdentity, string destinationFilename, IHostApi requestObject)


void ASYNC_Wait(ASYNC_HANDLE action);
void ASYNC_Cancel(ASYNC_HANDLE action);
void ASYNC_Abort(ASYNC_HANDLE action);

bool ASYNC_IsCanceled(ASYNC_HANDLE action);
bool ASYNC_IsAborted(ASYNC_HANDLE action);
bool ASYNC_IsCompleted(ASYNC_HANDLE action);

