' ===================================================
'
' Copyright (c) 2017 Sergey Tarasov
' https://github.com/RiSearcher
'
' ===================================================

Imports System.Text
Imports System.Math
Imports MatrixVectorLib
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class LaminateTests

    Private iso As Orthotropic2D = New Orthotropic2D("Iso", 1, 1, 0.3, 1 / 2 / (1 + 0.3))
    Private iso2 As Orthotropic2D = New Orthotropic2D("Iso2", 2, 2, 0.3, 2 / 2 / (1 + 0.3))
    Private cfrp As Orthotropic2D = New Orthotropic2D("CFRP", 148, 9.65, 0.3, 4.55)

#Region "ABD tests"

    <TestMethod()> Public Sub QMatrixTest()

        Dim m As New Matrix({{148.87, 2.91, 0}, {2.91, 9.71, 0}, {0, 0, 4.55}})
        Assert.IsTrue((cfrp.Q_bar(0) - m).MaxNorm < 0.01)

        m = New Matrix({{0.5, 0.5, 1.0}, {0.5, 0.5, -1.0}, {-0.5, 0.5, 0}})
        Assert.IsTrue((Orthotropic2D.Tsig(45) - m).MaxNorm < 0.0000000001)

        m = New Matrix({{0.5, 0.5, 0.5}, {0.5, 0.5, -0.5}, {-1.0, 1.0, 0}})
        Assert.IsTrue((Orthotropic2D.Teps(45) - m).MaxNorm < 0.0000000001)

        m = New Matrix({{45.65, 36.55, 34.79}, {36.55, 45.65, 34.79}, {34.79, 34.79, 38.19}})
        Assert.IsTrue((cfrp.Q_bar(45) - m).MaxNorm < 0.01)
        Assert.IsTrue((cfrp.Q_bar(45) - cfrp.Q(-45)).MaxNorm < 0.01)

    End Sub

    <TestMethod()> Public Sub ABDMatrixTest()

        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, 0)
        lam.AddLayer(cfrp, 1, 45)

        Dim m As New Matrix({{194.52, 39.46, 34.79}, {39.46, 55.36, 34.79}, {34.79, 34.79, 42.74}})
        Assert.IsTrue((lam.A - m).MaxNorm < 0.01)

        m = New Matrix({{-51.61, 16.82, 17.4}, {16.82, 17.97, 17.4}, {17.4, 17.4, 16.82}})
        Assert.IsTrue((lam.B - m).MaxNorm < 0.01)

        m = New Matrix({{64.84, 13.15, 11.6}, {13.15, 18.45, 11.6}, {11.6, 11.6, 14.25}})
        Assert.IsTrue((lam.D - m).MaxNorm < 0.01)

    End Sub

    <TestMethod()> Public Sub ABDInverseMatrixTest()

        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, 0)
        lam.AddLayer(cfrp, 1, 45)

        Dim m As New Matrix({{13.44, -4.85, -7.14}, {-4.85, 41.81, -21.23}, {-7.14, -21.23, 64.95}})
        Assert.IsTrue((lam.S_a - m / 1000).MaxNorm < 0.01)

        m = New Matrix({{17.07, -6.01, -11.06}, {-6.01, -5.04, -11.06}, {-11.06, -11.06, -24.05}})
        Assert.IsTrue((lam.S_b - m / 1000).MaxNorm < 0.01)

        m = New Matrix({{40.32, -14.56, -21.41}, {-14.56, 125.42, -63.68}, {-21.41, -63.68, 194.86}})
        Assert.IsTrue((lam.S_d - m / 1000).MaxNorm < 0.01)

    End Sub
#End Region

