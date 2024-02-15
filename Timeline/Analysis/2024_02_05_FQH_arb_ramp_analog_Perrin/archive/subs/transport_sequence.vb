Public Function AddTransportSequence(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal transport_en As Boolean) As Double

    ' take care of time t = 0 in first few digitaldata pulses

'********************************************* Begin Transport ********************************************************************************************
    Dim ST As Double = t_start
    Dim t_stop As Double = t_start + mot_load_time + 0.5 + MHT
    Dim transportSeqPath As String = "C:\\Users\\greinerlab\\Documents\\RbExpSoftware\\ExpControl\\mathematica\\"

    If transport_en = True Then
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
        
        'analogdata.AddFromTransportFile(transportSeqPath + "part1table1HR.txt", ST, ST + TT1, 60, 2, ps1_ao) 'Transport atoms to corner
        'analogdata.AddFromTransportFile(transportSeqPath + "part1table2HR.txt", ST, ST + TT1, 60, 2, ps2_ao)
        'analogdata.AddFromTransportFile(transportSeqPath + "part1table3HR.txt", ST, ST + TT1, 100, 2, ps3_ao)
        'analogdata.AddFromTransportFile(transportSeqPath + "part1table4HR.txt", ST, ST + TT1, 100, 2, ps4_ao)
        'analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table1HR.bin", ST, ST + TT1, ps1_ao) 'Transport atoms to corner
        'analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table2HR.bin", ST, ST + TT1, ps2_ao)
        'analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table3HR.bin", ST, ST + TT1, ps3_ao)
        'analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table4HR.bin", ST, ST + TT1, ps4_ao)
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table1HR_12.5MHz.bin", ST, ST + TT1, ps1_ao) 'Transport atoms to corner
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table2HR_12.5MHz.bin", ST, ST + TT1, ps2_ao)
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table3HR_12.5MHz.bin", ST, ST + TT1, ps3_ao)
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part1table4HR_12.5MHz.bin", ST, ST + TT1, ps4_ao)

        'analogdata.AddRamp(2.45, 2.45*1.5, ST + TT1, ST + TT1 + th, ps2_ao) 'Ramps up corner quadrupole.
        analogdata.AddRamp(2.45, 2.45*1.5, ST + TT1 - 0.4, ST + TT1 + th, ps2_ao) '2022/09/09 Test, 2022/09/12 Confirmed that actual DAC output is missing for about 0.4 ms, by Brice, Sooshin, and Perrin.
        'analogdata.AddFromTransportFile(transportSeqPath + "part2table1HR.txt", ST + TT1 + th, ST + TT + th, 60 / 1.5, 2, ps1_ao) 'Transport from corner to glass cell
        'analogdata.AddFromTransportFile(transportSeqPath + "part2table2HR.txt", ST + TT1 + th, ST + TT + th, 60 / 1.5, 2, ps2_ao) 'Probably th should be erased from this block
        'analogdata.AddFromTransportFile(transportSeqPath + "part2table3HR.txt", ST + TT1 + th, ST + TT + th, 100 / 1.5, 2, ps3_ao)
        'analogdata.AddFromTransportFile(transportSeqPath + "part2table4HR.txt", ST + TT1 + th, ST + TT + th, 100 / 1.5, 2, ps4_ao)
        'analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table1HR.bin", ST + TT1 + th, ST + TT + th, ps1_ao) 'Transport from corner to glass cell
        'analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table2HR.bin", ST + TT1 + th, ST + TT + th, ps2_ao) 'Probably th should be erased from this block
        'analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table3HR.bin", ST + TT1 + th, ST + TT + th, ps3_ao)
        'analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table4HR.bin", ST + TT1 + th, ST + TT + th, ps4_ao)
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table1HR_12.5MHz.bin", ST + TT1 + th, ST + TT + th, ps1_ao) 'Transport from corner to glass cell
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table2HR_12.5MHz.bin", ST + TT1 + th, ST + TT + th, ps2_ao) 'Probably th should be erased from this block
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table3HR_12.5MHz.bin", ST + TT1 + th, ST + TT + th, ps3_ao)
        analogdata.AddFromBinaryTransportFile(transportSeqPath + "part2table4HR_12.5MHz.bin", ST + TT1 + th, ST + TT + th, ps4_ao)
    '************************************************************ End Of Transport ***************************************************************************

        t_stop = ST + TT 'maybe I should include a +th?
    Else
        t_stop = t_start
    End If

    return t_stop
End Function
