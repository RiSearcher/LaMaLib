' ===================================================
'
' Copyright (c) 2017 Sergey Tarasov
' https://github.com/RiSearcher
'
' ===================================================

Imports System.Math
Imports MatrixVectorLib

Public Class Orthotropic2D

    Private _E11, _E22 As Double
    Private _nu12 As Double
    Private _G12 As Double
    Private _name As String
    Private _comment As String

    ' Compliance matrix
    Private _S As Matrix

    ' Stiffness matrix
    Private _Q As Matrix

    ' if true, stiffness and compliance matrix need to be recalculated
    Private data_changed As Boolean

#Region "GetSet"

    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property
    Public Property Comment As String
        Get
            Return _comment
        End Get
        Set(value As String)
            _comment = value
        End Set
    End Property

    Public Property E11 As Double
        Get
            Return _E11
        End Get
        Set(value As Double)
            _E11 = value
            data_changed = True
        End Set
    End Property
    Public Property E22 As Double
        Get
            Return _E22
        End Get
        Set(value As Double)
            _E22 = value
            data_changed = True
        End Set
    End Property

    Public Property nu12 As Double
        Get
            Return _nu12
        End Get
        Set(value As Double)
            _nu12 = value
            data_changed = True
        End Set
    End Property

    Public Property G12 As Double
        Get
            Return _G12
        End Get
        Set(value As Double)
            _G12 = value
            data_changed = True
        End Set
    End Property

    Public ReadOnly Property nu21 As Double
        Get
            Return _E22 / _E11 * _nu12
        End Get
    End Property

#End Region

    Public Sub New(name As String, E11 As Double, E22 As Double, nu12 As Double, G12 As Double, Optional comment As String = "")
        _name = name
        _comment = comment
        _E11 = E11
        _E22 = E22
        _nu12 = nu12
        _G12 = G12
        data_changed = True
    End Sub

    Public Sub New(Mat3D As Orthotropic3D)
        _name = Mat3D.Name
        _comment = Mat3D.Comment
        _E11 = Mat3D.E11
        _E22 = Mat3D.E22
        _nu12 = Mat3D.nu12
        _G12 = Mat3D.G12
        data_changed = True
    End Sub

    ''' <summary>
    ''' Compliance matrix
    ''' </summary>
    Public ReadOnly Property S As Matrix
        Get
            If data_changed Then
                Call CalcSQ()
                data_changed = False
            End If
            Return _S.Clone
        End Get
    End Property

    ''' <summary>
    ''' Compliance matrix
    ''' </summary>
    Public ReadOnly Property S(angle As Double) As Matrix
        Get
            If data_changed Then
                Call CalcSQ()
                data_changed = False
            End If
            Return Teps(angle) * _S * Tsig(angle).Inverse
        End Get
    End Property

    ''' <summary>
    ''' Stiffness matrix
    ''' </summary>
    Public ReadOnly Property Q As Matrix
        Get
            If data_changed Then
                Call CalcSQ()
                data_changed = False
            End If
            Return _Q.Clone
        End Get
    End Property

    ''' <summary>
    ''' Stiffness matrix
    ''' </summary>
    Public ReadOnly Property Q(angle As Double) As Matrix
        Get
            If data_changed Then
                Call CalcSQ()
                data_changed = False
            End If
            Return Tsig(angle) * _Q * Teps(angle).Inverse
        End Get
    End Property

    Private Sub CalcSQ()

        _S = New Matrix(3, 3)

        _S(0, 0) = 1 / _E11
        _S(0, 1) = -_nu12 / _E11
        _S(0, 2) = 0
        _S(1, 0) = _S(0, 1)
        _S(1, 1) = 1 / _E22
        _S(1, 2) = 0
        _S(2, 0) = _S(0, 2)
        _S(2, 1) = _S(1, 2)
        _S(2, 2) = 1 / _G12

        _Q = _S.Inverse
    End Sub

    ''' <summary>
    ''' Strain rotation matrix.
    ''' </summary>
    ''' <param name="a">Angle (degrees).</param>
    Public Shared Function Teps(a As Double) As Matrix
        Dim T As New Matrix(3, 3)
        Dim s As Double = Sin(a * PI / 180)
        Dim c As Double = Cos(a * PI / 180)
        T.Row(1) = New Vector({c ^ 2, s ^ 2, c * s})
        T.Row(2) = New Vector({s ^ 2, c ^ 2, -c * s})
        T.Row(3) = New Vector({-2 * c * s, 2 * c * s, c ^ 2 - s ^ 2})
        Return T
    End Function

    ''' <summary>
    ''' Stress rotation matrix.
    ''' </summary>
    ''' <param name="a">Angle (degrees).</param>
    Public Shared Function Tsig(a As Double) As Matrix
        Dim T As New Matrix(3, 3)
        Dim s As Double = Sin(a * PI / 180)
        Dim c As Double = Cos(a * PI / 180)
        T.Row(1) = New Vector({c ^ 2, s ^ 2, 2 * c * s})
        T.Row(2) = New Vector({s ^ 2, c ^ 2, -2 * c * s})
        T.Row(3) = New Vector({-c * s, c * s, c ^ 2 - s ^ 2})
        Return T
    End Function

    Public Function Q_bar(a As Double) As Matrix
        Return Tsig(a).Inverse * Q * Teps(a)
    End Function

End Class

