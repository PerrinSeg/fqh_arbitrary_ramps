Public Function AddDMDSequenceVerbose(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal dmd_en As Boolean, _
    ByVal isVert As Boolean, _
    ByVal isAnticonfine As Boolean, _
    ByVal dmdDur As Double, _
    ByVal dmdRampDur As Double, _
    ByVal lattRampDownDur As Double, _
    ByVal lattRampUpDur As Double, _
    ByVal latt_down_volt As Double, _
    ByVal dmd_up_volt As Double, _
    ByVal latt_start_volt As Double, _
    ByVal latt_end_volt As Double) As Double ()

    'some hard coded delay values
    Dim DMD_hw_delay As Double = -0.160
    Dim PID_response_dur As Double = 1
    Dim anticonfine_volt As Double = 2.3


    Dim dmd_ramp_dur As Double = dmdRampDur
    Dim latt_ramp_dur As Double = lattRampUpDur


	Dim dmd_turnon_start_time As Double  = t_start+2
    Dim dmd_turnon_end_time As Double = dmd_turnon_start_time + dmd_ramp_dur
    Dim latt_turnon_start_time As Double = dmd_turnon_end_time
    Dim latt_turnon_end_time As Double = latt_turnon_start_time + lattRampDownDur
    Dim latt_turnoff_start_time As Double = latt_turnon_end_time + dmdDur
    Dim latt_turnoff_end_time As Double = latt_turnoff_start_time + lattRampUpDur
    Dim dmd_turnoff_start_time As Double = latt_turnoff_end_time
    Dim dmd_turnoff_end_time As Double = dmd_turnoff_start_time + dmd_ramp_dur
    Dim last_time As Double = dmd_turnoff_end_time+2


	'Dim last_time =IT+3*guppy_interval+2000
	Dim t_stop As Double
    
    If dmd_en = True Then

        'just as a note. It is probably best to use "isVert" to decide which
        'axes to make code for and have the other direction actually do
        'nothing during this subroutine. You can stitch the lattice together
        'for the other direction outside of this subroutine if needed. 
        'Same for triggers since when patterns should change isn't necessarily
        'obvious to this subroutine

        If isVert = True Then
            'lattice part 2d1
            analogdata.AddStep(latt_start_volt,t_start,latt_turnon_start_time,lattice2D765_power)
            analogdata.AddRamp(latt_start_volt,latt_down_volt,latt_turnon_start_time,latt_turnon_end_time,lattice2D765_power)
            analogdata.AddStep(latt_down_volt,latt_turnon_end_time,latt_turnoff_start_time,lattice2D765_power)
            analogdata.AddRamp(latt_down_volt,latt_end_volt,latt_turnoff_start_time,latt_turnoff_end_time,lattice2D765_power)
            analogdata.AddStep(latt_end_volt,latt_turnoff_end_time,last_time,lattice2D765_power)

            'dmd part for vert
            digitaldata2.AddPulse(line_DMD_ttl, dmd_turnon_start_time - PID_response_dur/2, dmd_turnoff_end_time + PID_response_dur/2)

        	analogdata2.AddSmoothRamp(line_DMD_start_volt, dmd_up_volt, dmd_turnon_start_time, dmd_turnon_end_time, line_DMD_power)
        	analogdata2.AddStep(dmd_up_volt, dmd_turnon_end_time, dmd_turnoff_start_time, line_DMD_power)
        	analogdata2.AddSmoothRamp(dmd_up_volt, line_DMD_start_volt, dmd_turnoff_start_time, dmd_turnoff_end_time, line_DMD_power)
        Else
            'lattice part 2d2
            analogdata.AddStep(latt_start_volt,t_start,latt_turnon_start_time,lattice2D765_power2)
            analogdata.AddRamp(latt_start_volt,latt_down_volt,latt_turnon_start_time,latt_turnon_end_time,lattice2D765_power2)
            analogdata.AddStep(latt_down_volt,latt_turnon_end_time,latt_turnoff_start_time,lattice2D765_power2)
            analogdata.AddRamp(latt_down_volt,latt_end_volt,latt_turnoff_start_time,latt_turnoff_end_time,lattice2D765_power2)
            analogdata.AddStep(latt_end_volt,latt_turnoff_end_time,last_time,lattice2D765_power2)

            'dmd part for hor
            digitaldata2.AddPulse(hor_DMD_ttl, dmd_turnon_start_time - PID_response_dur/2, dmd_turnoff_end_time + PID_response_dur/2)

        	analogdata2.AddSmoothRamp(hor_DMD_start_volt, dmd_up_volt, dmd_turnon_start_time, dmd_turnon_end_time, hor_DMD_power)
        	analogdata2.AddStep(dmd_up_volt, dmd_turnon_end_time, dmd_turnoff_start_time, hor_DMD_power)
        	analogdata2.AddSmoothRamp(dmd_up_volt, hor_DMD_start_volt, dmd_turnoff_start_time, dmd_turnoff_end_time, hor_DMD_power)
        End If
        
        If isAnticonfine = True Then
            digitaldata.AddPulse(blue_dipole_shutter,dmd_turnon_start_time,dmd_turnoff_end_time)
            digitaldata2.AddPulse(blue_dipole_ttl,latt_turnon_start_time,latt_turnoff_end_time)

            analogdata.AddSmoothRamp(1,anticonfine_volt,latt_turnon_start_time,latt_turnon_end_time, red_dipole_power)
            analogdata.AddStep(anticonfine_volt,latt_turnon_end_time,latt_turnoff_start_time,red_dipole_power)
            analogdata.AddSmoothRamp(anticonfine_volt,1,latt_turnoff_start_time,latt_turnoff_end_time,red_dipole_power)
        End If
	
        t_stop = last_time
    Else
        t_stop = t_start
    End If

    Return {t_stop, latt_turnon_start_time}
End Function
