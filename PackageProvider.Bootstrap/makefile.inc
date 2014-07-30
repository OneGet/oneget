!include $(PROJECT_ROOT)\monad\nttargets\build\makefile.inc.inc

URT_VER=4.5

resources:
!if "$(MANAGED_RESOURCE_FILES)" != ""
        resgen.exe /useSourcePath /compile $(MANAGED_RESOURCE_FILES)
!endif