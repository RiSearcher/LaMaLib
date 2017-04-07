' ===================================================
'
' Copyright (c) 2017 Sergey Tarasov
' https://github.com/RiSearcher
'
' ===================================================

Imports System.Text
Imports System.Math
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class LaminateTests

    Private iso As Orthotropic2D = New Orthotropic2D("Iso", 1, 1, 0.3, 1 / 2 / (1 + 0.3))
    Private iso2 As Orthotropic2D = New Orthotropic2D("Iso2", 2, 2, 0.3, 2 / 2 / (1 + 0.3))
    Private cfrp As Orthotropic2D = New Orthotropic2D("CFRP", 148, 9.65, 0.3, 4.55)

#Region "Symmetry checks"
    <TestMethod()> Public Sub IsIsotropicTest()
        Dim lam As Laminate = New Laminate
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
        Dim lam As Laminate = New Laminate
        lam.AddLayer(iso, 1, 0)
        lam.AddLayer(iso2, 1, 0)

        Assert.IsTrue(lam.IsIsotropic)
        Assert.IsTrue(lam.IsQuasiIsotropic)
        Assert.IsTrue(lam.IsOrthotropic)
        Assert.IsTrue(lam.IsBalanced)
        Assert.IsFalse(lam.IsSymmetrical)
    End Sub

    <TestMethod()> Public Sub IsQuasiIsotropicTest()
        Dim lam As Laminate = New Laminate
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
        Dim lam As Laminate = New Laminate
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
        Dim lam As Laminate = New Laminate
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
        Dim lam As Laminate = New Laminate
        lam.AddLayer(cfrp, 1, -45)
        lam.AddLayer(cfrp, 1, 45)

        Assert.IsFalse(lam.IsIsotropic)
        Assert.IsFalse(lam.IsQuasiIsotropic)
        Assert.IsFalse(lam.IsOrthotropic)
        Assert.IsTrue(lam.IsBalanced)
        Assert.IsFalse(lam.IsSymmetrical)
    End Sub

    <TestMethod()> Public Sub IsSymmetricalTest()
        Dim lam As Laminate = New Laminate
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
        Dim lam As Laminate = New Laminate
        lam.AddLayer(iso, 1, 0)
        lam.AddLayer(iso, 1, 30)
        lam.AddLayer(iso, 1, 60)

        Assert.IsTrue(Abs(lam.Ex - iso.E11) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Ey - iso.E11) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Gxy - iso.G12) < 0.000000000001)
        Assert.IsTrue(Abs(lam.nu_xy - iso.nu12) < 0.000000000001)
    End Sub

    <TestMethod()> Public Sub IsotropicEffPropTest2()
        Dim lam As Laminate = New Laminate
        lam.AddLayer(iso, 1, 0)
        lam.AddLayer(iso2, 1, 0)

        Assert.IsTrue(Abs(lam.Ex - (iso.E11 + iso2.E11) / 2) < 0.000000000001)
        Assert.IsTrue(Abs(lam.Ey - (iso.E11 + iso2.E11) / 2) < 0.000000000001)
        Assert.IsTrue(Abs(lam.nu_xy - iso.nu12) < 0.000000000001)
    End Sub

    <TestMethod()> Public Sub OrthotropicEffPropTest()
        Dim lam As Laminate = New Laminate
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
        Dim lam As Laminate = New Laminate
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