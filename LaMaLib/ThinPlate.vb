' ===================================================
'
' Copyright (c) 2017 Sergey Tarasov
' https://github.com/RiSearcher
'
' ===================================================

Imports System.Math
Imports MatrixVectorLib

Public Class ThinPlate

    Private _lam As Laminate

    Public Sub New(lam As Laminate)
        _lam = lam
    End Sub

    Public ReadOnly Property Lam As Laminate
        Get
            Return _lam
        End Get
    End Property

    ''' <summary>
    ''' Returns shell forces vector {Nx,Ny,Nxy} from average stress tensor
    ''' </summary>
    ''' <param name="sig">Average stress tensor</param>
    Public Function GetShellForces(sig As Stress2D) As Vector
        Return New Vector({sig.Sxx * _lam.H, sig.Syy * _lam.H, sig.Sxy * _lam.H})
    End Function

    ''' <summary>
    ''' Returns shell forces vector {Nx,Ny,Nxy} from strain tensor
    ''' </summary>
    ''' <param name="eps">Strain tensor</param>
    Public Function GetShellForces(eps As Strain2D) As Vector
        Return _lam.A * eps.Vec
    End Function


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
            z += _lam(i).H
        Next
        z += _lam(n).H * pos

        Dim eps As Strain2D = New Strain2D(def.Slice(0, 3) + (z - _lam.H / 2) * def.Slice(3, 3))
        Dim sig As Stress2D = New Stress2D()
        sig.m = _lam(n).Q_bar * eps.m

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
        sig = sig.Rotate(_lam(n).Angle)
        Return sig
    End Function
End Class
