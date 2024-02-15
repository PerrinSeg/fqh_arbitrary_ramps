Public Function AddTransportSequenceRewind(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal transport_en As Boolean, _
    ByVal transport_back_en As Boolean) As Double

'********************************************* Transport forward ********************************************************************************************

    Dim ST As Double = t_start
    Dim dur_transport As Double = TT + th
    'Dim t_stop_forward  As Double = ST + dur_transport + 500
    Dim t_stop_forward  As Double = ST + dur_transport
    Dim TT_aux As Double = TT
    Dim t_stop As Double
    Dim transportSeqPath As String = "C:\\Users\\greinerlab\\Documents\\RbExpSoftware\\ExpControl\\mathematica\\"
    Dim transportBackSeqPath As String = "Z:\\Brice\\Sequences\\Rewind transport\\TransportBinaryFileRewind\\"

    If transport_en = True Then

        'Digital channels
        digitaldata.AddPulse(push, ST, ST + t1) 'Push
        digitaldata.AddPulse(mot_high_current, ST, ST + t2) 'MOT
        digitaldata.AddPulse(transport_1 ,ST, ST + t3) 'T1
        digitaldata.AddPulse(transport_2, ST + t1, ST + t4) 'T2
        digitaldata.AddPulse(transport_3, ST + t2, ST + t5) 'T3
        digitaldata.AddPulse(transport_4, ST + t3, ST + t6) 'T4
        digitaldata.AddPulse(transport_5, ST + t4, ST + t7) 'T5
        digitaldata.AddPulse(transport_6, ST + t5, ST + t8) 'T6
        digitaldata.AddPulse(transport_7, ST + t6, ST + tc) 'T7
        digitaldata.AddPulse(transport_8, ST + t7, ST + t10 + th) 'T8
        digitaldata.AddPulse(transport_9, ST + tc + th, ST + t11 + th) 'T9
        digitaldata.AddPulse(transport_10, ST + t9 + th, ST + t12 + th) 'T10
        digitaldata.AddPulse(transport_11, ST + t10 + th, ST + t13 + th) 'T11
        digitaldata.AddPulse(transport_12, ST + t11 + th, ST + t14 + th) 'T12
        digitaldata.AddPulse(transport_13, ST + t12 + th, ST + t15 + th) 'T13
        digitaldata.AddPulse(quad_fet, ST + t13 + th, ST + t15 + th) 'Quad

        digitaldata.AddPulse(ps1_shunt, ST + t5, ST + t6) 'PS1 shunt
        digitaldata.AddPulse(ps2_shunt, ST + t1, ST + t3) 'PS2 shunt
        digitaldata.AddPulse(ps2_shunt, ST + t6, ST + t7) 'PS2 shunt
        digitaldata.AddPulse(ps2_shunt, ST + t10 + th, ST + t11 + th) 'PS2 shunt
        digitaldata.AddPulse(ps3_shunt, ST + t3, ST + t4) 'PS3 shunt
        digitaldata.AddPulse(ps3_shunt, ST + t7, ST + tc - 10 + th) 'PS3 shunt
        digitaldata.AddPulse(ps3_shunt, ST + t11 + th, ST + t12 + th) 'PS3 shunt
        digitaldata.AddPulse(ps4_shunt, ST + t4, ST + t5) 'PS4 shunt
        digitaldata.AddPulse(ps4_shunt, ST + t8, ST + t10 + th) 'PS4 shunt
        digitaldata.AddPulse(ps4_shunt, ST + t13 + th, ST + TT)
        digitaldata.AddPulse(ps5_shunt, ST, ST + TT)

        'Analog channels
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table1HR_12.5MHz.bin", ST, ST + TT1, ps1_ao) 'Transport atoms to corner
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table2HR_12.5MHz.bin", ST, ST + TT1, ps2_ao)
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table3HR_12.5MHz.bin", ST, ST + TT1, ps3_ao)
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table4HR_12.5MHz.bin", ST, ST + TT1, ps4_ao)
        analogdata.AddRamp(2.45, 2.45*1.5, ST + TT1, ST + TT1 + th, ps2_ao) 'Ramps up corner quadrupole.
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table1HR_12.5MHz.bin", ST + TT1 + th, ST + TT + th, ps1_ao) 'Transport from corner to glass cell
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table2HR_12.5MHz.bin", ST + TT1 + th, ST + TT + th, ps2_ao) 'Probably th should be erased from this block
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table3HR_12.5MHz.bin", ST + TT1 + th, ST + TT + th, ps3_ao)
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table4HR_12.5MHz.bin", ST + TT1 + th, ST + TT + th, ps4_ao)


        digitaldata.AddPulse(quad_fet, ST + t15 + th, t_stop_forward + dur_transport - t15 - th) 'Quad
        analogdata.AddStep(1.415, ST + t15 + th, t_stop_forward + dur_transport - t15 - th, ps1_ao)
        digitaldata.AddPulse(transport_13, ST + t15 + th, t_stop_forward + dur_transport - t15 - th) 'T13
        analogdata.AddStep(0.2895, ST + t15 + th, t_stop_forward + dur_transport - t15 - th, ps3_ao)

        '********************************************* Transport backward ********************************************************************************************

        If transport_back_en = True Then

            'Digital channels
            digitaldata.AddPulse(quad_fet, t_stop_forward + dur_transport - t15 - th, t_stop_forward + dur_transport - t13 - th) 'Quad
            digitaldata.AddPulse(transport_13, t_stop_forward + dur_transport - t15 - th, t_stop_forward + dur_transport - t12 - th) 'T13
            digitaldata.AddPulse(transport_12, t_stop_forward + dur_transport - t14 - th - 150, t_stop_forward + dur_transport - t11 - th) 'T12
            digitaldata.AddPulse(transport_11, t_stop_forward + dur_transport - t13 - th - 100, t_stop_forward + dur_transport - t10 - th) 'T11
            digitaldata.AddPulse(transport_10,  t_stop_forward + dur_transport - t12 - th - 100,  t_stop_forward + dur_transport - t9 - th) 'T10
            digitaldata.AddPulse(transport_9,  t_stop_forward + dur_transport - t11 - th - 100, t_stop_forward + dur_transport - tc - th) 'T9
            digitaldata.AddPulse(transport_8, t_stop_forward + dur_transport - t10 - th - 100, t_stop_forward + dur_transport - t7) 'T8            
            digitaldata.AddPulse(transport_7, t_stop_forward + dur_transport - tc, t_stop_forward + dur_transport - t6) 'T7
            digitaldata.AddPulse(transport_6, t_stop_forward + dur_transport - t8, t_stop_forward + dur_transport - t5) 'T6
            digitaldata.AddPulse(transport_5, t_stop_forward + dur_transport - t7, t_stop_forward + dur_transport - t4) 'T5
            digitaldata.AddPulse(transport_4, t_stop_forward + dur_transport - t6, t_stop_forward + dur_transport - t3) 'T4
            digitaldata.AddPulse(transport_3, t_stop_forward + dur_transport - t5, t_stop_forward + dur_transport - t2) 'T3
            digitaldata.AddPulse(transport_2, t_stop_forward + dur_transport - t4, t_stop_forward + dur_transport - t1) 'T2
            digitaldata.AddPulse(transport_1, t_stop_forward + dur_transport - t3 - 20, t_stop_forward + dur_transport) 'T1
            digitaldata.AddPulse(mot_high_current, t_stop_forward + dur_transport - t2,  t_stop_forward + dur_transport) 'MOT
            digitaldata.AddPulse(push, t_stop_forward + dur_transport - t1 - 20, t_stop_forward + dur_transport) 'Push
            
            digitaldata.AddPulse(ps1_shunt, t_stop_forward + dur_transport - t6, t_stop_forward + dur_transport - t5) 'PS1 shunt
            digitaldata.AddPulse(ps2_shunt, t_stop_forward + dur_transport - t11 - th, t_stop_forward + dur_transport - t10 - th) 'PS2 shunt
            digitaldata.AddPulse(ps2_shunt, t_stop_forward + dur_transport - t7, t_stop_forward + dur_transport - t6) 'PS2 shunt
            digitaldata.AddPulse(ps2_shunt, t_stop_forward + dur_transport - t3, t_stop_forward + dur_transport - t1) 'PS2 shunt
            digitaldata.AddPulse(ps3_shunt, t_stop_forward + dur_transport - t12 - th, t_stop_forward + dur_transport - t11 - th) 'PS3 shunt
            digitaldata.AddPulse(ps3_shunt, t_stop_forward + dur_transport - tc + 10 - th, t_stop_forward + dur_transport - t7) 'PS3 shunt
            digitaldata.AddPulse(ps3_shunt, t_stop_forward + dur_transport - t4, t_stop_forward + dur_transport - t3) 'PS3 shunt
            digitaldata.AddPulse(ps4_shunt, t_stop_forward + dur_transport - TT, t_stop_forward + dur_transport - t13 - th) 'PS4 shunt
            digitaldata.AddPulse(ps4_shunt, t_stop_forward + dur_transport - t10 - th, t_stop_forward + dur_transport - t8) 'PS4 shunt
            digitaldata.AddPulse(ps4_shunt, t_stop_forward + dur_transport - t5, t_stop_forward + dur_transport - t4) 'PS4 shunt
            digitaldata.AddPulse(ps5_shunt, t_stop_forward + dur_transport - TT, t_stop_forward + dur_transport) 'PS5 shunt

            'Analog channels
            analogdata.AddFromBinaryTransportFile(transportBackSeqPath + "part2table1HR_Rewind_12.5MHz.bin", t_stop_forward + dur_transport - TT - th, t_stop_forward + dur_transport - TT1 - th, ps1_ao) 'Transport from glass cell to corner
            analogdata.AddFromBinaryTransportFile(transportBackSeqPath + "part2table2HR_Rewind_12.5MHz.bin", t_stop_forward + dur_transport - TT - th, t_stop_forward + dur_transport - TT1 - th, ps2_ao)
            analogdata.AddFromBinaryTransportFile(transportBackSeqPath + "part2table3HR_Rewind_12.5MHz.bin", t_stop_forward + dur_transport - TT - th, t_stop_forward + dur_transport - TT1 - th, ps3_ao)
            analogdata.AddFromBinaryTransportFile(transportBackSeqPath + "part2table4HR_Rewind_12.5MHz.bin", t_stop_forward + dur_transport - TT - th, t_stop_forward + dur_transport - TT1 - th, ps4_ao)
            analogdata.AddRamp(2.45*1.5, 2.45, t_stop_forward + dur_transport - TT1 - th, t_stop_forward + dur_transport - TT1, ps2_ao) 'Ramps up corner quadrupole.
            analogdata.AddFromBinaryTransportFile(transportBackSeqPath + "part1table1HR_Rewind_12.5MHz.bin", t_stop_forward + dur_transport - TT1, t_stop_forward + dur_transport, ps1_ao) 'Transport atoms from corner to MOT Chamber
            analogdata.AddFromBinaryTransportFile(transportBackSeqPath + "part1table2HR_Rewind_12.5MHz.bin", t_stop_forward + dur_transport - TT1, t_stop_forward + dur_transport, ps2_ao)
            analogdata.AddFromBinaryTransportFile(transportBackSeqPath + "part1table3HR_Rewind_12.5MHz.bin", t_stop_forward + dur_transport - TT1, t_stop_forward + dur_transport, ps3_ao)
            analogdata.AddFromBinaryTransportFile(transportBackSeqPath + "part1table4HR_Rewind_12.5MHz.bin", t_stop_forward + dur_transport - TT1, t_stop_forward + dur_transport, ps4_ao)

            t_stop = t_stop_forward + TT + th

        Else
            
            t_stop = t_stop_forward
           
        End If

    Else

        t_stop = t_start

    End If

    return t_stop
    
End Function
