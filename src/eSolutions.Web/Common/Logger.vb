Imports System
Imports System.IO
Imports System.Text
Imports System.Web

Namespace Common
    ''' <summary>
    ''' Proveedor de logging seguro y controlado para la aplicación.
    ''' Escribe en App_Data/logs para evitar exposición vía web en IIS.
    ''' </summary>
    Public Class Logger
        Private Shared ReadOnly _syncLock As New Object()

        Private Shared Function ObtenerRutaLog() As String
            Try
                Dim rutaBase As String = ""
                If HttpContext.Current IsNot Nothing AndAlso HttpContext.Current.Server IsNot Nothing Then
                    rutaBase = HttpContext.Current.Server.MapPath("~/App_Data/logs")
                Else
                    rutaBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "logs")
                End If

                If Not Directory.Exists(rutaBase) Then
                    Directory.CreateDirectory(rutaBase)
                End If

                Dim nombreArchivo As String = $"log_{DateTime.UtcNow:yyyyMMdd}.log"
                Return Path.Combine(rutaBase, nombreArchivo)
            Catch
                Return Path.Combine(Path.GetTempPath(), "eSolutions_fallback.log")
            End Try
        End Function

        Public Shared Sub LogInfo(mensaje As String, Optional correlationId As String = Nothing)
            EscribirEntrada("INFO", mensaje, Nothing, correlationId)
        End Sub

        Public Shared Sub LogWarning(mensaje As String, Optional correlationId As String = Nothing)
            EscribirEntrada("WARN", mensaje, Nothing, correlationId)
        End Sub

        Public Shared Sub LogError(mensaje As String, ex As Exception, Optional correlationId As String = Nothing)
            EscribirEntrada("ERROR", mensaje, ex, correlationId)
        End Sub

        Private Shared Sub EscribirEntrada(nivel As String, mensaje As String, ex As Exception, correlationId As String)
            Try
                If String.IsNullOrEmpty(correlationId) Then
                    correlationId = Guid.NewGuid().ToString("N").Substring(0, 8)
                End If

                Dim sb As New StringBuilder()
                sb.AppendLine("================================================================================")
                sb.AppendLine($"FECHA UTC:      {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}")
                sb.AppendLine($"NIVEL:          {nivel}")
                sb.AppendLine($"CORRELATION ID: {correlationId}")
                sb.AppendLine($"MENSAJE:        {mensaje}")

                If HttpContext.Current IsNot Nothing AndAlso HttpContext.Current.Request IsNot Nothing Then
                    Try
                        sb.AppendLine($"URL:            {HttpContext.Current.Request.HttpMethod} {HttpContext.Current.Request.RawUrl}")
                        sb.AppendLine($"IP REMOTA:      {HttpContext.Current.Request.UserHostAddress}")
                    Catch
                    End Try
                End If

                If ex IsNot Nothing Then
                    sb.AppendLine($"TIPO ERROR:     {ex.GetType().FullName}")
                    sb.AppendLine($"DETALLE:        {ex.Message}")
                    sb.AppendLine($"STACK TRACE:    {ex.StackTrace}")

                    Dim inner As Exception = ex.InnerException
                    Dim profundidad As Integer = 1
                    While inner IsNot Nothing
                        sb.AppendLine($"INNER EXCEPTION [{profundidad}]: {inner.GetType().FullName}: {inner.Message}")
                        sb.AppendLine($"INNER TRACE [{profundidad}]:    {inner.StackTrace}")
                        inner = inner.InnerException
                        profundidad += 1
                    End While
                End If
                sb.AppendLine("================================================================================")

                SyncLock _syncLock
                    Dim rutaArchivo As String = ObtenerRutaLog()
                    File.AppendAllText(rutaArchivo, sb.ToString(), Encoding.UTF8)
                End SyncLock
            Catch
                ' Los fallos de logging jamás deben tumbar la aplicación
            End Try
        End Sub
    End Class
End Namespace
