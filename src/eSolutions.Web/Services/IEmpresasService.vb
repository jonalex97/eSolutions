Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports eSolutions.Web.Models

Namespace Services
    ''' <summary>
    ''' Interfaz para el servicio de empresas.
    ''' </summary>
    Public Interface IEmpresasService
        Function ObtenerEmpresasAsync(config As CanalConfiguracion) As Task(Of List(Of EmpresaModel))
    End Interface
End Namespace
