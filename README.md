# MatrixAlgebraNET

This library is intended to provide basic functionality for solving some of the common matrix algebra problems.

In order to create a new Matrix object, you can type:
```c#
var mat = new Matrix(3, 4);
```
This will create a 12-element zero matrix consisting of 3 rows and 4 columns. You can verify that by calling:
```c#
Console.WriteLine("Number of rows: " + mat.NumRows);
Console.WriteLine("Number of colums: " + mat.NumCols);
Console.WriteLine("Number of elements: " + mat.TotalSize);
```
Output:
```
Number of rows: 3
Number of colums: 4
Number of elements: 12
```
If you want to fill it with random numbers from a specific interval instead, use:
```c#
mat.FillWithRandomValues(-1, 1);
```
Alternatively, you can initialize a Matrix object with an existing array of double-precision values or integers:
```c#
double[,] arr = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 } };
var mat = new Matrix(arr);
```
... or directly within the constuctor call:
```c#
var mat = new Matrix(new[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 } });
```
... or implicitly:
```c#
Matrix mat = new[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 } };
```
It is also possible to initialize a Matrix with a flat (1-D) array if we specify the target width (number of columns):
```c#
var mat = new Matrix(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 4);
```
If the length of the flat array is not a multiple of the specified width, all the remaining elements default to zeros:
```c#
var mat = new Matrix(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 4);
```
In the case shown above, the last two elemens of the matrix will be zeros.

## Resources:  
* Passos, WD. *Numerical Methods, Algorithms and Tools in C#*. CRC Press, Inc., 2009  
* http://dev.bratched.fr/en/fun-with-matrix-multiplication-and-unsafe-code/  
* https://stackoverflow.com/questions/3407012/c-rounding-up-to-the-nearest-multiple-of-a-number
