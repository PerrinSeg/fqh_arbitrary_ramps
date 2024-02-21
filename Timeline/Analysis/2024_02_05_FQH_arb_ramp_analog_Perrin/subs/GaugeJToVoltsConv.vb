Public Function GaugeJToVolts(ByVal desired_j As Double, _
                              ByVal coeffs As Double(), _
                              ByVal ncoeffs As Integer, _
                              ByVal calib_depth As Double, _
                              ByVal calib_volt As Double) As Double

    'Dim nterms As Integer = (ncoeffs - 1)/2
    Dim volts As Double
    Dim desired_depth As Double
    If desired_j = 0 Then
        volts = 0
    Else
        desired_depth = 0
        For index As Integer = 0 To ncoeffs
            desired_depth = desired_depth + coeffs(index) * desired_j^(index+1)
            'volts = volts + coeffs(2 * index - 1) * Exp(-desired_j * coeffs(2 * index))
            'Console.WriteLine(" next term {2}: {0} * x^{1})", coeffs(index), index+1, index)
        Next
        volts = calib_volt + 0.5*Log10(desired_depth/calib_depth)
    End If
    
    Return volts
End Function
