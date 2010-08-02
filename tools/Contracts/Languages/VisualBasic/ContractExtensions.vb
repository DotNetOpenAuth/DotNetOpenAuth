'
'  Include this file in your project if you want to use
'  ContractArgumentValidator methods or ContractAbbreviator methods
'


''' <summary>
''' Enables factoring legacy if-then-throw into separate methods for reuse and full control over
''' thrown exception and arguments
''' </summary>
<AttributeUsage(AttributeTargets.Method, AllowMultiple:=False)> _
<Conditional("CONTRACTS_FULL")> _
NotInheritable Class ContractArgumentValidatorAttribute
  Inherits Global.System.Attribute
End Class

''' <summary>
''' Enables writing abbreviations for contracts that get copied to other methods
''' </summary>
<AttributeUsage(AttributeTargets.Method, AllowMultiple:=False)> _
<Conditional("CONTRACTS_FULL")> _
NotInheritable Class ContractAbbreviatorAttribute
  Inherits Global.System.Attribute
End Class

''' <summary>
''' Allows setting contract and tool options at assembly, type, or method granularity.
''' </summary>
<AttributeUsage(AttributeTargets.All, AllowMultiple:=True, Inherited:=False)> _
<Conditional("CONTRACTS_FULL")> _
NotInheritable Class ContractOptionAttribute
  Inherits Global.System.Attribute

  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="category")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="setting")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="toggle")> _
  Public Sub New(ByVal category As String, ByVal setting As String, ByVal toggle As Boolean)
  End Sub

  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="category")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="setting")> _
  <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="value")> _
  Public Sub New(ByVal category As String, ByVal setting As String, ByVal value As String)
  End Sub

End Class
