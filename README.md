# NET.Algebra

This simple library should provide basic functionality for solving some of the common algebra problems. At the moment, it focuses mostly on matrices.

## Matrices
In order to create a new Matrix object, you can type:

```c#
var mat = new Matrix(3, 4);
```
This will create a zero matrix consisting of 3 rows and 4 columns. If you want to fill it with random numbers from a specific interval instead, use:
```c#
mat.FillWithRandomValues(-1, 1);
```
You can initialize a Matrix object with an array of double-precision values or integers:
```c#
double[,] arr = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 }, { 10, 11, 12 } };
var mat = new Matrix(arr);
```

### Resources used:  
* Passos, WD. *Numerical Methods, Algorithms and Tools in C#*. CRC Press, Inc., 2009  
* http://dev.bratched.fr/en/fun-with-matrix-multiplication-and-unsafe-code/  
* https://stackoverflow.com/questions/3407012/c-rounding-up-to-the-nearest-multiple-of-a-number
