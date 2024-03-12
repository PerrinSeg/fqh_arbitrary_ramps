Public Function JToVolts(ByVal desired_j As Double, _
                         ByVal coeffs As Double(), _
                         ByVal ncoeffs As Integer, _
                         ByVal calib_depth As Double, _
                         ByVal calib_volt As Double, _
                         ByVal offset_volt As Double) As Double

    Dim nterms As Integer = (ncoeffs)/2
    Dim desired_depth As Double = coeffs(0)
    For index As Integer = 1 To nterms
        desired_depth = desired_depth + coeffs(2 * index - 1) * Exp(-desired_j / coeffs(2 * index))
    Next
    Dim volts As Double = offset_volt + calib_volt + 0.5*Log10(desired_depth/calib_depth)
    
    Return volts
End Function
