Imports LaMaLib
Imports MatrixVectorLib

Module Module1

    Sub Main()

        ' Create new material
        Dim cfrp As Orthotropic2D = New Orthotropic2D("CFRP", 148, 9.65, 0.3, 4.55)


        ' Create new laminate
        Dim lam As Laminate = New Laminate("Test")
        lam.AddLayer(cfrp, 1, 0)
        lam.AddLayer(cfrp, 1, 45)
        lam.AddLayer(cfrp, 1, -45)
        lam.AddLayer(cfrp, 1, 90)
        Console.WriteLine(lam)
        Console.WriteLine()


        ' Print ABD matrix
        Console.WriteLine(lam.ABD.ToString)
        Console.WriteLine()


        ' Calculate effective properties for current laminate
        lam.AssumeSymmetric = False
        Console.WriteLine("Ex: {0}, Ey: {1}, Gxy: {2}, nu_xy: {3}", lam.Ex, lam.Ey, lam.Gxy, lam.nu_xy)
        Console.WriteLine()

        ' Calculate effective properties for laminate
        ' (assuming that laminate is symmetric)
        lam.AssumeSymmetric = True
        Console.WriteLine("Ex: {0}, Ey: {1}, Gxy: {2}, nu_xy: {3}", lam.Ex, lam.Ey, lam.Gxy, lam.nu_xy)
        Console.WriteLine()

        ' Print stiffness matrix of third ply
        Console.WriteLine("Ply 3 stiffness matrix: ")
        Console.WriteLine(lam(3).Q_bar)
        Console.WriteLine()

        ' Apply unidirectional tension to the laminate
        Dim NM As New Vector({1, 0, 0, 0, 0, 0})
        Dim eps As Vector = lam.ABD.Inverse * NM
        Console.WriteLine("Applied load: {Nx, Ny, Nxy, Mx, My, Mxy}")
        Console.WriteLine(NM)
        Console.WriteLine("Midplane strains: {eps_x, eps_y, eps_xy, k_x, k_y, k_xy}")
        Console.WriteLine(eps)
        Console.WriteLine()

        ' Create "ThinPlate" object to calculate ply stressess
        Dim plate As ThinPlate = New ThinPlate(lam)
        Dim sig As Stress2D

        ' Get midply stresses (in global coordinate system)
        Console.WriteLine("Midply stresses in global coord.sys.")
        sig = plate.GetStressGlobal(eps, 1, 0.5)
        Console.WriteLine(sig.Vec)
        sig = plate.GetStressGlobal(eps, 2, 0.5)
        Console.WriteLine(sig.Vec)
        sig = plate.GetStressGlobal(eps, 3, 0.5)
        Console.WriteLine(sig.Vec)
        sig = plate.GetStressGlobal(eps, 4, 0.5)
        Console.WriteLine(sig.Vec)
        Console.WriteLine()


        ' Get midply stresses (in ply coordinate system)
        Console.WriteLine("Midply stresses in ply coord.sys.")
        sig = plate.GetStressLocal(eps, 1, 0.5)
        Console.WriteLine(sig.Vec)
        sig = plate.GetStressLocal(eps, 2, 0.5)
        Console.WriteLine(sig.Vec)
        sig = plate.GetStressLocal(eps, 3, 0.5)
        Console.WriteLine(sig.Vec)
        sig = plate.GetStressLocal(eps, 4, 0.5)
        Console.WriteLine(sig.Vec)
        Console.WriteLine()



        Console.ReadLine()

    End Sub

End Module

