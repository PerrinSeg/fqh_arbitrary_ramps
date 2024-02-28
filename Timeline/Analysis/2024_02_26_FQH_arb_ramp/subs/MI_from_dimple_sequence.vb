Public Function AddMottInsulatorFromDimpleSequence(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal MI_en As Boolean) As Double ()


'Sequence from evaporation_end_time to twodmax_ready_time
'returns end time and final voltages for lattices and dipole
'*********************************************************************************************************************************************************

'************************************************************ Calculate time for 2D ramp ************************************************************

Dim lattice1_midpoint_volt As Double = lattice1_voltage_offset + lattice1_calib_volt+.5*Log10(twod_ramp_midpoint/lattice1_calib_depth)
Dim lattice2_midpoint_volt As Double = lattice2_voltage_offset + lattice2_calib_volt+.5*Log10(twod_ramp_midpoint/lattice2_calib_depth)

Dim lattice1_endpoint_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(twod_ramp_endpoint/lattice1_calib_depth)
Dim lattice2_endpoint_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(twod_ramp_endpoint/lattice2_calib_depth)

Dim lattice1_max_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(lattice1_max/lattice1_calib_depth)
Dim lattice2_max_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(lattice2_max/lattice2_calib_depth)

Dim lattice1_start_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(lattice1_start_depth/lattice1_calib_depth)
Dim lattice2_start_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(lattice2_start_depth/lattice2_calib_depth)

Dim twod_chopped_rampup As Double
If lattice1_max > lattice2_max Then
twod_chopped_rampup = (lattice1_max_volt - lattice1_midpoint_volt)/(lattice1_endpoint_volt - lattice1_midpoint_volt)*twodmax_rampup
Else
twod_chopped_rampup = (lattice2_max_volt - lattice2_midpoint_volt)/(lattice2_endpoint_volt - lattice2_midpoint_volt)*twodmax_rampup
End If
' Time constant is 54ms in base e, 125ms in base 10
'Dim dipole_voltage As Double
Dim end_dipole_voltage As Double
'************************************************************ Definition of Time Variables ***************************************************************
Dim cigar_move_end_time As Double = t_start + cigar_move_time
Dim vertical_move_end_time As Double = cigar_move_end_time + vertical_move
Dim dimple_start_time As Double = vertical_move_end_time+1.5*biglattice_ramp_on+spherical_creation_time
Dim dimple_ready_time As Double = dimple_start_time + dimple_ramp_on
Dim bfield_off_time As Double = dimple_ready_time + bfield_off	
Dim axial_start_time As Double = bfield_off_time + dimple_delay
Dim axial_ready_time As Double = axial_start_time + axial_ramp_on
Dim dipole_start_time As Double = axial_ready_time + big_lattice_rampdown	
Dim dipole_ready_time As Double = dipole_start_time + dimple_compression_ramp
Dim twod_start_time As Double = dipole_ready_time
Dim twodmax_ready_time As Double = twod_start_time + twod_chopped_rampup+bandgap_open_rampup
Dim dimple_ramp_down_start_time As Double = twodmax_ready_time
Dim dimple_ramp_down_end_time As double = dimple_ramp_down_start_time + 100
Dim t_stop As Double
    
If MI_en = True Then

'************************************* patching MOSFETS from evaporation sequence *************************************
analogdata.AddStep(0.04,t_start, dimple_ready_time, ps1_ao)

analogdata.AddStep(2.342, t_start, dimple_ready_time, ps2_ao) 'Holds cigar trap on during evaporation

digitaldata.AddPulse(offset_fet, t_start, twodmax_ready_time) 'Offset FET
digitaldata.AddPulse(bias_enable, t_start, twodmax_ready_time) 'Enables bias coils
digitaldata.AddPulse(transport_13, t_start, twodmax_ready_time) 'Keeps T13 on through remainder of experiment
digitaldata.AddPulse(ps5_enable, t_start, twodmax_ready_time)
digitaldata2.AddPulse(ioffe_mirror_fet, t_start, twodmax_ready_time)
digitaldata.AddPulse(quad_shim, t_start, twodmax_ready_time)
digitaldata2.AddPulse(single_quad_shim, bfield_off_time, twodmax_ready_time)
digitaldata.AddPulse(ps4_shunt, t_start, twodmax_ready_time)
digitaldata.AddPulse(imaging_coil, t_start, twodmax_ready_time)

'*********************************************** begin xyz movement of atoms in cigar ************************************************
analogdata.AddSmoothRamp(0,quic_push*ps5_scaler, t_start, t_start + cigar_move_time, ps5_ao)
analogdata.AddStep(quic_push*ps5_scaler, t_start+ cigar_move_time, dimple_ready_time, ps5_ao)

