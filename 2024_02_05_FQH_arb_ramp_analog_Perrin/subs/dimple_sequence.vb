Public Function AddLoadDimpleSequence(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal MI_en As Boolean) As Double


'Sequence from evaporation_end_time to axial_ready_time
'returns end time and final voltages for lattices and dipole
'*********************************************************************************************************************************************************

'************************************************************ Calculate time for 2D ramp ************************************************************

'************************************************************ Definition of Time Variables ***************************************************************
Dim cigar_move_end_time As Double = t_start + cigar_move_time
Dim vertical_move_end_time As Double = cigar_move_end_time + vertical_move
Dim dimple_start_time As Double = vertical_move_end_time+1.5*biglattice_ramp_on+spherical_creation_time
Dim dimple_ready_time As Double = dimple_start_time + dimple_ramp_on
Dim bfield_off_time As Double = dimple_ready_time + bfield_off	
Dim axial_start_time As Double = bfield_off_time + dimple_delay
Dim axial_ready_time As Double = axial_start_time + axial_ramp_on
Dim big_lattice_off_time As Double = axial_ready_time + big_lattice_rampdown	
Dim t_stop As Double
    
If MI_en = True Then

'************************************* patching MOSFETS from evaporation sequence *************************************
analogdata.AddStep(0.04,t_start, dimple_ready_time, ps1_ao)

analogdata.AddStep(2.342, t_start, dimple_ready_time, ps2_ao) 'Holds cigar trap on during evaporation

digitaldata.AddPulse(offset_fet, t_start, big_lattice_off_time) 'Offset FET
digitaldata.AddPulse(bias_enable, t_start, big_lattice_off_time) 'Enables bias coils
digitaldata.AddPulse(transport_13, t_start, big_lattice_off_time) 'Keeps T13 on through remainder of experiment
digitaldata.AddPulse(ps5_enable, t_start, big_lattice_off_time)
digitaldata2.AddPulse(ioffe_mirror_fet, t_start, big_lattice_off_time)
digitaldata.AddPulse(quad_shim, t_start, big_lattice_off_time)
digitaldata2.AddPulse(single_quad_shim, bfield_off_time, big_lattice_off_time)
digitaldata.AddPulse(ps4_shunt, t_start, big_lattice_off_time)
digitaldata.AddPulse(imaging_coil, t_start, big_lattice_off_time)

'*********************************************** begin xyz movement of atoms in cigar ************************************************
analogdata2.AddSmoothRamp(0,quic_push*ps5_scaler, t_start, t_start + cigar_move_time, ps5_ao)
analogdata2.AddStep(quic_push*ps5_scaler, t_start+ cigar_move_time, dimple_ready_time, ps5_ao)

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
analogdata2.AddStep(biglattice_galvo_volt, cigar_move_end_time-1000, big_lattice_off_time, biglattice_galvo)

digitaldata2.AddPulse(big_lattice_ttl, cigar_move_end_time-1, big_lattice_off_time)
digitaldata2.AddPulse(big_lattice_shutter, cigar_move_end_time-40, big_lattice_off_time)

analogdata.AddSmoothRamp(1.9, biglattice_midPower, _
    cigar_move_end_time, cigar_move_end_time + biglattice_ramp_on, big_lattice_power)
analogdata.AddStep(biglattice_midPower, _
    cigar_move_end_time + biglattice_ramp_on, cigar_move_end_time + biglattice_ramp_on + spherical_creation_time, big_lattice_power)
analogdata.AddSmoothRamp(biglattice_midPower, biglattice_maxPower, _
    cigar_move_end_time + biglattice_ramp_on+spherical_creation_time, cigar_move_end_time + 1.5*biglattice_ramp_on + spherical_creation_time, big_lattice_power)
analogdata.AddStep(biglattice_maxPower, cigar_move_end_time + 1.5*biglattice_ramp_on + spherical_creation_time, axial_ready_time, big_lattice_power)
analogdata.AddSmoothRamp(biglattice_maxPower, 1.9, axial_ready_time, big_lattice_off_time, big_lattice_power)

'conversion to spherical trap here
analogdata.AddSmoothRamp(0, siphoned_current/16.0, cigar_move_end_time+biglattice_ramp_on,cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,bias_siphon)
analogdata.AddStep(siphoned_current/16.0, cigar_move_end_time+biglattice_ramp_on+spherical_creation_time,dimple_ready_time,bias_siphon)


