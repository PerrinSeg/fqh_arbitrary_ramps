'===================
'=====Variables=====
quic_init = 0.5
quad_init = 0
dur_idx = 1
var_idx = 4
var_value = 3.66
dur_value = 75.981
'=====Variables=====

Dim rampSegPath As String = "C:\\Users\\Rb Lab\\Documents\\experiments\\2024_01_08_FQH_ramp_test\\ramp_segments.txt"
Dim n_variables As Integer = 4

Dim lattice1_max As Double = 45
Dim lattice2_max As Double = 45
Dim lattice1_max_volt As Double = lattice1_voltage_offset+lattice1_calib_volt+.5*Log10(lattice1_max/lattice1_calib_depth)
Dim lattice2_max_volt As Double = lattice2_voltage_offset+lattice2_calib_volt+.5*Log10(lattice2_max/lattice2_calib_depth)

'----------------------------------------------------- Time Definitions for Sequence --------------------------------------------------------------

'(0) cutting
Dim cutting_dur As Double = 1000
Dim cutting_start_time As Double = 0
Dim cutting_end_time As Double = cutting_start_time + cutting_dur 

'(1) Turn on quic and quad gradients
' Turn on quic gradient
Dim coil_ramp_dur As Double = 100
Dim quic_turnon_start_time As Double = cutting_end_time
Dim quic_turnon_end_time As Double = quic_turnon_start_time + coil_ramp_dur ' raise from 0 to quic_init
' Turn on quad gradient
Dim quad_turnon_start_time As Double = cutting_end_time
Dim quad_turnon_end_time As Double = quic_turnon_end_time ' raise from 0 to quad_init

'(2) Arbitrary ramp
Dim ramp_start_time As Double = quic_turnon_end_time 'ramp delocalizes along y first, so it can start as soon as y tilt is ramped up

'Dim ramp_variables = {New Double() {0, 3000, 3000, 3000}, New Double() {0, 4, 1, 4}, New Double() {0, 4, 2, 3}, New Double() {0, 4, 2, 3}, New Double() {0, 2, 1, 2}}
Dim ramp_variables = LoadRampSegmentsFromFile(rampSegPath, n_variables)
Dim n_times As Integer = ramp_variables(0).GetUpperBound(0)
Console.WriteLine("N times: {0}", n_times)

' overwrite appropriate variables
If dur_idx <= n_times Then
    ramp_variables(0)(dur_idx) = dur_value
    If var_idx <= n_variables Then
        ramp_variables(var_idx)(dur_idx) = var_value
    Else
        Console.WriteLine("Bad value for var_idx = {0}, must be <= {1}. Default voltage value will not be overwritten.", var_idx, n_variables)
        'Microsoft.VisualBasic.Interaction.MsgBox("Bad value for var_idx = {0}, must be <= {1}. Default voltage value will not be overwritten.", var_idx, n_variables)
    End If
Else
    Console.WriteLine("Bad value for dur_idx = {0}, must be <= {1}. Default values will not be overwritten.", dur_idx, n_times)
    'Microsoft.VisualBasic.Interaction.MsgBox("Bad value for dur_idx = {0}, must be <= {1}. Default values will not be overwritten.", dur_idx, n_times)
End If


Dim ramp_durs As Double() = ramp_variables(0)
Dim ramp_t(n_times) As Double
ramp_t(0) = ramp_start_time
For index As Integer = 1 To n_times
ramp_t(index) = ramp_t(index - 1) + ramp_durs(index)
'Console.WriteLine("ramp_t({0}) = {1}", index, ramp_t(index))
Next
Dim ramp_end_time As Double = ramp_t(n_times)

Dim lattice1_ramp_v(n_times) As Double
Dim lattice2_ramp_v(n_times) As Double
For index As Integer = 0 To n_times
    lattice1_ramp_v(index) = DepthToVolts(ramp_variables(1)(index), lattice1_calib_depth, lattice1_calib_volt, lattice1_voltage_offset)
    lattice2_ramp_v(index) = DepthToVolts(ramp_variables(2)(index), lattice2_calib_depth, lattice2_calib_volt, lattice2_voltage_offset)
