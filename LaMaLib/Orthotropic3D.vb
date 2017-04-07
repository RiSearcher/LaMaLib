' ===================================================
'
' Copyright (c) 2017 Sergey Tarasov
' https://github.com/RiSearcher
'
' ===================================================

Public Class Orthotropic3D

    Private _E11, _E22, _E33 As Double
    Private _nu12, _nu13, _nu23 As Double
    Private _G12, _G13, _G23 As Double
    Private _name As String
    Private _comment As String

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
        End Set
    End Property
    Public Property E22 As Double
        Get
            Return _E22
        End Get
        Set(value As Double)
            _E22 = value
        End Set
    End Property
    Public Property E33 As Double
        Get
            Return _E33
        End Get
        Set(value As Double)
            _E33 = value
        End Set
    End Property

    Public Property nu12 As Double
        Get
            Return _nu12
        End Get
        Set(value As Double)
            _nu12 = value
        End Set
    End Property
    Public Property nu13 As Double
        Get
            Return _nu13
        End Get
        Set(value As Double)
            _nu13 = value
        End Set
    End Property
    Public Property nu23 As Double
        Get
            Return _nu23
        End Get
        Set(value As Double)
            _nu23 = value
        End Set
    End Property

    Public Property G12 As Double
        Get
            Return _G12
        End Get
        Set(value As Double)
            _G12 = value
        End Set
    End Property
    Public Property G13 As Double
        Get
            Return _G13
        End Get
        Set(value As Double)
            _G13 = value
        End Set
    End Property
    Public Property G23 As Double
        Get
            Return _G23
        End Get
        Set(value As Double)
            _G23 = value
        End Set
    End Property

    Public ReadOnly Property nu21 As Double
        Get
            Return _E22 / _E11 * _nu12
        End Get
    End Property
    Public ReadOnly Property nu31 As Double
        Get
            Return _E33 / _E11 * _nu13
        End Get
    End Property
    Public ReadOnly Property nu32 As Double
        Get
            Return _E33 / _E22 * _nu23
        End Get
    End Property


#End Region

    Public Sub New(name As String, E11 As Double, E22 As Double, E33 As Double,
                   nu12 As Double, nu23 As Double, nu13 As Double,
                   G12 As Double, G23 As Double, G13 As Double, Optional comment As String = "")
        _name = name
        _comment = comment
        _E11 = E11
        _E22 = E22
        _E33 = E33
        _nu12 = nu12
        _nu23 = nu23
        _nu13 = nu13
        _G12 = G12
        _G23 = G23
        _G13 = G13
    End Sub



End Class