analogdata.AddStep(0, t_start - 4, t_start, ps6_ao)
analogdata.AddSmoothRamp(0, ps6_scaler*(quic_push*quic_mirror_ratio/16.0) + ps6_offset, t_start, t_start + cigar_move_time,ps6_ao)
analogdata.AddStep(ps6_scaler*(quic_push*quic_mirror_ratio/16.0) + ps6_offset, t_start + cigar_move_time, cigar_move_end_time + biglattice_ramp_on,ps6_ao)

analogdata.AddSmoothRamp(0.04, quad_axis_position, t_start, t_start + cigar_move_time, ps1_ao)
analogdata.AddStep(quad_axis_position, t_start + cigar_move_time, cigar_move_end_time + biglattice_ramp_on, ps1_ao)
analogdata.AddSmoothRamp(.875, 0.03, t_start, t_start + cigar_move_time, ps3_ao)
analogdata.AddStep(0.03, t_start + cigar_move_time, dimple_ready_time, ps3_ao)


analogdata.AddSmoothRamp(0, black_coil_zed*ps7_scaler, t_start, t_start + cigar_move_time, ps7_ao)
analogdata.AddSmoothRamp(black_coil_zed*ps7_scaler, spherical_black_coil*ps7_scaler, t_start + cigar_move_time, vertical_move_end_time, ps7_ao)
analogdata.AddStep(spherical_black_coil*ps7_scaler, vertical_move_end_time, dimple_ready_time, ps7_ao)

'*************************************beginning of big lattice ramp on*****************************************************************
analogdata2.AddStep(biglattice_galvo_volt, cigar_move_end_time-1000, dimple_ramp_down_end_time, biglattice_galvo)

digitaldata2.AddPulse(big_lattice_ttl, cigar_move_end_time-1, dipole_start_time+1)
digitaldata2.AddPulse(big_lattice_shutter, cigar_move_end_time-40, dipole_start_time)
analogdata.AddSmoothRamp(1.9,biglattice_midPower, cigar_move_end_time, cigar_move_end_time+biglattice_ramp_on, big_lattice_power)
analogdata.AddStep(biglattice_midPower,cigar_move_end_time+biglattice_ramp_on,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,big_lattice_power)
analogdata.AddSmoothRamp(biglattice_midPower,biglattice_maxPower, cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,cigar_move_end_time+1.5*biglattice_ramp_on+spherical_creation_time, big_lattice_power)
analogdata.AddStep(biglattice_maxPower, cigar_move_end_time+1.5*biglattice_ramp_on+spherical_creation_time, axial_ready_time, big_lattice_power)
analogdata.AddSmoothRamp(biglattice_maxPower,1.9, axial_ready_time, dipole_start_time, big_lattice_power)

'conversion to spherical trap here
analogdata.AddSmoothRamp(0,siphoned_current/16.0,cigar_move_end_time+biglattice_ramp_on,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,bias_siphon)
analogdata.AddStep(siphoned_current/16.0,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,dimple_ready_time,bias_siphon)


'Ioffe mirror is ramped during conversion to spherical trap to offset the shift in trap location
analogdata.AddSmoothRamp(ps6_scaler*quic_push*quic_mirror_ratio/16.0 + ps6_offset, _
    ps6_scaler*quic_axis_offset + ps6_offset, cigar_move_end_time + biglattice_ramp_on, _
    cigar_move_end_time + biglattice_ramp_on + spherical_creation_time,ps6_ao)
analogdata.AddStep(ps6_scaler*quic_axis_offset + ps6_offset,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,dimple_ready_time,ps6_ao)
'ps1 is used to offset the shift in quad axis position
analogdata.AddSmoothRamp(quad_axis_position,quad_axis_offset,cigar_move_end_time+biglattice_ramp_on,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time, ps1_ao)
analogdata.AddStep(quad_axis_offset,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,dimple_ready_time,ps1_ao)

'****************************************Axial lattice***********************************************
digitaldata.AddPulse(ttl_axial_lattice, axial_start_time-1,dimple_ramp_down_end_time)
digitaldata.AddPulse(axial_lattice_shutter, axial_start_time-50, dimple_ramp_down_end_time)

analogdata.AddSmoothRamp(1.76,intermediate_axial_voltage,axial_start_time,axial_ready_time,axial_lattice_power)
analogdata.AddStep(intermediate_axial_voltage,axial_ready_time,dipole_ready_time,axial_lattice_power)
analogdata.AddSmoothRamp(intermediate_axial_voltage,axial_voltage,dipole_ready_time,twod_start_time,axial_lattice_power)
analogdata.AddStep(axial_voltage,twod_start_time,dimple_ramp_down_end_time,axial_lattice_power)

