Imports System
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports eSolutions.Web.Common
Imports eSolutions.Web.Models

Namespace Data
    ''' <summary>
    ''' Repositorio de acceso a datos para la configuración de canales mediante ADO.NET seguro.
    ''' </summary>
    Public Class ConfiguracionRepository
        Private ReadOnly _cadenaConexion As String

        Public Sub New()
            Dim settingConexion As ConnectionStringSettings = ConfigurationManager.ConnectionStrings("eSolutionsConn")
            If settingConexion IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(settingConexion.ConnectionString) Then
                _cadenaConexion = settingConexion.ConnectionString
            Else
                _cadenaConexion = "Server=(localdb)\MSSQLLocalDB;Database=eSolutionsPagos;Integrated Security=True;MultipleActiveResultSets=True;"
            End If
        End Sub

        Public Sub New(cadenaConexion As String)
            If String.IsNullOrWhiteSpace(cadenaConexion) Then
                Throw New ArgumentNullException(NameOf(cadenaConexion))
            End If
            _cadenaConexion = cadenaConexion
        End Sub

        ''' <summary>
        ''' Obtiene la configuracion del canal invocando el sp_ObtenerConfiguracionCanal.
        ''' </summary>
        Public Function ObtenerPorCanal(canalCodigo As String) As CanalConfiguracion
            If String.IsNullOrWhiteSpace(canalCodigo) Then
                Return Nothing
            End If

            Try
                ' Usamos Using para garantizar el cierre y liberacion de la conexion
                Using conn As New SqlConnection(_cadenaConexion)
                    conn.Open()

                    Using cmd As New SqlCommand("dbo.sp_ObtenerConfiguracionCanal", conn)
                        cmd.CommandType = CommandType.StoredProcedure

                        ' Parametro tipado para evitar inyeccion sql
                        Dim paramCanal As New SqlParameter("@CanalCodigo", SqlDbType.VarChar, 10)
                        paramCanal.Value = canalCodigo.Trim()
                        cmd.Parameters.Add(paramCanal)

                        Using reader As SqlDataReader = cmd.ExecuteReader(CommandBehavior.SingleRow)
                            If reader.Read() Then
                                Dim config As New CanalConfiguracion() With {
                                    .CanalCodigo = reader("CanalCodigo").ToString(),
                                    .CanalNombre = reader("CanalNombre").ToString(),
                                    .ServicioUrl = reader("ServicioUrl").ToString(),
                                    .ApiKey = reader("ApiKey").ToString(),
                                    .MuestraMenu = Convert.ToBoolean(reader("MuestraMenu")),
                                    .PaginaDestino = reader("PaginaDestino").ToString(),
                                    .TimeoutSegundos = Convert.ToInt32(reader("TimeoutSegundos")),
                                    .Activo = Convert.ToBoolean(reader("Activo"))
                                }
                                Return config
                            End If
                        End Using
                    End Using
                End Using

            Catch ex As SqlException
                Logger.LogError($"Error de SQL Server al consultar configuración del canal '{canalCodigo}'", ex)
                Throw
            Catch ex As Exception
                Logger.LogError($"Error inesperado al consultar configuración del canal '{canalCodigo}'", ex)
                Throw
            End Try

            Return Nothing
        End Function
    End Class
End Namespace
