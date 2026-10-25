<%@ Page Title="Detalle de Empresa (Canal CJ)" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="DetalleEmpresa.aspx.vb" Inherits="eSolutions.Web.DetalleEmpresa" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
        <div class="col-12 col-md-8 col-lg-7">

            <div class="card shadow-sm border-0 rounded-3 overflow-hidden">
                <div class="card-header bg-secondary text-white p-4">
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="badge bg-light text-dark fw-bold">Canal CJ: Cajas y Ventanilla</span>
                        <span class="small text-white-50">Consulta de Empresa</span>
                    </div>
                    <h3 class="card-title fw-bold mt-2 mb-0">Ficha Informativa de Servicio</h3>
                </div>

                <div class="card-body p-4">
                    <div class="d-flex align-items-center gap-4 mb-4 pb-4 border-bottom">
                        <asp:Image ID="imgLogoEmpresa" runat="server" CssClass="rounded-3 border p-2 bg-light" Width="80" Height="80" />
                        <div>
                            <h4 class="fw-bold mb-1"><asp:Literal ID="litNombreEmpresa" runat="server" /></h4>
                            <div class="text-muted">Identificador de Servicio: <strong>#<asp:Literal ID="litIdEmpresa" runat="server" /></strong></div>
                            <span class="badge bg-success-subtle text-success border border-success-subtle mt-1">Convenio de Recaudación Vigente</span>
                        </div>
                    </div>

                    <h5 class="fw-bold mb-3">Parámetros Operativos del Canal CJ</h5>
                    <ul class="list-group list-group-flush mb-4 border rounded-3">
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <span>Tipo de Recaudación Habilitada:</span>
                            <span class="fw-semibold">En Línea / Tiempo Real</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <span>Comisión por Ventanilla:</span>
                            <span class="text-success fw-semibold">Exenta de Recargo</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <span>Horario de Liquidación:</span>
                            <span class="text-muted">Corte diario 18:00 hrs</span>
                        </li>
                    </ul>

                    <div class="d-grid gap-2">
                        <asp:LinkButton ID="btnRegresar" runat="server" CssClass="btn btn-secondary py-2 fw-semibold" OnClick="btnRegresar_Click">
                            <i class="bi bi-arrow-left me-1"></i> Regresar al Catálogo del Canal CJ
                        </asp:LinkButton>
                    </div>
                </div>
            </div>

            <div class="mt-4 p-3 bg-light rounded-3 border text-center small text-muted">
                <i class="bi bi-info-circle me-1"></i>
                Página de destino configurada específicamente para el canal <strong>CJ</strong> en la tabla <code>CanalConfiguracion</code>.
            </div>

        </div>
    </div>
</asp:Content>
