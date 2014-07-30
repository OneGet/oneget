URT_VER=4.5

RESOURCES_NAMESPACE=Microsoft.PowerShell.OneGet.Resources
MESSAGES_RESOURCES=$(OBJDIR)\$(RESOURCES_NAMESPACE).Messages.resources

resources:
!if "$(MANAGED_RESOURCE_FILES)" != ""
        resgen.exe /useSourcePath /compile $(MANAGED_RESOURCE_FILES)
!endif
$(OBJDIR)\Messages.cs: Resources\Messages.resx
   resgen Resources\Messages.resx $(MESSAGES_RESOURCES) /str:c#,$(RESOURCES_NAMESPACE),Messages,$(OBJDIR)\Messages.cs

$(MESSAGES_RESOURCES): Resources\Messages.resx
