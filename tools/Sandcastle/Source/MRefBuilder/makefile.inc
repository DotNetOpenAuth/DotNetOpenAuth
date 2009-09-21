!INCLUDE $(INETROOT)\build\makefile.inc

$(TARGETNAME).exe.config: MRefBuilder.config
        copy MRefBuilder.config $(TARGETNAME).exe.config
