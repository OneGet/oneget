
###
### common.inc
###

URT_VER=4.0
MANAGED_CODE        = 1

ASSEMBLY_IDENTITY_VERSION_NAME=monad
TARGET_DESTINATION  = monad
ASSEMBLY_IDENTITY_PUBLIC_KEY_NAME   = windows

# library refernces
PACKAGEMANAGEMENT_CORE=$(OBJ_PATH)\..\PackageManagement\$(O)\Microsoft.PackageManagement.metadata_dll;
PACKAGEMANAGEMENT_UTILITY=$(OBJ_PATH)\..\Utility\$(O)\Microsoft.PackageManagement.Utility.metadata_dll;
PACKAGEMANAGEMENT_UTILITY_POWERSHELL=$(OBJ_PATH)\..\Utility.PowerShell\$(O)\Microsoft.PackageManagement.Utility.PowerShell.metadata_dll;
