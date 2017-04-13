' ===================================================
'
' Copyright (c) 2017 Sergey Tarasov
' https://github.com/RiSearcher
'
' ===================================================

Imports System.Math
Imports MatrixVectorLib

Public Class ThinPlate

    Private plate As Laminate


    ''' <summary>
    ''' Calculate stresses at given layer in global coordinate system.
    ''' </summary>
    ''' <param name="def">Deformation vector.</param>
    ''' <param name="n">Layer number</param>
    ''' <param name="pos">Position inside layer (0 - bottom surface, 1 - top surface).</param>
    ''' <returns>2D stress object</returns>
    Public Function GetStressGlobal(def As Vector, n As Integer, pos As Double) As Stress2D

        Dim z As Double = 0
        For i As Integer = 1 To n - 1
            z += plate(i).H
        Next
        z += plate(n).H * pos

        Dim eps As Strain2D = New Strain2D(def.Slice(0, 3) + (z - plate.H / 2) * def.Slice(3, 3))
        Dim sig As Stress2D = New Stress2D()
        sig.m = plate(n).Q_bar * eps.m

        Return sig
    End Function

    ''' <summary>
    ''' Calculate stresses at given layer in local coordinate system.
    ''' </summary>
    ''' <param name="def">Deformation vector.</param>
    ''' <param name="n">Layer number</param>
    ''' <param name="pos">Position inside layer (0 - bottom surface, 1 - top surface).</param>
    ''' <returns>2D stress object</returns>
    Public Function GetStressLocal(def As Vector, n As Integer, pos As Double) As Stress2D
        Dim sig As Stress2D = Me.GetStressGlobal(def, n, pos)
        sig = sig.Rotate(plate(n).Angle)
        Return sig
    End Function
End Class
