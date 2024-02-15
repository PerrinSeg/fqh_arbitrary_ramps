Public Function AddPinningSequenceWithGauge(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal pinning_en As Boolean, _
    ByVal gauge_volt as Double) As Double()
'Pinning sequence and fluorescence imaging
'Returns pinning_end_time, pinning_ready_time, and molasses_start_time  since they are used to reference timing for MOSFETS etc.

	Dim pinning_start_time As Double = t_start
	Dim pinning_ready_time As Double = pinning_start_time + 1
	Dim molasses_start_time As Double = pinning_ready_time + pinning_ramp_up+1000
	Dim pinning_end_time As Double = pinning_start_time + pinning_hold_time
	Dim t_stop As Double
    
    Dim GaugeDur As Double = 500

    If pinning_en = True Then

'*****************************************************Shunt power supplies*****************************
digitaldata.AddPulse(ps5_shunt, pinning_ready_time, pinning_end_time)
digitaldata2.AddPulse(ps7_shunt, pinning_ready_time, pinning_end_time)
'********************************************************** Begin Pinning Lattice ********************************************************
digitaldata.AddPulse(axial795_ttl,pinning_start_time-5,pinning_end_time)
digitaldata.AddPulse(lattice2D795_ttl,pinning_start_time,pinning_end_time)
digitaldata2.AddPulse(lattice2D795_ttl2,pinning_start_time,pinning_end_time)
digitaldata2.AddPulse(axial795_shutter,pinning_start_time - 15,pinning_end_time)
digitaldata2.AddPulse(lattice2D795_shutter,pinning_start_time - 16.5,pinning_end_time)

analogdata.AddExp(pinning_axial, pinning_axial, pinning_start_time, pinning_ready_time, pinning_time_const, axial795_power)
analogdata.AddExp(pinning_2D1, pinning_2D1, pinning_start_time, pinning_ready_time, pinning_time_const, lattice2D795_power)
analogdata.AddExp(pinning_2D2, pinning_2D2, pinning_start_time, pinning_ready_time, pinning_time_const, lattice2D795_power2)

analogdata.AddStep(pinning_axial,pinning_ready_time,pinning_end_time,axial795_power)
analogdata.AddStep(pinning_2D1,pinning_ready_time,pinning_end_time,lattice2D795_power)
analogdata.AddStep(pinning_2D2,pinning_ready_time,pinning_end_time,lattice2D795_power2)
'********************************************************** End Pinning Lattice ***********************************************************

'********************************************************** Begin Imaging Molasses *********************************************************
digitaldata2.AddPulse(bfield_compensation, pinning_ready_time, pinning_end_time+30) 'bfield compensation on.
digitaldata2.AddPulse(bfield_compensation2,pinning_ready_time, pinning_end_time+30) 'bfield compensation on.
analogdata2.AddStep(small_galvo_dim,molasses_start_time-100,pinning_end_time,galvo_voltage_small)
analogdata2.AddStep(big_galvo_dim,molasses_start_time-100,pinning_end_time,galvo_voltage_big)
digitaldata.AddPulse(ttl_80MHz, molasses_start_time, pinning_end_time)    ' Molasses AOM 1
digitaldata.AddPulse(ttl_133MHz, molasses_start_time, pinning_end_time)   ' Molasses AOM2
'analogdata2.AddStep(quic_molasses_voltage_dim, molasses_start_time, pinning_end_time, quic_molasses_power) 
'digitaldata2.AddPulse(ttl_molasses1, molasses_start_time, pinning_end_time+100) ' Molasses Shifter 1
'digitaldata2.AddPulse(ttl_molasses2, molasses_start_time, pinning_end_time+100) ' Molasses Shifter 2
'analogdata.AddStep(axial_molasses_voltage_dim, molasses_start_time, pinning_end_time+100, axial_molasses_power)   
'digitaldata2.AddPulse(molasses_shaker, molasses_start_time, pinning_end_time+100) ' molasses shaker on quic retro-reflect mirror
'analogdata2.AddSineRamp(2.5, 2.5, 170, 170, molasses_start_time, pinning_end_time+100, molasses_shaker_axial)' molasses shaker on axial retro-reflect mirror
'digitaldata.AddPulse(quic_molasses_shutter, molasses_start_time-30, pinning_end_time+100) ' Molasses Shutter
'digitaldata.AddPulse(axial_molasses_shutter, molasses_start_time-30, pinning_end_time+100) ' Molasses Shutter
'digitaldata.AddPulse(ttl_78MHz, pinning_start_time-40, molasses_start_time)   ' Molasses Repumper
digitaldata2.AddPulse(gauge_shutter,molasses_start_time-30,pinning_end_time+100)
digitaldata2.AddPulse(gauge_ttl, molasses_start_time, pinning_end_time+100)



analogdata2.AddStep(0,pinning_start_time,molasses_start_time,gauge1_power)
analogdata2.AddStep(gauge_volt,molasses_start_time,molasses_start_time+GaugeDur,gauge1_power)
analogdata2.AddStep(0,molasses_start_time+GaugeDur,pinning_end_time+100,gauge1_power)


'********************************************************** End Imaging Molasses ***********************************************************
'*********************************************************** Begin Fluoresence Imaging ********************************************************************

digitaldata.AddPulse(ixon_camera,molasses_start_time+fluo_image_wait,molasses_start_time+fluo_image_wait+10)

        t_stop = pinning_end_time
    Else
        t_stop = t_start
        pinning_ready_time = t_start
        molasses_start_time = t_start
    End If

    Return {t_stop, pinning_ready_time, molasses_start_time}
End Function
