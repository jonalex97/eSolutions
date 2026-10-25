Namespace Models
    ''' <summary>
    ''' Representa la configuración técnica y visual de un canal de pagos obtenida de la base de datos.
    ''' </summary>
    <Serializable>
    Public Class CanalConfiguracion
        Public Property CanalCodigo As String
        Public Property CanalNombre As String
        Public Property ServicioUrl As String
        Public Property ApiKey As String
        Public Property MuestraMenu As Boolean
        Public Property PaginaDestino As String
        Public Property TimeoutSegundos As Integer
        Public Property Activo As Boolean

        Public Sub New()
            TimeoutSegundos = 10
            MuestraMenu = True
            Activo = True
        End Sub
    End Class
End Namespace
