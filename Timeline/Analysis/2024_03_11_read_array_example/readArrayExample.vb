'Example to show how to read in an arbitrary 1D array from a .txt file using 'LoadArrayFromFile'

'===================
'=====Variables=====
free_var = 0
'=====Variables=====
'===================
	
'----------------------------------------------------- Constants -----------------------------------------------------------------------------
Dim filePath As String = "Z:\\Timeline\\Analysis\\2024_03_11_read_array_example\\array_file.txt"

Dim array_values As Double() = LoadArrayFromFile(filepath)
Dim n_values As Double = array_values.GetUpperBound(0) 'gets the index of the last entry in the array. Note that since arrays are indexed from 0, the actual length of the array is (array_values + 1).
Console.WriteLine("max array index: {0}", n_values)

array_values(n_values) = 9 'replaces the value of the last index of the array

Dim new_constant As Double = array_values(0) 'copies the last value of the array into a new constant

For index As Integer = 0 To n_values 'iterate through every element of the array
    array_values(index) = array_values(index)*2 'operations on arrays have to be performed in a loop     
Next

	
'----------------------------------------------------- Output -----------------------------------------------------------------------------

'quad grad
For index As Integer = 0 To n_values - 1 'using array values as arguments to a function
    analogdata.AddRamp(array_values(index), array_values(index + 1), index * 42, (index + 1)*42, ps8_ao) 'ps8_ao
NextNext








