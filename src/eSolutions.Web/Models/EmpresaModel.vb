Namespace Models
    ''' <summary>
    ''' Modelo de empresa retornado por el servicio rest.
    ''' </summary>
    <Serializable>
    Public Class EmpresaModel
        Public Property Id As Integer
        Public Property Nombre As String
        Public Property LogoUrl As String

        Public Sub New()
        End Sub

        Public Sub New(id As Integer, nombre As String, logoUrl As String)
            Me.Id = id
            Me.Nombre = nombre
            Me.LogoUrl = logoUrl
        End Sub
    End Class
End Namespace
