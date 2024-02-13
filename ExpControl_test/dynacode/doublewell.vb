Public Function AddDoublewell(ByVal t_start As Double, _
    ByVal dw_variables As Double(),
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal dw_en As Boolean) As Double
    
    Dim flopping_dur As Double = dw_variables(1)
    Dim isVertical As Double = dw_variables(2)
    Dim lattice_dw_depth As Double = dw_variables(3)
    Dim dw_DMD_volt1 As Double = dw_variables(4)
    Dim mod_freq_start As Double = dw_variables(5)
    Dim mod_freq_stop As Double = dw_variables(6)

    Dim t_stop As Double = t_start
    Dim latt_ramp_dur As Double = 0.5 'lattice ramp duration for analysis DW and after the quench
    Dim dw_start_time As Double = t_start

    'lattice1 high, dw1 to ramp up very quickly to deep
    Dim dw_rampup_start_time1 As Double = t_start + DMD_switch_dur   'added dw_wait_time for the inteferometer
    Dim dw_rampup_end_time1 As Double = dw_rampup_start_time1 + latt_ramp_dur 

    'lattice ramps down with dw at deep
    Dim lattice_down_start_time1 As Double = dw_rampup_end_time1
    Dim lattice_down_end_time1 As Double = lattice_down_start_time1 + latt_ramp_dur
    
    'atoms flopping in shallow lattice
    Dim conserv_rampup_start_time1 As Double = lattice_down_end_time1 + flopping_dur
    Dim conserv_rampup_end_time1 As Double = conserv_rampup_start_time1 + latt_ramp_dur

    Dim dw_final_turnoff_start_time1 As Double = conserv_rampup_end_time1
    Dim dw_final_turnoff_end_time1 As Double = dw_final_turnoff_start_time1 + latt_ramp_dur

    Dim dw_end_time As Double = dw_final_turnoff_end_time1

    Dim lattice_dw_volt As Double 
    If isVertical = 1 Then
        lattice_dw_volt = DepthToVolts(dw_2D_depth, lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
    Else 
        lattice_dw_volt = DepthToVolts(dw_2D_depth, lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
    End If

    If dw_en = True Then
        ' DMD
        analogdata2.AddRamp(line_DMD_start_volt, dw_DMD_volt1, dw_rampup_start_time1, dw_rampup_end_time1, line_DMD_power) 'ramp up DW
        analogdata2.AddStep(dw_DMD_volt1, dw_rampup_end_time1, dw_final_turnoff_start_time1, line_DMD_power)
        analogdata2.AddRamp(dw_DMD_volt1, line_DMD_start_volt, dw_final_turnoff_start_time1, dw_final_turnoff_end_time1, line_DMD_power)
        
        ' maybe need shutter
        digitaldata2.AddPulse(line_DMD_ttl, dw_rampup_start_time1, dw_final_turnoff_end_time1) 'for dw

        ' Lattices
        If isVertical = 1 Then
        	analogdata.AddStep(lattice1_max_volt, t_start, lattice_down_start_time1, lattice2D765_power)
        	analogdata.AddRamp(lattice1_max_volt, lattice_dw_volt, lattice_down_start_time1, lattice_down_end_time1, lattice2D765_power)
        	analogdata.AddSineRamp(lattice_dw_volt, lattice_mod_amp, mod_freq_start, mod_freq_stop, lattice_down_end_time1, conserv_rampup_start_time1, lattice2D765_power)
        	analogdata.AddRamp(lattice_dw_volt, lattice1_max_volt, conserv_rampup_start_time1, conserv_rampup_end_time1, lattice2D765_power)
        	analogdata.AddStep(lattice1_max_volt, conserv_rampup_end_time1, dw_end_time, lattice2D765_power)
        	
        	analogdata.AddStep(lattice2_max_volt, t_start, dw_end_time, lattice2D765_power2)
        Else
        	analogdata.AddStep(lattice2_max_volt, t_start, lattice_down_start_time1, lattice2D765_power2)
        	analogdata.AddRamp(lattice2_max_volt, lattice_dw_volt, lattice_down_start_time1, lattice_down_end_time1, lattice2D765_power2)
        	analogdata.AddSineRamp(lattice_dw_volt, lattice_mod_amp, mod_freq_start, mod_freq_stop, lattice_down_end_time1, conserv_rampup_start_time1, lattice2D765_power2)
        	analogdata.AddRamp(lattice_dw_volt, lattice2_max_volt, conserv_rampup_start_time1, conserv_rampup_end_time1, lattice2D765_power2)
        	analogdata.AddStep(lattice2_max_volt, conserv_rampup_end_time1, dw_end_time, lattice2D765_power2)
        	
        	analogdata.AddStep(lattice1_max_volt, t_start, dw_end_time, lattice2D765_power)
        End If

        ' B fields
	    analogdata.AddStep(dw_offset_quic, t_start, t_stop, ps6_ao)
	    analogdata.AddStep(dw_offset_quad, t_start, t_stop, ps8_ao)
    
        t_stop = dw_final_turnoff_end_time2
    Else
        t_stop = t_start
    End If

    Return t_stop
End Function