Next
Dim quad_ramp_v As Double() = ramp_variables(3)  ' 0th value is "quad_init"
Dim quic_ramp_v As Double() = ramp_variables(4)  ' 0th value is "quic_init"

'(3) pinning
Dim freeze_lattice_ramp_dur As Double = 100
Dim pinning_dur As Double = 15000
Dim pinning_start_time As Double = ramp_end_time + freeze_lattice_ramp_dur
Dim pinning_end_time As Double = pinning_start_time + pinning_dur
Dim IT As Double = pinning_end_time


'---------------------------------------------------------------------------------------------------------------------------------------------

analogdata.DisableClkDist(0.95, 4.05)

'----------------------------------------------------- (0) Cutting ---------------------------------------------------------------------------

'Ramp lattice power to starting positions
' 2D1
analogdata.AddStep(lattice1_max_volt, cutting_start_time, ramp_start_time, lattice2D765_power)
' 2D2
analogdata.AddStep(lattice2_max_volt, cutting_start_time, ramp_start_time, lattice2D765_power2)

'----------------------------------------------------- (1) Turn on gradients -----------------------------------------------------------------

' quic
analogdata.AddSmoothRamp(0, quic_init, quic_turnon_start_time, quic_turnon_end_time, ps1_ao) 'ps6_ao
' quad
analogdata.AddRamp(0, quad_init, quad_turnon_start_time, quad_turnon_end_time, ps3_ao) 'ps8_ao

'----------------------------------------------------- (2) Adiabatic Ramps -------------------------------------------------------------------

'Linear interpolation to connect the ramp end points

'2D1
analogdata.AddLogRamp(lattice1_max_volt, lattice1_ramp_v(1), ramp_start_time, ramp_t(1), lattice2D765_power)
For index As Integer = 1 To n_times - 1
    analogdata.AddLogRamp(lattice1_ramp_v(index), lattice1_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), lattice2D765_power)
Next

'2D2
analogdata.AddLogRamp(lattice2_max_volt, lattice2_ramp_v(1), ramp_start_time, ramp_t(1), lattice2D765_power2)
For index As Integer = 1 To n_times - 1
    analogdata.AddLogRamp(lattice2_ramp_v(index), lattice2_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), lattice2D765_power2)
Next

'quic
analogdata.AddRamp(quic_init, quic_ramp_v(1), ramp_start_time, ramp_t(1), ps1_ao) 'ps6_ao
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(quic_ramp_v(index), quic_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), ps1_ao) 'ps6_ao
Next

'quad
analogdata.AddRamp(quad_init, quad_ramp_v(1), ramp_start_time, ramp_t(1), ps3_ao) 'ps8_ao
For index As Integer = 1 To n_times - 1
    analogdata.AddRamp(quad_ramp_v(index), quad_ramp_v(index + 1), ramp_t(index), ramp_t(index + 1), ps3_ao) 'ps8_ao
Next

'----------------------------------------------------- Pinning -------------------------------------------------------------------------------

'Bring everything back up for pinning
' 2D1 
analogdata.AddSmoothRamp(lattice1_ramp_v(n_times), lattice1_max_volt, ramp_end_time, pinning_start_time, lattice2D765_power)
analogdata.AddStep(lattice1_max_volt, pinning_start_time, pinning_end_time, lattice2D765_power)
' 2D2 raise to final depth and hold
analogdata.AddSmoothRamp(lattice2_ramp_v(n_times), lattice2_max_volt, ramp_end_time, pinning_start_time, lattice2D765_power2)
analogdata.AddStep(lattice2_max_volt, pinning_start_time, pinning_end_time, lattice2D765_power2)

analogdata.AddRamp(quic_ramp_v(n_times), 0, ramp_end_time, pinning_start_time, ps1_ao) 'ps6_ao

'---------------------------------------------------------------------------------------------------------------------------------------------
