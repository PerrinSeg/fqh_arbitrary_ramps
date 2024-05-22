Public Function AddMOTSequenceTMP(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal MOT_en As Boolean) As Double

    Dim transport_start_time As Double = t_start + mot_load_time + 0.5 + MHT
    Dim t_stop As Double = t_start + mot_load_time + 0.5 + MHT

    Dim ramp_end_volt As Double = -0.5
    Dim molasses_ramp_dur As Double = 12
    'Dim blow_away_start_time As Double = 0.8
    
    If MOT_en = True Then
        'Load MOT and then do molasses
        digitaldata.AddPulse(mot_low_current, t_start, t_start + mot_load_time - molasses_time) 'MOT Current Supply
        digitaldata.AddPulse(ta_shutter, t_start, t_start + mot_load_time - shutter_offset_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
        digitaldata.AddPulse(repump_shutter, t_start, t_start + mot_load_time) 'Repump Shutter
        digitaldata2.AddPulse(mot_detuning, _
            t_start + mot_load_time - molasses_time - compressed_mot_time, _
            t_start + mot_load_time - molasses_time - compressed_mot_time + 25) 'MOT Detuning molasses
        
        analogdata2.AddRamp(0, ramp_end_volt, t_start + mot_load_time - molasses_time - compressed_mot_time, t_start + mot_load_time - molasses_time - compressed_mot_time + molasses_ramp_dur, ps9_ao)
        analogdata2.AddStep(ramp_end_volt, t_start + mot_load_time - molasses_time - compressed_mot_time + molasses_ramp_dur, t_start + mot_load_time, ps9_ao)
        analogdata2.AddSmoothRamp(ramp_end_volt, 0, t_start + mot_load_time, t_start + mot_load_time + 5, ps9_ao) 'Dummy; Just for smooth turning off. Anyway, by ttl_97MHz, it would be already turned off at t_start + mot_load_time.

        
        'Optical Pumping
        digitaldata.AddPulse(ttl_80MHz, t_start + mot_load_time, transport_start_time) '-80MHz TTL(Switches light to 0th order AOM)
        digitaldata.AddPulse(ttl_97MHz, t_start + mot_load_time, t_start + mot_load_time + 30) '97.5MHz TTL (Switches light to 0th order AOM)
        digitaldata.AddPulse(ttl_N133MHz, t_start + mot_load_time, t_start + mot_load_time + optical_pumping_time) ' -133MHz TTL(2->2 AOM)
        digitaldata.AddPulse(ttl_N78MHz, t_start + mot_load_time, t_start + mot_load_time + optical_pumping_time - 0.02) ' -78MHz TTL (1->1 AOM)
        digitaldata.AddPulse(ttl_78MHz, t_start + mot_load_time, t_start + mot_load_time + 60) '78MHz TTL to keep repump off until shutter closes
        digitaldata.AddPulse(optical_pumping, t_start + mot_load_time - 0.5, t_start + mot_load_time + 0.4) 'Optical Pumping B Field
        digitaldata.AddPulse(polarizer_shutter_11, t_start + mot_load_time - 60, t_start + mot_load_time + 30) 'Optical Pumping Shutter
        
        'Magnetic Trapping
        digitaldata.AddPulse(ps1_shunt, t_start, t_start + mot_load_time) 'PS1 Shunt
        'digitaldata.AddPulse(ps2_shunt, t_start, t_start + mot_load_time) 'PS2 Shunt
        'digitaldata.AddPulse(ps3_shunt, t_start, t_start + mot_load_time) 'PS3 Shunt
        'digitaldata.AddPulse(ps4_shunt, t_start, t_start + mot_load_time) 'PS4 Shunt

        digitaldata.AddPulse(ps2_shunt, t_start, t_stop) 'PS2 shunt
        digitaldata.AddPulse(ps3_shunt, t_start, t_stop) 'PS3 shunt
        digitaldata.AddPulse(ps4_shunt, t_start, t_stop) 'PS4 shunt

        'modify 0.2 to optical_pumping_time if scanning variable
        digitaldata.AddPulse(mot_high_current, t_start + mot_load_time + 0.2, transport_start_time) 'MOT High Current
        digitaldata2.AddPulse(cap_discharge, t_start + mot_load_time + 0.2, transport_start_time) 'Capacitor Discharge
        analogdata.AddExpAndRamp(2, 2, t_start + mot_load_time + 0.2, transport_start_time, 20, -30, 0, 1.33, ps1_ao)

        'Blow away atoms in f=2 if any
        digitaldata.AddPulse(ttl_N133MHz, mot_load_time+blow_away_start_time, mot_load_time+blow_away_start_time+10) ' -133MHz TTL(2->2 AOM)

        t_stop = transport_start_time
    Else
        t_stop = t_start
    End If

    Return t_stop
End Function