'******************************************** beginning of dimple ramp on ****************************
digitaldata2.AddPulse(round_dimple_shutter, dimple_start_time-20, dimple_ramp_down_end_time)
digitaldata2.AddPulse(round_dimple_ttl, dimple_start_time-1,dimple_ramp_down_end_time)


analogdata2.AddSmoothRamp(1.7, round_dimple_voltage, dimple_start_time, dimple_ready_time, round_dimple_power)
analogdata2.AddStep(round_dimple_voltage,dimple_ready_time, dimple_ramp_down_start_time, round_dimple_power)
analogdata2.AddSmoothRamp(round_dimple_voltage, compressed_dimple_voltage, dipole_start_time, dipole_ready_time, round_dimple_power)
analogdata2.AddStep(compressed_dimple_voltage, dipole_ready_time, dimple_ramp_down_start_time, round_dimple_power)
analogdata2.AddSmoothRamp(compressed_dimple_voltage,1.7,dimple_ramp_down_start_time,dimple_ramp_down_end_time, round_dimple_power)

'******************************************** ramp down bfields ***********************************************************************
'PS 1
analogdata.AddSmoothRamp(quad_axis_offset,0,dimple_ready_time,bfield_off_time, ps1_ao)
analogdata.AddSmoothRamp(0,quad_gravity_offset,bfield_off_time,axial_start_time,ps1_ao)
analogdata.AddStep(quad_gravity_offset,axial_start_time,dimple_ramp_down_end_time,ps1_ao)
'PS 2
analogdata.AddSmoothRamp(2.342,0,dimple_ready_time,bfield_off_time, ps2_ao)'Ramp down PS2
'PS 3
analogdata.AddSmoothRamp(0.03,0,dimple_ready_time,bfield_off_time, ps3_ao)'Ramp down PS3
'PS 5
analogdata.AddSmoothRamp(quic_push*ps5_scaler,0,dimple_ready_time,bfield_off_time, ps5_ao)'Ramp down PS5
'PS 6
analogdata.AddSmoothRamp(ps6_scaler*quic_axis_offset + ps6_offset,0 + ps6_offset, dimple_ready_time, bfield_off_time, ps6_ao)'Ramp down PS6
analogdata.AddSmoothRamp(0 + ps6_offset, ps6_scaler*quic_gravity_offset + ps6_offset, bfield_off_time, axial_start_time, ps6_ao)
analogdata.AddStep(ps6_scaler*quic_gravity_offset + ps6_offset,axial_start_time,dimple_ramp_down_end_time,ps6_ao)
'PS 7
analogdata.AddSmoothRamp(black_coil_zed*ps7_scaler,0,dimple_ready_time,bfield_off_time, ps7_ao)
'Siphon
analogdata.AddSmoothRamp(siphoned_current/16.0,0,dimple_ready_time,bfield_off_time,bias_siphon)

'************************************************************** conservative 2D lattice rampup *********************************

digitaldata2.AddPulse(lattice2D765_ttl,twod_start_time-1, dimple_ramp_down_end_time)
digitaldata2.AddPulse(lattice2D765_ttl2,twod_start_time-1,  dimple_ramp_down_end_time)
digitaldata2.AddPulse(lattice2D765_shutter,twod_start_time-20, dimple_ramp_down_end_time)
digitaldata2.AddPulse(lattice2D765_shutter2,twod_start_time-20, dimple_ramp_down_end_time)

analogdata.AddLogRamp(lattice1_start_volt,lattice1_midpoint_volt,twod_start_time,twod_start_time+bandgap_open_rampup,lattice2D765_power)
analogdata.AddLogRamp(lattice2_start_volt,lattice2_midpoint_volt,twod_start_time,twod_start_time+bandgap_open_rampup,lattice2D765_power2)
analogdata.AddRamp(lattice1_midpoint_volt,lattice1_max_volt,twod_start_time+bandgap_open_rampup,twodmax_ready_time,lattice2D765_power)
analogdata.AddRamp(lattice2_midpoint_volt,lattice2_max_volt,twod_start_time+bandgap_open_rampup,twodmax_ready_time,lattice2D765_power2)
analogdata.AddStep(lattice1_max_volt,twodmax_ready_time,dimple_ramp_down_end_time,lattice2D765_power)
analogdata.AddStep(lattice2_max_volt,twodmax_ready_time,dimple_ramp_down_end_time,lattice2D765_power2)
'*********************************************End of Mott Inuslator Sequence****************************************

'On at the end of the sequence
'beams: 2D1, 2D2, axial, red dipole
'coils: quad_gravity, quic_gravity

	t_stop = dimple_ramp_down_end_time
Else
        t_stop = t_start
        end_dipole_voltage = 1.0
        
End If

    Return {t_stop, lattice1_max_volt, lattice2_max_volt, end_dipole_voltage}
End Function
