
###
### OneGet/common.inc
###

URT_VER=4.0
MANAGED_CODE        = 1

ASSEMBLY_IDENTITY_VERSION_NAME=monad
TARGET_DESTINATION  = monad
ASSEMBLY_IDENTITY_PUBLIC_KEY_NAME   = windows

# library refernces
ONEGET_CORE=$(OBJ_PATH)\..\OneGet\$(O)\Microsoft.OneGet.metadata_exe;
ONEGET_UTILITY=$(OBJ_PATH)\..\Utility\$(O)\Microsoft.OneGet.Utility.metadata_dll;
ONEGET_UTILITY_POWERSHELL=$(OBJ_PATH)\..\Utility.PowerShell\$(O)\Microsoft.OneGet.Utility.PowerShell.metadata_dll;
