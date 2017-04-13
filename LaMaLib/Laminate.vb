' ===================================================
'
' Copyright (c) 2017 Sergey Tarasov
' https://github.com/RiSearcher
'
' ===================================================

Imports System.Math
Imports MatrixVectorLib

Public Class Laminate

    Public Structure Layer
        Dim Angle As Double
        Dim Material As Orthotropic2D
        Dim H As Double
        Public ReadOnly Property Q_bar As Matrix
            Get
                Return Material.Q_bar(Angle)
            End Get
        End Property
    End Structure

    Private _H As Double
    Private _N As Integer
    Private val As Layer()
    Private _abd As Matrix
    Private _abd_inv As Matrix
    Private _a, _b, _d As Matrix
    Private _data_changed As Boolean = True
    Private _tol As Double = 0.000000000001
    Private _assume_symmetric As Boolean = True

    Private _name As String
    Private _comment As String

    Public Sub New(name As String, Optional comment As String = "")
        _name = name
        _comment = comment
        _N = 0
        _H = 0
        _data_changed = True
    End Sub

    Public Sub New(name As String, lam() As Layer, Optional comment As String = "")
        _name = name
        _comment = comment
        val = lam
        _N = val.Length
        _H = 0
        For i As Integer = 0 To _N - 1
            _H += val(i).H
        Next
        _data_changed = True
    End Sub

    Default Public Property Item(i As Integer) As Layer
        Get
            Return val(i - 1)
        End Get
        Set(value As Layer)
            val(i - 1) = value
            _data_changed = True
        End Set
    End Property

#Region "GetSet"

    Public ReadOnly Property N As Integer
        Get
            Return _N
        End Get
    End Property
    Public ReadOnly Property H As Double
        Get
            Return _H
        End Get
    End Property
    Public ReadOnly Property Angle(i As Integer) As Double
        Get
            If i < 1 OrElse i > _N Then Throw New Exception("Invalid layer number")
            Return Me(i).Angle
        End Get
    End Property
    Public ReadOnly Property LayerThickness(i As Integer) As Double
        Get
            If i < 1 OrElse i > _N Then Throw New Exception("Invalid layer number")
            Return Me(i).H
        End Get
    End Property
    Public ReadOnly Property Material(i As Integer) As Orthotropic2D
        Get
            If i < 1 OrElse i > _N Then Throw New Exception("Invalid layer number")
            Return Me(i).Material
        End Get
    End Property

#End Region

