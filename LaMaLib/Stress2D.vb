﻿' ===================================================
'
' Copyright (c) 2017 Sergey Tarasov
' https://github.com/RiSearcher
'
' ===================================================

Imports System.Math
Imports MatrixVectorLib

Public Class Stress2D

    Private val As Matrix

    Public Sub New()
        val = New Matrix(2, 2)
    End Sub

    Public Sub New(m As Matrix)
        If m.RowCount <> 2 AndAlso m.ColumnCount <> 2 Then
            Throw New Exception("Invalid matrix size")
        End If
        val = m.Clone
    End Sub

    Public Sub New(v As Vector)
        If v.Size <> 3 Then
            Throw New Exception("Invalid vector size")
        End If
        val = New Matrix(2, 2)
        val(0, 0) = v(0)
        val(1, 1) = v(1)
        val(0, 1) = v(2)
        val(1, 0) = v(2)
    End Sub

    Public Sub New(v As Double())
        If v.Length <> 3 Then
            Throw New Exception("Invalid vector size")
        End If
        val = New Matrix(2, 2)
        val(0, 0) = v(0)
        val(1, 1) = v(1)
        val(0, 1) = v(2)
        val(1, 0) = v(2)
    End Sub

    Default Public Property Item(i As Integer, j As Integer) As Double
        Get
            Return val(i, j)
        End Get
        Set(value As Double)
            val(i, j) = value
        End Set
    End Property

    Public Property Vec As Vector
        Get
            Return New Vector({val(0, 0), val(1, 1), val(0, 1)})
        End Get
        Set(value As Vector)
            val(0, 0) = value(0)
            val(1, 1) = value(1)
            val(0, 1) = value(2)
            val(1, 0) = value(2)
        End Set
    End Property

    Public Property m As Matrix
        Get
            Return val.Clone
        End Get
        Set(value As Matrix)
            val = value.Clone
        End Set
    End Property

    Public Property Sxx As Double
        Get
            Return val(0, 0)
        End Get
        Set(value As Double)
            val(0, 0) = value
        End Set
    End Property
    Public Property Syy As Double
        Get
            Return val(1, 1)
        End Get
        Set(value As Double)
            val(1, 1) = value
        End Set
    End Property
    Public Property Sxy As Double
        Get
            Return val(0, 1)
        End Get
        Set(value As Double)
            val(0, 1) = value
            val(1, 0) = value
        End Set
    End Property

    Public ReadOnly Property MaxPrincipal As Double
        Get
            Return (Sxx + Syy) / 2 + Sqrt((Sxx - Syy) ^ 2 / 4 + Sxy ^ 2)
        End Get
    End Property
    Public ReadOnly Property MinPrincipal As Double
        Get
            Return (Sxx + Syy) / 2 - Sqrt((Sxx - Syy) ^ 2 / 4 + Sxy ^ 2)
        End Get
    End Property
    Public ReadOnly Property MaxShear As Double
        Get
            Return Sqrt((Sxx - Syy) ^ 2 / 4 + Sxy ^ 2)
        End Get
    End Property
    Public ReadOnly Property PrincipalAngle As Double
        Get
            Return Atan2(2 * Sxy, (Sxx - Syy)) / 2
        End Get
    End Property
    Public ReadOnly Property Mises As Double
        Get
            Return Sqrt(Sxx ^ 3 - Sxx * Syy + Syy ^ 2 + 3 * Sxy ^ 2)
        End Get
    End Property

    ''' <summary>
    ''' First stress invariant
    ''' </summary>
    Public ReadOnly Property I1 As Double
        Get
            Return Sxx + Syy
        End Get
    End Property
    ''' <summary>
    ''' Second stress invariant
    ''' </summary>
    Public ReadOnly Property I2 As Double
        Get
            Return Sxx * Syy - Sxy ^ 2
        End Get
    End Property
    ''' <summary>
    ''' Third stress invariant
    ''' </summary>
    Public ReadOnly Property I3 As Double
        Get
            Return 0
        End Get
    End Property



    Public Shared Function RotationMatrix(angle As Double) As Matrix
        Return Orthotropic2D.Tsig(angle)
    End Function

    Public Function Rotate(angle As Double) As Stress2D
        Return New Stress2D(RotationMatrix(angle) * Me.Vec)
    End Function




    Public Overrides Function ToString() As String
        Dim str As New System.Text.StringBuilder
        str.Append(String.Format("Stress2D:") + vbCrLf)
        For i As Integer = 0 To 1
            For j As Integer = 0 To 1
                str.Append(String.Format("{0,10:g4}", val(i, j)) + " ")
            Next
            str.Append(vbCrLf)
        Next
        Return str.ToString
    End Function

    ' Operators overloads
    '-----------------------------------------------------------------
    Public Shared Operator +(s1 As Stress2D, s2 As Stress2D) As Stress2D
        Return New Stress2D(s1.m + s2.m)
    End Operator
    Public Shared Operator -(s1 As Stress2D, s2 As Stress2D) As Stress2D
        Return New Stress2D(s1.m - s2.m)
    End Operator
    Public Shared Operator -(s1 As Stress2D) As Stress2D
        Return New Stress2D(-s1.m)
    End Operator
    Public Shared Operator *(s1 As Stress2D, a As Double) As Stress2D
        Return New Stress2D(s1.m * a)
    End Operator
    Public Shared Operator *(a As Double, s1 As Stress2D) As Stress2D
        Return New Stress2D(a * s1.m)
    End Operator
    Public Shared Operator /(s1 As Stress2D, a As Double) As Stress2D
        Return New Stress2D(s1.m / a)
    End Operator

End Class
