Public Function DepthToVolts(ByVal desired_depth As Double, _
    ByVal calib_depth As Double, _
    ByVal calib_volt As Double, _
    ByVal offset_volt As Double) As Double

    'If desired_depth <= 0 Then
    '    volts = 0
    'End If 

    Dim volts As Double = offset_volt + calib_volt + 0.5*Log10(desired_depth/calib_depth)
    
    Return volts
End Function