#Region "Symmetry checks"
    <TestMethod()> Public Sub IsIsotropicTest()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(iso, 1, 0)
        lam.AddLayer(iso, 1, 30)
        lam.AddLayer(iso, 1, 60)

        Assert.IsTrue(lam.IsIsotropic)
        Assert.IsTrue(lam.IsQuasiIsotropic)
        Assert.IsTrue(lam.IsOrthotropic)
        Assert.IsTrue(lam.IsBalanced)
        Assert.IsTrue(lam.IsSymmetrical)
    End Sub

    <TestMethod()> Public Sub IsIsotropicTest2()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(iso, 1, 0)
        lam.AddLayer(iso2, 1, 0)

        Assert.IsTrue(lam.IsIsotropic)
        Assert.IsTrue(lam.IsQuasiIsotropic)
        Assert.IsTrue(lam.IsOrthotropic)
        Assert.IsTrue(lam.IsBalanced)
        Assert.IsFalse(lam.IsSymmetrical)
    End Sub

    <TestMethod()> Public Sub IsQuasiIsotropicTest()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, 0)
        lam.AddLayer(cfrp, 1, 45)
        lam.AddLayer(cfrp, 1, -45)
        lam.AddLayer(cfrp, 1, 90)

        Assert.IsFalse(lam.IsIsotropic)
        Assert.IsTrue(lam.IsQuasiIsotropic)
        Assert.IsFalse(lam.IsOrthotropic)
        Assert.IsTrue(lam.IsBalanced)
        Assert.IsFalse(lam.IsSymmetrical)
    End Sub

    <TestMethod()> Public Sub IsQuasiIsotropicTest2()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, 0)
        lam.AddLayer(cfrp, 1, 45)
        lam.AddLayer(cfrp, 1, -45)
        lam.AddLayer(cfrp, 1, 90)
        lam.MakeSymmetric()

        Assert.IsFalse(lam.IsIsotropic)
        Assert.IsTrue(lam.IsQuasiIsotropic)
        Assert.IsFalse(lam.IsOrthotropic)
        Assert.IsTrue(lam.IsBalanced)
        Assert.IsTrue(lam.IsSymmetrical)
    End Sub

    <TestMethod()> Public Sub IsOrthotropicTest()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, 0)
        lam.AddLayer(cfrp, 1, 90)
        lam.AddLayer(cfrp, 1, 0)

        Assert.IsFalse(lam.IsIsotropic)
        Assert.IsFalse(lam.IsQuasiIsotropic)
        Assert.IsTrue(lam.IsOrthotropic)
        Assert.IsTrue(lam.IsBalanced)
        Assert.IsTrue(lam.IsSymmetrical)
    End Sub

    <TestMethod()> Public Sub IsBalancedTest()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, -45)
        lam.AddLayer(cfrp, 1, 45)

        Assert.IsFalse(lam.IsIsotropic)
        Assert.IsFalse(lam.IsQuasiIsotropic)
        Assert.IsFalse(lam.IsOrthotropic)
        Assert.IsTrue(lam.IsBalanced)
        Assert.IsFalse(lam.IsSymmetrical)
    End Sub

    <TestMethod()> Public Sub IsSymmetricalTest()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, -45)
        lam.AddLayer(cfrp, 1, 45)
        lam.AddLayer(cfrp, 1, -45)

        Assert.IsFalse(lam.IsIsotropic)
        Assert.IsFalse(lam.IsQuasiIsotropic)
        Assert.IsFalse(lam.IsOrthotropic)
        Assert.IsFalse(lam.IsBalanced)
        Assert.IsTrue(lam.IsSymmetrical)
    End Sub
#End Region

#Region "Effective properties"
    <TestMethod()> Public Sub IsotropicEffPropTest()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(iso, 1, 0)
        lam.AddLayer(iso, 1, 30)
        lam.AddLayer(iso, 1, 60)

        Assert.IsTrue(Abs(lam.Ex - iso.E11) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Ey - iso.E11) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Gxy - iso.G12) < 0.000000000001)
        Assert.IsTrue(Abs(lam.nu_xy - iso.nu12) < 0.000000000001)
    End Sub

    <TestMethod()> Public Sub IsotropicEffPropTest2()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(iso, 1, 0)
        lam.AddLayer(iso2, 1, 0)

        Assert.IsTrue(Abs(lam.Ex - (iso.E11 + iso2.E11) / 2) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Ey - (iso.E11 + iso2.E11) / 2) < 0.000000000001)
        Assert.IsTrue(Abs(lam.nu_xy - iso.nu12) < 0.000000000001)
    End Sub

    <TestMethod()> Public Sub OrthotropicEffPropTest()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, 0)

        Assert.IsTrue(Abs(lam.Ex - cfrp.E11) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Ey - cfrp.E22) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Gxy - cfrp.G12) < 0.000000000001)
        Assert.IsTrue(Abs(lam.nu_xy - cfrp.nu12) < 0.000000000001)

        lam.DeleteLayer(1)
        lam.AddLayer(cfrp, 1, 90)

        Assert.IsTrue(Abs(lam.Ex - cfrp.E22) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Ey - cfrp.E11) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Gxy - cfrp.G12) < 0.000000000001)
        Assert.IsTrue(Abs(lam.nu_xy - cfrp.nu21) < 0.000000000001)
    End Sub

    <TestMethod()> Public Sub QuasiIsotropicEffPropTest()
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, 0)
        lam.AddLayer(cfrp, 1, 45)
        lam.AddLayer(cfrp, 1, -45)
        lam.AddLayer(cfrp, 1, 90)
        lam.MakeSymmetric()

        Assert.IsTrue(Abs(lam.Ex - lam.Ey) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Gxy - lam.Ex / 2 / (1 + lam.nu_xy)) < 0.000000000001)
    End Sub
#End Region

End Class