Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Web


Public Class TracePageAppender
	Inherits log4net.Appender.AppenderSkeleton

	Protected Overrides Sub Append(ByVal loggingEvent As log4net.Core.LoggingEvent)
		Dim sw As StringWriter = New StringWriter(Global_asax.LogMessages)
		Layout.Format(sw, loggingEvent)
	End Sub
End Class