#Region "Laminate generators"

    ''' <summary>
    ''' Add new layer to laminate
    ''' </summary>
    ''' <param name="material">Material</param>
    ''' <param name="thickness">Thickness</param>
    ''' <param name="angle">Angle</param>
    Public Sub AddLayer(material As Orthotropic2D, thickness As Double, angle As Double)

        ReDim Preserve val(_N)
        val(_N).Angle = angle
        val(_N).Material = material
        val(_N).H = thickness
        _N += 1
        _H += thickness
        _data_changed = True
    End Sub

    ''' <summary>
    ''' Add new layer to laminate at specific position
    ''' </summary>
    ''' <param name="ind">Position of new layer</param>
    ''' <param name="mat">Material</param>
    ''' <param name="thickness">Thickness</param>
    ''' <param name="angle">Angle</param>
    Public Sub AddLayerAt(ind As Integer, mat As Orthotropic2D, thickness As Double, angle As Double)
        If (ind > _N + 1 OrElse ind < 1) Then
            Throw New Exception("Invalid layer index")
        End If
        ReDim Preserve val(_N)

        For i As Integer = _N - 1 To ind - 1 Step -1
            val(i + 1).Angle = val(i).Angle
            val(i + 1).Material = val(i).Material
            val(i + 1).H = val(i).H
        Next

        val(ind - 1).Angle = angle
        val(ind - 1).Material = mat
        val(ind - 1).H = thickness

        _N += 1
        _H += thickness
        _data_changed = True
    End Sub

    Public Sub DeleteLayer(ind As Integer)
        If (ind > _N OrElse ind < 1) Then
            Throw New Exception("Invalid layer index")
        End If

        _H -= val(ind - 1).H
        For i As Integer = ind To _N - 1
            val(i - 1).Angle = val(i).Angle
            val(i - 1).Material = val(i).Material
            val(i - 1).H = val(i).H
        Next
        _N -= 1
        ReDim Preserve val(_N)
        _data_changed = True

    End Sub

    Public Sub MakeSymmetric()
        ReDim Preserve val(2 * _N - 1)
        For i As Integer = 0 To _N - 1
            val(2 * _N - 1 - i) = val(i)
        Next
        _N += _N
        _H += _H
        _data_changed = True
    End Sub

    Public Sub MakeSymmetricMidPly()
        ReDim Preserve val(2 * _N - 2)
        For i As Integer = 0 To _N - 2
            val(2 * _N - 2 - i) = val(i)
            _H += val(i).H
        Next
        _N += _N - 1
        _data_changed = True
    End Sub

    Public Sub MakeAntiSymmetric()
        ReDim Preserve val(2 * _N - 1)
        For i As Integer = 0 To _N - 1
            val(2 * _N - 1 - i) = val(i)
            val(2 * _N - 1 - i).Angle = -val(i).Angle
        Next
        _N += _N
        _H += _H
        _data_changed = True
    End Sub

    Public Sub MakeAntiSymmetricMidPly()
        ReDim Preserve val(2 * _N - 2)
        For i As Integer = 0 To _N - 2
            val(2 * _N - 2 - i) = val(i)
            val(2 * _N - 2 - i).Angle = -val(i).Angle
            _H += val(i).H
        Next
        _N += _N - 1
        _data_changed = True
    End Sub

    Public Sub Repeat(n As Integer)
        ReDim Preserve val(_N + n * _N - 1)
        For j As Integer = 1 To n
            For i As Integer = 0 To _N - 1
                val(j * _N + i) = val(i)
            Next
        Next
        _N += n * _N
        _H += n * _H
        _data_changed = True
    End Sub

    Public Function Sublaminate(name As String, start As Integer, len As Integer, Optional comment As String = "") As Laminate
        If start < 1 AndAlso start > _N AndAlso start + len > _N Then
            Throw New Exception("Invalid input data")
        End If
        Dim tarr As Layer()
        ReDim tarr(len - 1)
        For i As Integer = 0 To len - 1
            tarr(i) = val(i + start - 1)
        Next
        Return New Laminate(name, tarr, comment)
    End Function

#End Region

#Region "ABD"

    Private Sub CalcABD()

        _abd = New Matrix(6, 6)
        _a = New Matrix(3, 3)
        _b = New Matrix(3, 3)
        _d = New Matrix(3, 3)

        Dim z As Double()
        ReDim z(_N)
        z(0) = -_H / 2
        For i As Integer = 0 To _N - 1
            z(i + 1) = z(i) + val(i).H
        Next

        For i As Integer = 0 To _N - 1
            Dim Q_bar As Matrix = val(i).Material.Q_bar(val(i).Angle)
            _a += Q_bar * (z(i + 1) - z(i))
            _b += Q_bar * (z(i + 1) ^ 2 - z(i) ^ 2) / 2
            _d += Q_bar * (z(i + 1) ^ 3 - z(i) ^ 3) / 3
        Next

        _abd.Slice(0, 0, 3, 3) = _a
        _abd.Slice(0, 3, 3, 3) = _b
        _abd.Slice(3, 0, 3, 3) = _b
        _abd.Slice(3, 3, 3, 3) = _d

        ' Invalidate ABD inverse
        _abd_inv = Nothing

    End Sub

    Public ReadOnly Property ABD As Matrix
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _abd.Clone
        End Get
    End Property

    Public ReadOnly Property A As Matrix
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _a.Clone
        End Get
    End Property

    Public ReadOnly Property B As Matrix
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _b.Clone
        End Get
    End Property

    Public ReadOnly Property D As Matrix
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _d.Clone
        End Get
    End Property

    Public ReadOnly Property A11 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _a(0, 0)
        End Get
    End Property

    Public ReadOnly Property A12 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _a(0, 1)
        End Get
    End Property

    Public ReadOnly Property A16 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _a(0, 2)
        End Get
    End Property

    Public ReadOnly Property A22 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _a(1, 1)
        End Get
    End Property

    Public ReadOnly Property A26 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _a(1, 2)
        End Get
    End Property

    Public ReadOnly Property A66 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _a(2, 2)
        End Get
    End Property

    Public ReadOnly Property B11 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _b(0, 0)
        End Get
    End Property

    Public ReadOnly Property B12 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _b(0, 1)
        End Get
    End Property

    Public ReadOnly Property B16 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _b(0, 2)
        End Get
    End Property

    Public ReadOnly Property B22 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _b(1, 1)
        End Get
    End Property

    Public ReadOnly Property B26 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _b(1, 2)
        End Get
    End Property

    Public ReadOnly Property B66 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _b(2, 2)
        End Get
    End Property

    Public ReadOnly Property D11 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _d(0, 0)
        End Get
    End Property

    Public ReadOnly Property D12 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _d(0, 1)
        End Get
    End Property

    Public ReadOnly Property D16 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _d(0, 2)
        End Get
    End Property

    Public ReadOnly Property D22 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _d(1, 1)
        End Get
    End Property

    Public ReadOnly Property D26 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _d(1, 2)
        End Get
    End Property

    Public ReadOnly Property D66 As Double
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
            End If
            Return _d(2, 2)
        End Get
    End Property

#End Region

#Region "ABD inverse"
    Public ReadOnly Property S_abd As Matrix
        Get
            If _data_changed Then
                Call CalcABD()
                _data_changed = False
                _abd_inv = Nothing
            End If
            If _abd_inv Is Nothing Then
                _abd_inv = _abd.Inverse
            End If
            Return _abd_inv.Clone
        End Get
    End Property
    Public ReadOnly Property S_a As Matrix
        Get
            Return S_abd.Slice(0, 0, 3, 3)
        End Get
    End Property
    Public ReadOnly Property S_b As Matrix
        Get
            Return S_abd.Slice(0, 3, 3, 3)
        End Get
    End Property
    Public ReadOnly Property S_d As Matrix
        Get
            Return S_abd.Slice(3, 3, 3, 3)
        End Get
    End Property
    Public ReadOnly Property S_a11 As Double
        Get
            Return S_a(0, 0)
        End Get
    End Property
    Public ReadOnly Property S_a12 As Double
        Get
            Return S_a(0, 1)
        End Get
    End Property
    Public ReadOnly Property S_a16 As Double
        Get
            Return S_a(0, 2)
        End Get
    End Property
    Public ReadOnly Property S_a22 As Double
        Get
            Return S_a(1, 1)
        End Get
    End Property
    Public ReadOnly Property S_a26 As Double
        Get
            Return S_a(1, 2)
        End Get
    End Property
    Public ReadOnly Property S_a66 As Double
        Get
            Return S_a(2, 2)
        End Get
    End Property
    Public ReadOnly Property S_b11 As Double
        Get
            Return S_b(0, 0)
        End Get
    End Property
    Public ReadOnly Property S_b12 As Double
        Get
            Return S_b(0, 1)
        End Get
    End Property
    Public ReadOnly Property S_b16 As Double
        Get
            Return S_b(0, 2)
        End Get
    End Property
    Public ReadOnly Property S_b22 As Double
        Get
            Return S_b(1, 1)
        End Get
    End Property
    Public ReadOnly Property S_b26 As Double
        Get
            Return S_b(1, 2)
        End Get
    End Property
    Public ReadOnly Property S_b66 As Double
        Get
            Return S_b(2, 2)
        End Get
    End Property
    Public ReadOnly Property S_d11 As Double
        Get
            Return S_d(0, 0)
        End Get
    End Property
    Public ReadOnly Property S_d12 As Double
        Get
            Return S_d(0, 1)
        End Get
    End Property
    Public ReadOnly Property S_d16 As Double
        Get
            Return S_d(0, 2)
        End Get
    End Property
    Public ReadOnly Property S_d22 As Double
        Get
            Return S_d(1, 1)
        End Get
    End Property
    Public ReadOnly Property S_d26 As Double
        Get
            Return S_d(1, 2)
        End Get
    End Property
    Public ReadOnly Property S_d66 As Double
        Get
            Return S_d(2, 2)
        End Get
    End Property
#End Region

#Region "Symmetry check"
    ' Various symmetry properties of the laminate

    Public ReadOnly Property IsSymmetrical As Boolean
        Get
            If _b.MaxNorm < _tol Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public ReadOnly Property IsBalanced As Boolean
        Get
            If Abs(A16) < _tol AndAlso Abs(A26) < _tol Then
                Return True
            Else
                Return False
            End If
            Return False
        End Get
    End Property

    Public ReadOnly Property IsOrthotropic As Boolean
        Get
            If Abs(A16) < _tol AndAlso Abs(A26) < _tol AndAlso Abs(B16) < _tol AndAlso Abs(B26) < _tol AndAlso Abs(D16) < _tol AndAlso Abs(D26) < _tol Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public ReadOnly Property IsIsotropic As Boolean
        Get
            If Me.IsOrthotropic AndAlso
                  Abs(A11 - A22) < _tol AndAlso Abs(A66 - (A11 - A12) / 2) < _tol AndAlso
                  Abs(B11 - B22) < _tol AndAlso Abs(B66 - (B11 - B12) / 2) < _tol AndAlso
                  Abs(D11 - D22) < _tol AndAlso Abs(D66 - (D11 - D12) / 2) < _tol Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public ReadOnly Property IsQuasiIsotropic As Boolean
        Get
            If Abs(A11 - A22) < _tol AndAlso Abs(A66 - (A11 - A12) / 2) < _tol Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

#End Region

#Region "Effective properties"
    Public ReadOnly Property Ex As Double
        Get
            Return S_d11 / (S_a11 * S_d11 - S_b11 ^ 2) / _H
        End Get
    End Property
    Public ReadOnly Property Ey As Double
        Get
            Return S_d22 / (S_a22 * S_d22 - S_b22 ^ 2) / _H
        End Get
    End Property
    Public ReadOnly Property Gxy As Double
        Get
            Return S_d66 / (S_a66 * S_d66 - S_b66 ^ 2) / _H
        End Get
    End Property
    Public ReadOnly Property nu_xy As Double
        Get
            Return -(S_a12 * S_d11 - S_b12 * S_b11) / (S_a11 * S_d11 - S_b11 ^ 2)
        End Get
    End Property


#End Region

    Public Overrides Function ToString() As String
        If N = 0 Then Return "Empty laminate"

        Dim str As New System.Text.StringBuilder

        str.Append(String.Format("Laminate: number of layers {0}, total thickness {1}", _N, _H) + vbCrLf)
        str.Append(" Layer | Thickness | Angle | Material " + vbCrLf)
        str.Append("--------------------------------------" + vbCrLf)
        For i As Integer = 1 To _N
            str.Append(String.Format(" {0,5} | {1,9} | {2,5} | {3} ", i, Me.LayerThickness(i), Me.Angle(i), Me.Material(i).Name) + vbCrLf)
        Next
        str.Append("--------------------------------------" + vbCrLf)
        Return str.ToString
    End Function

End Class

