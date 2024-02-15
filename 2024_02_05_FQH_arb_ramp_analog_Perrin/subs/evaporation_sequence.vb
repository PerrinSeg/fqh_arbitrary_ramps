Public Function AddEvaporationSequence(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal evap_en As Boolean) As Double

    Dim transport_end_time As Double = t_start
    Dim quic_hold_time As Double = evaporation_time-quad_rampup_time-ioffe_creation_time
    Dim ioffe_ready_time As Double = transport_end_time + quad_rampup_time + ioffe_creation_time
    Dim evaporation_end_time As Double = ioffe_ready_time + quic_hold_time + 10
    
    Dim t_stop As Double = evaporation_end_time

    ' Things to Fix
    ' - dimple_ready_time
    ' - pinning_ready_time
    ' - pinning_end_time
    ' - ST + t13 + th
    
    If evap_en = True Then
        '********************************** Begin Evaporation ************************************
        'Ramp up quadrupole and final transport coil prior to ramping on ioffe coil
        analogdata.AddSmoothRamp(1.415, 0.8*2.342, _
            transport_end_time - 0.4, transport_end_time + quad_rampup_time, ps1_ao) 'Ramps quadrupole from final transport value to quic value(33.96->56.208)
        analogdata.AddSmoothRamp(.2895, 0.585, _
            transport_end_time - 0.4, transport_end_time + quad_rampup_time, ps3_ao) 'Ramps final transport(T13) to higher value to keep pace with quad rampup(11.58->23.4)
        
        'Ramps up T13 to proper value for Ioffe trap which is higher than in quadrupole configuration
        analogdata.AddSmoothRamp(.585, 0.875, _
            transport_end_time + quad_rampup_time, 
            ioffe_ready_time, ps3_ao) 'Ramps final transport(T13) to higher value to keep pace with quic rampup(23.4->35)
        analogdata.AddStep(0.875, ioffe_ready_time, _
            evaporation_end_time, ps3_ao) 'Holds T13 on during evaporation(make sure T13 MOSFET is on during this time.)
        'Ramps us into Ioffe trap from quadrupole trap
        
        Dim cigarwindup As Double = 1.35 '1.9
        analogdata.AddSmoothRamp(0.8*2.342, 0, transport_end_time + quad_rampup_time, ioffe_ready_time, ps1_ao)'Ramp Down PS1
        analogdata.AddSmoothRamp(0, 0.04, ioffe_ready_time + 200, ioffe_ready_time + 600, ps1_ao)
        analogdata.AddStep(0.04,ioffe_ready_time + 600, evaporation_end_time, ps1_ao)
        analogdata.AddStep(cigarwindup*0.04167,transport_end_time + quad_rampup_time - 200, transport_end_time + quad_rampup_time - 100, ps2_ao) 'fix windup problem
        analogdata.AddStep(0.004167, transport_end_time + quad_rampup_time - 100, transport_end_time + quad_rampup_time, ps2_ao)
        analogdata.AddSmoothRamp(0.00417, 0.8*2.342, transport_end_time + quad_rampup_time, ioffe_ready_time, ps2_ao) 'Ramp Up PS2
        
        'MOSFET Management for all QUIC trap coils and coils that control final position of atoms.
        digitaldata.AddPulse(offset_fet, transport_end_time + quad_rampup_time, evaporation_end_time) 'Offset FET
        digitaldata.AddPulse(bias_enable, transport_end_time - 1020, evaporation_end_time) 'Enables bias coils
        digitaldata.AddPulse(transport_13, transport_end_time, evaporation_end_time) 'Keeps T13 on through remainder of experiment
        digitaldata.AddPulse(ps5_shunt, transport_end_time, evaporation_end_time)
        digitaldata2.AddPulse(ps7_shunt, transport_end_time, evaporation_end_time)
        digitaldata.AddPulse(quad_fet, transport_end_time, ioffe_ready_time + 100) 'quadrupole mosfet
        digitaldata.AddPulse(ps1_shunt, ioffe_ready_time + 100, ioffe_ready_time + 200)
        digitaldata.AddPulse(quad_shim, ioffe_ready_time + 200, evaporation_end_time)
        digitaldata.AddPulse(ps4_shunt, transport_end_time, evaporation_end_time)
        analogdata.AddStep(0.8*2.342, ioffe_ready_time, evaporation_end_time, ps2_ao) 'Holds cigar trap on during evaporation
        
        'Evaporation
        digitaldata.AddPulse(evap_ttl, transport_end_time, transport_end_time + 10) 'Evaporation TTL Trigger
        digitaldata.AddPulse(evap_switch, transport_end_time, evaporation_end_time - 10) 'RF Switch FET
        
        'Resynch to 60 Hz
        digitaldata2.AddPulse(clock_resynch, evaporation_end_time - 9, evaporation_end_time - 6)
        analogdata.DisableClkDist(evaporation_end_time - 12, evaporation_end_time - 2.95)
        analogdata2.DisableClkDist(evaporation_end_time - 12, evaporation_end_time - 2.95)
        'analogdata3.DisableClkDist(evaporation_end_time - 12, evaporation_end_time - 2.95)

        'digitaldata2.AddPulse(clock_resynch, evaporation_end_time - 9, evaporation_end_time - 9 + resync_pulse_width)
        'analogdata.DisableClkDist(evaporation_end_time - 9 - 0.05,evaporation_end_time + resync_pulse_width - 9 + 0.05)
        'analogdata2.DisableClkDist(evaporation_end_time - 9 - 0.05,evaporation_end_time + resync_pulse_width - 9 + 0.05)

        t_stop = evaporation_end_time
    Else
        t_stop = t_start
    End If

    Return t_stop
End Function
