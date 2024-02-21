Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.ServiceModel
Imports RBECtronClient.SpectronServiceReference

Module Module1

    Sub Main()
        ' Step 1: Create an instance of the WCF proxy
        'I don't know why the extra '/Service' is needed in the endpoint address 
        Dim epAddress As New EndpointAddress("http://192.168.1.239:8000/SpectronService/Spectron/SpectronService")
        Dim binding As New WSHttpBinding()
        With binding
            .Name = "binding1"
            .HostNameComparisonMode = HostNameComparisonMode.StrongWildcard
            .ReliableSession.Enabled = True
            .ReliableSession.InactivityTimeout = New TimeSpan(4, 0, 0, 0, 0)
            .ReceiveTimeout = New TimeSpan(4, 0, 0, 0, 0)
            .Security.Mode = SecurityMode.None
        End With
        Dim Client As New SpectronClient(binding, epAddress)


        Console.WriteLine("Hi, this is the RBECtron client!")
        Client.Open()

        'Step 2: Call the service operations.
        'Call the Add service operation.
        Dim value1 As Double = 100D
        Dim value2 As Double = 15.99D
        Dim total_cycles As Double = 31500000
        total_cycles = 20250000
        Dim retVal As Integer

        'Call the Subtract service operation.
        value1 = 145D
        value2 = 76.54D
        Dim result As Double = Client.Add(value1, value2)
        Console.WriteLine("Add({0},{1}) = {2}", value1, value2, result)

        Console.WriteLine("Allocating NI_waveform array.")
        Client.allocate_NI_waveform(total_cycles)

        Console.WriteLine("Initialize 'NI_waveform' vector.")
        Client.initialize_data("/dev1/line0:31/", total_cycles)

        Console.WriteLine("Configuring PCIe-653x")
        'Client.configure_653x("/dev1/line0:31", Nothing, "OnboardClock", total_cycles)

        Console.WriteLine("Insert linear ramps on DIO23 and DIO27.")
        Client.AddRamp(5, 2, 0, 200, 8)
        'Client.AddRamp(5, 2, 0, 200, 4)
        'Client.AddRamp(2, 0, 210, 250, 8)
        'Client.AddRamp(2, 0, 210, 250, 4)

        Console.WriteLine("Insert sines on DIO30 and DIO26.")
        'Client.AddSine(2, 200, 2, 0, 200, 1)
        'Client.AddSine(2, 200, 2, 0, 200, 5)

        Console.WriteLine("Transpose 'NI_waveform' vector.")
        'Client.transpose_data()

        Console.WriteLine("Write 'NI_waveform' vector to PCIe-653x.")
        'retVal = Client.write_to_653x()

        Console.WriteLine()
        Console.WriteLine("Press <ENTER> to continue client.")
        Console.ReadLine()

        Console.WriteLine("Perform clean up.")
        Client.release_data()

        'Call the Subtract service operation.
        'value1 = 145D
        'value2 = 76.54D
        'Dim result As Double = Client.Add(value1, value2)
        'Console.WriteLine("Add({0},{1}) = {2}", value1, value2, result)

        ''Call the Multiply service operation.
        'value1 = 9D
        'value2 = 81.25D
        'result = Client.Multiply(value1, value2)
        'Console.WriteLine("Multiply({0},{1}) = {2}", value1, value2, result)

        ''Call the Divide service operation.
        'value1 = 22D
        'value2 = 7D
        'result = Client.Divide(value1, value2)
        'Console.WriteLine("Divide({0},{1}) = {2}", value1, value2, result)

        ' Step 3: Closing the client gracefully closes the connection and cleans up resources.
        Client.Close()

        Console.WriteLine()
        Console.WriteLine("Press <ENTER> to terminate client.")
        Console.ReadLine()

    End Sub

End Module
