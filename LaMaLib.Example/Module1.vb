Imports LaMaLib

Module Module1

    Sub Main()

        ' Create new material
        Dim cfrp As Orthotropic2D = New Orthotropic2D("CFRP", 148, 9.65, 0.3, 4.55)


        ' Create new laminate
        Dim lam As Laminate = New Laminate


        lam.AddLayer(cfrp, 1, 0)
        lam.AddLayer(cfrp, 1, 45)
        lam.AddLayer(cfrp, 1, -45)
        lam.AddLayer(cfrp, 1, 90)

        ' Print ABD matrix
        Console.WriteLine(lam.ABD.ToString)

        ' Make symmetric laminate
        lam.MakeSymmetric()

        ' Print ABD matrix
        Console.WriteLine(lam.ABD.ToString)

        ' Print effective properties
        Console.WriteLine("Ex: {0}, Ey: {1}, Gxy: {2}, nu_xy: {3}", lam.Ex, lam.Ey, lam.Gxy, lam.nu_xy)


        Console.ReadLine()

    End Sub

End Module

