LIBRARY = ..\BuildAssembler\BuildAssembler.exe

FLOW_COMPONENTS = IfThenComponent.cs ForEachComponent.cs CloneComponent.cs
DIAGNOSTIC_COMPONENTS = DisplayComponent.cs ValidateComponent.cs
PROCESSING_COMPONENTS = CopyFromFileComponent.cs CopyFromDirectoryComponent.cs SharedContentComponent.cs TransformComponent.cs SaveComponent.cs ResolveLinksComponent.cs MsdnService.cs

COMPONENTS = $(FLOW_COMPONENTS) $(DIAGNOSTIC_COMPONENTS) $(PROCESSING_COMPONENTS)

all: BuildComponents.dll

BuildComponents.dll: *Component.cs CustomContext.cs MsdnService.cs $(LIBRARY)
	csc /t:library /out:BuildComponents.dll *Component.cs CustomContext.cs MsdnService.cs /r:$(LIBRARY)
	copy BuildComponents.dll ..\..\ProductionTools\BuildComponents
