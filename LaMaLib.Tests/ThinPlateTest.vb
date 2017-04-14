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

<TestClass()>
Public Class ThinPlateTest

    Private testContextInstance As TestContext

    Private cfrp As Orthotropic2D = New Orthotropic2D("CFRP", 148, 9.65, 0.3, 4.55)
    Private lam_quasi As Laminate
    Private plate_quasi As ThinPlate

    Private lam_45 As Laminate
    Private plate_45 As ThinPlate

    '''<summary>
    '''Gets or sets the test context which provides
    '''information about and functionality for the current test run.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(ByVal value As TestContext)
            testContextInstance = Value
        End Set
    End Property

#Region "Additional test attributes"
    '
    ' You can use the following additional attributes as you write your tests:
    '
    ' Use ClassInitialize to run code before running the first test in the class
    ' <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    ' End Sub
    '
    ' Use ClassCleanup to run code after all tests in a class have run
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub
    '
    ' Use TestInitialize to run code before running each test
    <TestInitialize()> Public Sub MyTestInitialize()
        lam_quasi = New Laminate("QuasiIsotropic")
        lam_quasi.AddLayer(cfrp, 1, 0)
        lam_quasi.AddLayer(cfrp, 1, 45)
        lam_quasi.AddLayer(cfrp, 1, -45)
        lam_quasi.AddLayer(cfrp, 1, 90)
        plate_quasi = New ThinPlate(lam_quasi)

        lam_45 = New Laminate("-45")
        lam_45.AddLayer(cfrp, 1, 0)
        lam_45.AddLayer(cfrp, 1, 45)
        lam_45.AddLayer(cfrp, 1, -45)
        lam_45.AddLayer(cfrp, 1, 90)
        plate_45 = New ThinPlate(lam_45)
    End Sub
    '
    ' Use TestCleanup to run code after each test has run
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region

    <TestMethod()>
    Public Sub GetShellForcesTest()
        Dim eps As Strain2D = New Strain2D({0.01, 0, 0})
        Dim Target As Vector = New Vector({2.499, 0.7893, 0})

        Dim N As Vector = plate_quasi.GetShellForces(eps)
        Assert.IsTrue((N - Target).Norm < 0.001)
    End Sub

    <TestMethod()>
    Public Sub CalcShellForcesVectorTest()
        Dim eps As Vector = New Vector({0.01, -0.01, 0, 0.01, 0, -0.02})
        Dim Target As Vector = New Vector({0.3179, -1.014, -0.3479, 1.917, -1.708, -0.9339})

        Dim NM As Vector = lam_quasi.ABD * eps
        Assert.IsTrue((NM - Target).Norm < 0.001)
    End Sub

    <TestMethod()>
    Public Sub CalcMidplaneStrainVectorTest()
        Dim NM As Vector = New Vector({1, 0, 0, 0, 0, 0})
        Dim Target As Vector = New Vector({0.01092, -0.004539, 0.003203, 0.005771, 0.002097, 0.004754})
        Dim eps As Vector = lam_quasi.S_abd * NM
        Assert.IsTrue((eps - Target).Norm < 0.001)

        NM = New Vector({0, 0, 0, 1, 0, 0})
        Target = New Vector({0.005771, -0.002097, 0.002623, 0.005659, 0.0007847, 0.002737})
        eps = lam_quasi.S_abd * NM
        Assert.IsTrue((eps - Target).Norm < 0.001)
    End Sub

    <TestMethod()>
    Public Sub GetStressGlobalTest()
        Dim NM As Vector = New Vector({0, 0, 0, 1, 0, 0})
        Dim eps As Vector = lam_quasi.S_abd * NM

        Dim sig As Stress2D = plate_quasi.GetStressGlobal(eps, 1, 0.5)
        Assert.IsTrue(Abs(sig.Sxx + 0.41409) < 0.00001)
        sig = plate_quasi.GetStressGlobal(eps, 2, 0.5)
        Assert.IsTrue(Abs(sig.Syy - 0.037494) < 0.000001)
    End Sub

    <TestMethod()>
    Public Sub GetStressLocalTest()
        Dim NM As Vector = New Vector({0, 0, 0, 1, 0, 0})
        Dim eps As Vector = lam_quasi.S_abd * NM

        Dim sig As Stress2D = plate_quasi.GetStressLocal(eps, 1, 0.5)
        Assert.IsTrue(Abs(sig.Sxx + 0.41409) < 0.00001)
        sig = plate_quasi.GetStressLocal(eps, 2, 0.5)
        Assert.IsTrue(Abs(sig.Syy + 0.0014087) < 0.0000001)
    End Sub

End Class
