'Module1.vb
Imports System
Imports System.ServiceModel
Imports System.ServiceModel.Description
Imports RBECtronLib

Module Service
    Class Program
        Shared Sub Main()
            ' Step 1 Create a URI to serve as the base address
            'Dim baseAddress As New Uri("http://192.168.1.239:8000/SpectronService/Spectron")
            'Dim baseAddress As New Uri("http://192.168.1.239:8000/SpectronService/Spectron")
            Dim baseAddress As New Uri("http://localhost:8000/SpectronService/Spectron")
            Dim binding As New WSHttpBinding()
            With binding
                .Name = "binding1"
                .HostNameComparisonMode = HostNameComparisonMode.StrongWildcard
                .ReliableSession.Enabled = True
                .ReliableSession.InactivityTimeout = New TimeSpan(4, 0, 0, 0, 0)
                .ReceiveTimeout = New TimeSpan(4, 0, 0, 0, 0)
                '.Security.Mode = SecurityMode.None
            End With

            ' Step 2 Create a ServiceHost instance
            Dim selfHost As New ServiceHost(GetType(SpectronService), baseAddress)
            Try
                ' Step 3 Add a service endpoint
                ' Add a service endpoint
                selfHost.AddServiceEndpoint(
                        GetType(ISpectron),
                        binding,
                        "SpectronService")

                ' Step 4 Enable metadata exchange.
                Dim smb As New ServiceMetadataBehavior()
                Dim sdb As New ServiceDebugBehavior()
                sdb.IncludeExceptionDetailInFaults = True
                smb.HttpGetEnabled = True
                'System.ServiceModel.Configuration.ServiceHostingEnvironmentSection.]
                selfHost.Description.Behaviors.Add(smb)
                'selfHost.Description.Behaviors.Add(sdb)


                ' Step 5 Start the service
                selfHost.Open()
                Console.WriteLine("The service is ready.")
                Console.WriteLine("Press <ENTER> to terminate service.")
                Console.WriteLine()
                Console.ReadLine()

                ' Close the ServiceHostBase to shutdown the service.
                selfHost.Close()
            Catch ce As CommunicationException
                Console.WriteLine("An exception occurred: {0}", ce.Message)
                Console.ReadLine()
                selfHost.Abort()
            End Try
        End Sub
    End Class

End Module