'Ioffe mirror is ramped during conversion to spherical trap to offset the shift in trap location
analogdata.AddSmoothRamp(ps6_scaler*quic_push*quic_mirror_ratio/16.0 + ps6_offset, _
    ps6_scaler*quic_axis_offset + ps6_offset, cigar_move_end_time + biglattice_ramp_on, _
    cigar_move_end_time + biglattice_ramp_on + spherical_creation_time, ps6_ao)
analogdata.AddStep(ps6_scaler*quic_axis_offset + ps6_offset,
    cigar_move_end_time + biglattice_ramp_on + spherical_creation_time, dimple_ready_time, ps6_ao)
'ps1 is used to offset the shift in quad axis position
analogdata.AddSmoothRamp(quad_axis_position, quad_axis_offset, _
    cigar_move_end_time + biglattice_ramp_on, cigar_move_end_time + biglattice_ramp_on + spherical_creation_time, ps1_ao)
analogdata.AddStep(quad_axis_offset, cigar_move_end_time + biglattice_ramp_on + spherical_creation_time,dimple_ready_time,ps1_ao)

'****************************************Axial lattice***********************************************
digitaldata.AddPulse(ttl_axial_lattice, axial_start_time-1, big_lattice_off_time)
digitaldata.AddPulse(axial_lattice_shutter, axial_start_time-50, big_lattice_off_time)

analogdata.AddSmoothRamp(1.76,intermediate_axial_voltage, axial_start_time, axial_ready_time, axial_lattice_power)
analogdata.AddSmoothRamp(intermediate_axial_voltage, axial_voltage, axial_ready_time, big_lattice_off_time, axial_lattice_power)

'******************************************** beginning of dimple ramp on ****************************
digitaldata2.AddPulse(round_dimple_shutter, dimple_start_time-20, big_lattice_off_time)
digitaldata2.AddPulse(round_dimple_ttl, dimple_start_time-1, big_lattice_off_time)


analogdata2.AddSmoothRamp(1.7, round_dimple_voltage, dimple_start_time, dimple_ready_time, round_dimple_power)
analogdata2.AddStep(round_dimple_voltage, dimple_ready_time, big_lattice_off_time, round_dimple_power)
'******************************************** ramp down bfields ***********************************************************************
'PS 1
analogdata.AddSmoothRamp(quad_axis_offset, 0, dimple_ready_time, bfield_off_time, ps1_ao)
analogdata.AddSmoothRamp(0,quad_gravity_offset, bfield_off_time, axial_start_time, ps1_ao)
analogdata.AddStep(quad_gravity_offset, axial_start_time, big_lattice_off_time, ps1_ao)
'PS 2
analogdata.AddSmoothRamp(2.342, 0, dimple_ready_time, bfield_off_time, ps2_ao)'Ramp down PS2
'PS 3
analogdata.AddSmoothRamp(0.03,0,dimple_ready_time, bfield_off_time, ps3_ao)'Ramp down PS3
'PS 5
analogdata2.AddSmoothRamp(quic_push*ps5_scaler,0, dimple_ready_time, bfield_off_time, ps5_ao)'Ramp down PS5
'PS 6
analogdata.AddSmoothRamp(ps6_scaler*quic_axis_offset + ps6_offset, ps6_offset, dimple_ready_time, bfield_off_time, ps6_ao)'Ramp down PS6
analogdata.AddSmoothRamp(0 + ps6_offset, ps6_scaler*quic_gravity_offset + ps6_offset, bfield_off_time, axial_start_time, ps6_ao)
analogdata.AddStep(ps6_scaler*quic_gravity_offset + ps6_offset, axial_start_time, big_lattice_off_time, ps6_ao)
'PS 7
analogdata.AddSmoothRamp(black_coil_zed*ps7_scaler,0, dimple_ready_time, bfield_off_time, ps7_ao)
'Siphon
analogdata.AddSmoothRamp(siphoned_current/16.0,0, dimple_ready_time, bfield_off_time, bias_siphon)

'*********************************************End of Mott Inuslator Sequence****************************************

'On at the end of the sequence
'beams: 2D1, 2D2, axial, red dipole
'coils: quad_gravity, quic_gravity

	t_stop = big_lattice_off_time
Else
    t_stop = t_start
End If

    Return t_stop
End Function
