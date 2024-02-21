Public Function GaugeJToVolts(ByVal desired_j As Double, _
                              ByVal coeffs As Double(), _
                              ByVal ncoeffs As Integer) As Double

    Dim nterms As Integer = (ncoeffs - 1)/2
    Dim volts As Double = coeffs(0)
    For index As Integer = 1 To nterms
        volts = volts + coeffs(2 * index - 1) * Exp(-desired_j * coeffs(2 * index))
    Next
    Console.WriteLine("J = {0} -> V = {1}", desired_j, volts)
    Return volts
End Function
